using UnityEngine;

/// <summary>
/// 화면 새로 고침 빈도와 일치하도록 애플리케이션의 대상 프레임 속도를 조정합니다.
/// </summary>
public class RefreshRateFrameLimiter : MonoBehaviour
{
    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.numerator;
    }
}
