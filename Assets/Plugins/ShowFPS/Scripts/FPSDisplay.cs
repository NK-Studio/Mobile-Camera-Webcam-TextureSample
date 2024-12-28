using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Utility
{
    [RequireComponent(typeof(UIDocument))]
    public class FPSDisplay : MonoBehaviour
    {
        private UIDocument _UIDocument;
        private Label _FPSText;
        private float _FPSCount;
        private float _deltaTime;
        private readonly WaitForSeconds _waitForSeconds = new(0.25f);

        public bool ShowDebug = true;

        private void Awake()
        {
            _UIDocument = GetComponent<UIDocument>();

            VisualElement root = _UIDocument.rootVisualElement;
            _FPSText = root.Q<Label>();
        }

        private void Start()
        {
            StartCoroutine(ChangeFPS());
        }

        private void Update()
        {
            RefreshText();
            SetActiveUI(ShowDebug);
        }

        private IEnumerator ChangeFPS()
        {
            _FPSCount = Mathf.Round(1 / Time.deltaTime);
            yield return _waitForSeconds;
            StartCoroutine(ChangeFPS());
        }

        /// <summary>
        /// 텍스트를 새로고침 합니다.
        /// </summary>
        private void RefreshText()
        {
            if (_FPSText != null)
                _FPSText.text = "FPS : " + _FPSCount;
        }

        /// <summary>
        /// 인자로 들어온 값에 따라 FPS 텍스트를 보이거나 안보이게 합니다.
        /// </summary>
        /// <param name="active"></param>
        public void SetActiveUI(bool active)
        {
            gameObject.TryGetComponent(out _UIDocument);

            if (!_UIDocument)
                return;

            VisualElement root = _UIDocument.rootVisualElement;
            var fps = root.Q<Label>();
            fps.style.visibility = active ? Visibility.Visible : Visibility.Hidden;
        }
    }
}