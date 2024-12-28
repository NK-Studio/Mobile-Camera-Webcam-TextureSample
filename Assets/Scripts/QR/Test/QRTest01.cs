using UnityEngine;

namespace QR.Test
{
    public class QRTest01 : MonoBehaviour
    {
        private void Start()
        {
            QRSystem.Instance.ScanFinishResult += ScanFinishResult;
        }

        private void ScanFinishResult(string result)
        {
            NativeToast.Toast(result);
        }
    }
}