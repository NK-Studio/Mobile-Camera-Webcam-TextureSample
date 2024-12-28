using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace QR.Test
{
    public class QRTest02 : MonoBehaviour
    {
        public string QRData = "Hello World";
        private UIDocument _document;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        private void Start()
        {
            // QR UI 찾기
            var root = _document.rootVisualElement;
            var qrImage = root.Q("qr__image");

            // QR 코드 컬러 데이터 생성
            var qrPixel = QRSystem.GenerateQR(QRData, 256, 256);
            
            // QR 코드 컬러 데이터를 텍스쳐로 변환
            var texture = new Texture2D(256, 256);
            texture.SetPixels32(qrPixel);
            texture.Apply();
            
            // UI에 QR 코드 이미지 표시
            qrImage.style.backgroundImage = new StyleBackground(texture);
        }
    }
}