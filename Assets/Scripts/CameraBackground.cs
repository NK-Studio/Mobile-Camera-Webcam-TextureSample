using System;
using UnityEngine;

namespace NKStudio
{
    public class CameraBackground : MonoBehaviour
    {
        public enum CameraFacing
        {
            /// <summary>
            /// 전면
            /// </summary>
            Front,
            /// <summary>
            /// 후면
            /// </summary>
            Back
        }

        [SerializeField] private Transform planeTransform; // 3D 플랜의 Transform
        [SerializeField] private Renderer planeRenderer; // 3D 플랜의 Renderer
        
        [SerializeField, Range(0, 60)]
        [Tooltip("앱의 FPS가 아닙니다.\n카메라는 지원되는 가장 가까운 FPS를 사용합니다.\n0(0)은 기본적으로 값으로 이어집니다.")]
        private int requestedFPS = 60;

        [Tooltip("카메라의 방향을 선택합니다.")]
        [SerializeField] private CameraFacing cameraFacingType = CameraFacing.Back;

        private int _frontFacingCameraIndex;

        private WebCamTexture _webCamTextureTarget;
        private WebCamTexture _webCamTextureBack;
        private WebCamTexture _webCamTextureFront;

        private WebCamDevice[] _webCamDevices;
        private WebCamDevice _webCamDeviceTarget;
        private WebCamDevice _webCamDeviceBack;
        private WebCamDevice _webCamDeviceFront;
        
        private Camera _camera;

        // Constants
        private const float PlaneDistance = 10f; // Plane과 카메라 사이의 거리
        private const float MinimumWidthForOrientation = 100f;

        /// <summary>
        /// 화면의 회전 각도
        /// </summary>
        public float VideoRotationAngle => _webCamTextureTarget.videoRotationAngle;

        private void Awake()
        {
            _camera = Camera.main;

            try
            {
                _webCamDevices = WebCamTexture.devices;

                if (_webCamDevices.Length == 0)
                    Debug.LogError("카메라 장치를 찾을 수 없습니다");
                else
                {
                    for (int i = 0; i < _webCamDevices.Length; i++)
                        if (_webCamDevices[i].isFrontFacing)
                            _frontFacingCameraIndex = i;

                    _webCamDeviceBack = _webCamDevices[0];
                    _webCamDeviceFront = _webCamDevices[_frontFacingCameraIndex];

                    switch (cameraFacingType)
                    {
                        case CameraFacing.Front:
                            _webCamTextureFront = CreateWebCamTexture(_webCamDeviceFront.name, requestedFPS);
                            _webCamDeviceTarget = _webCamDeviceFront;
                            _webCamTextureTarget = _webCamTextureFront;
                            break;
                        case CameraFacing.Back:
                            _webCamTextureBack = CreateWebCamTexture(_webCamDeviceBack.name, requestedFPS);
                            _webCamDeviceTarget = _webCamDeviceBack;
                            _webCamTextureTarget = _webCamTextureBack;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    // 웹캠 카메라 시작
                    _webCamTextureTarget.Play();

                    // 플랜에 텍스처 적용
                    planeRenderer.material.mainTexture = _webCamTextureTarget;

                    // 포지션 변경
                    transform.localPosition = new Vector3(0f, 0f, PlaneDistance);
                }
            }
            catch (Exception e)
            {
                Debug.Log("카메라를 사용할 수 없습니다:" + e);
            }
        }

        private void Update()
        {
            UpdateOrientation();
        }

        private void UpdateOrientation()
        {
            if (IsWebCamTextureInitialized())
            {
                // 전면이면 음수 처리
                var calculateAngle = VideoRotationAngle;
                if (!_webCamDeviceTarget.isFrontFacing)
                    calculateAngle = -VideoRotationAngle;

                planeTransform.localEulerAngles = new Vector3(
                    90 + calculateAngle,
                    -90,
                    90);

                // 카메라 거리에 따라 플랜 크기 조정
                float planeToCameraDistance = Vector3.Distance(transform.position, _camera.transform.position);
                float planeToCameraHeight = 2.0f * Mathf.Tan(0.5f * _camera.fieldOfView * Mathf.Deg2Rad) *
                    planeToCameraDistance / 10f;
                float planeToCameraWidth = planeToCameraHeight * _camera.aspect;

                // 세로 영상이면 가로 세로 변경
                transform.localScale = IsPortrait
                    ? new Vector3(planeToCameraHeight, 1f, planeToCameraWidth)
                    : new Vector3(planeToCameraWidth, 1f, planeToCameraHeight);
                
                // 전면 카메라면 좌우 반전(옵션)
                if (cameraFacingType == CameraFacing.Front)
                {
                    var tempScale = transform.localScale;
                    // 상하 반전
                    tempScale.x *= -1;
#if UNITY_IOS
                    // iOS에서 전면 카메라는 좌우 반전이 필요합니다.
                    tempScale.z *= -1;
#endif
                    transform.localScale = tempScale;
                }       

                // 전면 카메라면 좌우 반전(옵션)
                if (cameraFacingType == CameraFacing.Back)
                {
#if UNITY_IOS
                    var tempScale = transform.localScale;
                    tempScale.z *= -1;
                    transform.localScale = tempScale;
#endif
                }   
            }
        }

        /// <summary>
        /// 세로 영상이면 true를 반환합니다.
        /// </summary>
        public bool IsPortrait => (int)Mathf.Abs(VideoRotationAngle) == 90 || (int)Mathf.Abs(VideoRotationAngle) == 270;

        /// <summary>
        /// 웹캠 텍스처가 처음부터 재대로 렌더링되지 않기 때문에 초기화되었는지 확인합니다.
        /// </summary>
        /// <returns></returns>
        private bool IsWebCamTextureInitialized()
        {
            return _webCamTextureTarget
                   && _webCamTextureTarget.width >= MinimumWidthForOrientation;
        }

        /// <summary>
        /// 웹캠 텍스처를 생성합니다.
        /// </summary>
        /// <param name="deviceName">카메라 장치 이름</param>
        /// <param name="fps">타겟 fps</param>
        /// <returns></returns>
        private WebCamTexture CreateWebCamTexture(string deviceName, int fps)
        {
            return new WebCamTexture(deviceName, Screen.width, Screen.height, fps);
        }

        private void OnDestroy()
        {
            if (_webCamTextureTarget)
                _webCamTextureTarget.Stop();
        }
    }
}