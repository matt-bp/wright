using System;
using System.Collections.Generic;
using System.Linq;
using Events;
using GrabTool.Mesh;
using Models.Local;
using TMPro;
using UnityEngine;
using Wright.Library.Analysis;
using Wright.Library.Logging;
using Wright.Library.Messages;
using MeshCopier = Wright.Library.Mesh.MeshCopier;

namespace Presenters
{
    public class LevelPresenter : MonoBehaviour
    {
        [Header("View")] [SerializeField] private TMP_Text timeLabel;
        [SerializeField] private MeshFilter currentCloth;
        [SerializeField] private MeshFilter goalMesh;
        [SerializeField] private TMP_Text statusLabel;

        [Header("Model")] [SerializeField] private TaskResultModel taskResultModel;
        [SerializeField] private GoalMeshModel goalMeshModel;
        [SerializeField] private GoalMeshDataModel goalDataModel;
        [SerializeField] private TaskRunnerModel taskRunner;
        [SerializeField] private DataExportModel dataModel;

        private void OnEnable()
        {
            MDebug.Log("Enable");
            Messenger<int>.AddListener(ModelToPresenter.CURRENT_TASK, OnReceivedTask);
        }

        private void OnDisable()
        {
            MDebug.Log("Disable");
            Messenger<int>.RemoveListener(ModelToPresenter.CURRENT_TASK, OnReceivedTask);
        }

        private void OnReceivedTask(int task)
        {
            MDebug.Log($"Going to grab task {task}.");

            goalDataModel.LoadFromDisk(task);

            dataModel.task = task;
        }

        private void Start()
        {
            goalDataModel.OnMeshesFound += HandleGoalMeshesFound;
            goalDataModel.OnMeshesMissing += HandleGoalMeshesMissing;
            taskResultModel.OnTimeUpdate += OnTimeUpdate;
        }

        private void HandleGoalMeshesFound(Dictionary<int, Mesh> meshes)
        {
            goalMeshModel.GoalMeshes = meshes;

            if (!goalMeshModel.GoalMeshes.Any())
            {
                statusLabel.text = "Handle found but none passed";
            }

            NextGoalMesh();
        }

        private void HandleGoalMeshesMissing()
        {
            const string message = "Goal meshes are missing :(";
            statusLabel.text = message;
            MDebug.Log(message);
        }

        private void OnTimeUpdate(float t)
        {
            timeLabel.text = $"Time is now: {TimeSpan.FromSeconds(t):mm\\:ss}";
        }

        private void NextGoalMesh()
        {
            taskRunner.ProgressToNextMesh();
        }

        public void UpdateCurrentGoalMesh((int current, int total) value)
        {
            statusLabel.text = $"Found {value.current}";

            if (value.current == 0)
            {
                MDebug.Log("First, set cloth to current goal mesh and get another one!");

                var newPositions = taskRunner.CurrentMesh.vertices.Select(currentCloth.transform.InverseTransformPoint).ToArray();

                var clothCollider = currentCloth.gameObject.GetComponent<MeshCollider>();
                
                MeshUpdater.UpdateMeshes(currentCloth.sharedMesh, clothCollider, newPositions);
                
                // For each results, we have a padded one at the beginning. I guess that will be a good check, the 
                // error should always be zero on that one.
                OnSubmitClicked();

                return;
            }

            goalMesh.sharedMesh = taskRunner.CurrentMesh;
        }

        public void TaskComplete()
        {
            statusLabel.text = "Task complete.";
            dataModel.SaveResults();

            Messenger.Broadcast(PresenterToModel.TASK_COMPLETE);
        }

        public void HandleTaskRunnerNoMeshes()
        {
            const string message = "GoalMeshModel not populated.";
            statusLabel.text = message;
            MDebug.Log(message);
        }

        private void Update()
        {
            taskResultModel.AddTime(Time.deltaTime);
        }

        public void OnSubmitClicked()
        {
            var copy = MeshCopier.MakeCopy(currentCloth.sharedMesh);
            // Convert to world space to make comparison
            copy.vertices = copy.vertices.Select(v => currentCloth.transform.TransformPoint(v)).ToArray();
            
            // Recalculating normals because the mesh draggers aren't doing this
            copy.RecalculateNormals();
            taskRunner.CurrentMesh.RecalculateNormals();
            
            var distanceError = DistanceError.GetError(taskRunner.CurrentMesh, copy);
            
            var normalError = NormalError.GetError(taskRunner.CurrentMesh, copy);

            // Save add generated mesh to the data model
            // Also save out goal mesh (just in case)
            taskResultModel.AddUserGeneratedMesh(taskRunner.CurrentKeyframe, copy, distanceError, normalError);

            MDebug.Log("Added to task result model!");

            NextGoalMesh();
        }
    }
}