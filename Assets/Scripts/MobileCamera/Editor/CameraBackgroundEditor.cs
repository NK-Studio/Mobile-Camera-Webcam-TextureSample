using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UniversalCamera.Editor
{
    [CustomEditor(typeof(CameraBackground))]
    public class CameraBackgroundEditor : UnityEditor.Editor
    {
        private VisualElement _root;

        private SerializedProperty _requestedFPSProperty;
        private SerializedProperty _cameraFacingType;
        private SerializedProperty _editorTestCameraName;

        private void FindProperty()
        {
            _requestedFPSProperty = serializedObject.FindProperty("requestedFPS");
            _cameraFacingType = serializedObject.FindProperty("cameraFacingType");
            _editorTestCameraName = serializedObject.FindProperty("editorTestCameraName");
        }

        private void InitElement()
        {
            _root = new VisualElement();

            var fpsField = new PropertyField(_requestedFPSProperty);
            fpsField.Bind(serializedObject);
            _root.Add(fpsField);

            var cameraFacingField = new PropertyField(_cameraFacingType);
            cameraFacingField.Bind(serializedObject);
            _root.Add(cameraFacingField);

            var cameraListField = new DropdownField();
            cameraListField.label = "Target Camera";
            cameraListField.AddToClassList("unity-base-field__aligned");
            
            List<string> cameraList = new List<string>();
            foreach (var device in WebCamTexture.devices)
                cameraList.Add(device.name);

            foreach (var device in cameraList)
                cameraListField.choices.Add(device);

            cameraListField.index = CalculateTargetCamera(cameraList);
            _editorTestCameraName.stringValue = cameraList[cameraListField.index];
            serializedObject.ApplyModifiedProperties();
            
            cameraListField.RegisterValueChangedCallback(evt =>
            {
                var newTargetCamera = evt.newValue;
                _editorTestCameraName.stringValue = newTargetCamera;
                EditorPrefs.SetString("EditorTestCameraName", newTargetCamera);
                serializedObject.ApplyModifiedProperties();
            });

            _root.Add(cameraListField);
        }

        private int CalculateTargetCamera(List<string> cameraList)
        {
            var targetCamera = EditorPrefs.GetString("EditorTestCameraName");

            if (string.IsNullOrEmpty(targetCamera))
                return 0;

            var targetCameraIndex = 0;
            for (var i = 0; i < cameraList.Count; i++)
            {
                if (cameraList[i] == targetCamera)
                {
                    targetCameraIndex = i;
                    break;
                }
            }

            return targetCameraIndex;
        }

        public override VisualElement CreateInspectorGUI()
        {
            FindProperty();
            InitElement();

            return _root;
        }
    }
}