using System;
using GrabTool.Mesh;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace Presenters.VR
{
    public class VRSizeChanger : MonoBehaviour
    {
        [SerializeField] private VRMeshDragger vRMeshDragger;

        [Header("Settings")]
        [SerializeField] private float radiusChangeRate = 0.1f;
        [SerializeField] private float exponent = 1.0f;
        [SerializeField] private float minimumValue = 0.01f;
        [SerializeField] private float maximumValue = 1000.0f;

        [Header("Input")]
        [SerializeField] private InputActionProperty changeValue;
        
        private float _currentValue = 0.2f;
        
        private void OnEnable()
        {
            changeValue.EnableDirectAction();
        }

        private void OnDisable()
        {
            changeValue.DisableDirectAction();
        }

        private void Update()
        {
            var value = changeValue.action.ReadValue<Vector2>();

            _currentValue += radiusChangeRate * value.y * Time.deltaTime;
            _currentValue = Mathf.Max(minimumValue, Mathf.Min(maximumValue, _currentValue));
            
            vRMeshDragger.OnRadiusChanged(Mathf.Pow(_currentValue, exponent));
        }
    }
}