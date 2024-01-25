using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Models.Global;
using UnityEngine;
using Wright.Library.File;
using Wright.Library.Study;
using static Models.Local.GoalMeshCreationDataModel;

namespace Models.Local
{
    [RequireComponent(typeof(TaskResultModel))]
    public class DataExportModel : MonoBehaviour
    {
        public int task = -1;
        public InputMethod inputMethod;

        public void SaveResults()
        {
            var model = GetComponent<TaskResultModel>();
            var participantModel = GameObject.FindWithTag("Global Models").GetComponent<ParticipantModel>();

            var overallData = new Dictionary<string, object>
            {
                { "type", "result" },
                { "num_meshes", model.UserGeneratedMeshes.Count },
                { "time", model.ElapsedTime },
                { "task", task },
                { "inputMethod", inputMethod.ToString() },
                { "participant_number", participantModel.ParticipantNumber }
            };

            foreach (var (key, result) in model.UserGeneratedMeshes)
            {
                var goalMesh = new Dictionary<string, object>
                {
                    { "distance_error", result.DistanceError },
                    { "normal_error", result.NormalError },
                    { "verts", result.Mesh.vertices.Select(VectorToTuple).ToArray() },
                    { "tris", result.Mesh.triangles }
                };

                overallData.Add(MakeGoalKey(key), goalMesh);
            }

            var subDirectory = $"p{participantModel.ParticipantNumber}_results";
            var directory = Path.Combine(Application.persistentDataPath, subDirectory);
            Directory.CreateDirectory(directory);
            
            var id = Guid.NewGuid();
            Debug.Log($"Id is {id}");
            var filename = $"new_results_{id}.json";

            DictionaryFileHelper.WriteToFile(overallData, $"{subDirectory}\\{filename}");
        }
    }
}