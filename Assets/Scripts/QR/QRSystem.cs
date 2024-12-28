using System.Threading;
using UniversalCamera;
using UnityEngine;
using ZXing;
using ZXing.QrCode;

public enum EResultMode
{
    Update,
    ChangeValue
}

public class QRSystem : MonoBehaviour
{
    public static QRSystem Instance { get; private set; }

    public delegate void ScanFinishResultDelegate(string result);

    public ScanFinishResultDelegate ScanFinishResult;

    [Tooltip("ChangeValue : 이전 결과와 다를 경우에만 호출합니다.")]
    public EResultMode resultMode = EResultMode.ChangeValue;

    [Tooltip("스캔할 때 딜레이를 적용하여 성능 퍼포먼스를 향상시킵니다.")]
    public float ScanDelay = 0.2f;

    public bool CanTracking { get; set; } = true;

    private string _result;
    private IBarcodeReader _barcodeReader;
    private CancellationTokenSource _cts;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
    }

    private void Start()
    {
        _barcodeReader = new BarcodeReader();
        _cts = new CancellationTokenSource();
        StartQRCodeScanning(); // 비동기 작업 시작
    }

    private void OnDestroy()
    {
        _cts?.Cancel(); // CancellationToken을 통해 비동기 작업 중단
        _cts?.Dispose();
    }

    private async void StartQRCodeScanning()
    {
        while (!_cts.IsCancellationRequested)
        {
            if (CanTracking) 
                ProcessQRCodeScanning(_cts.Token); // QR 스캔 프로세스 호출

            await Awaitable.WaitForSecondsAsync(ScanDelay); // 딜레이
        }
    }

    private async void ProcessQRCodeScanning(CancellationToken token)
    {
        WebCamTexture cameraTexture = CameraBackground.Instance.CameraBackgroundTexture;

        if (!cameraTexture)
            return;

        // Unity 메인 스레드에서 픽셀 데이터 가져오기
        await Awaitable.MainThreadAsync();
        token.ThrowIfCancellationRequested();

        var pixelData = cameraTexture.GetPixels32();

        // 별도 쓰레드에서 디코딩 수행
        await Awaitable.BackgroundThreadAsync();

        var result = _barcodeReader.Decode(pixelData, cameraTexture.width, cameraTexture.height);

        if (result != null)
        {
            // 결과값에 따른 로직 처리 (Unity 메인 스레드에서 실행)
            await Awaitable.MainThreadAsync();
            // token.ThrowIfCancellationRequested();

            if (resultMode == EResultMode.Update)
                ScanFinishResult?.Invoke(result.Text);
            else if (resultMode == EResultMode.ChangeValue && _result != result.Text)
                ScanFinishResult?.Invoke(result.Text);

            _result = result.Text;
        }
    }

    public static Color32[] GenerateQR(string textForEncoding, int width, int height)
    {
        BarcodeWriter writer = new()
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = height,
                Width = width
            }
        };

        return writer.Write(textForEncoding);
    }

    public void Reset()
    {
        _result = string.Empty;
    }
}