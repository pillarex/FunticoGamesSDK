using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FunticoGamesSDK.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class TabSwitcher : MonoBehaviour
    {
        public event Action<int> OnSwitchTab;

        [SerializeField] private List<Button> _tabs;
        [SerializeField] private TMP_FontAsset _regularFont;
        [SerializeField] private Color _regularColor = Color.gray;
        [SerializeField] private TMP_FontAsset _selectedFont;
        [SerializeField] private Color _selectedColor = Color.white;
        [SerializeField] private RectTransform _switcherObject;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private LayoutGroup _layoutGroup;

        private CancellationTokenSource _animCts;

        public void Init()
        {
            if (_layoutGroup != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_layoutGroup.transform);
            }

            for (int i = 0; i < _tabs.Count; i++)
            {
                int iLocal = i;
                _tabs[i].onClick.AddListener(() => SwitchTab(iLocal));
            }
        }

        public void SwitchTab(int tabIndex, bool quite = false, bool immediately = false)
        {
            if (tabIndex < 0 || tabIndex >= _tabs.Count) return;
            Button tab = _tabs[tabIndex];

            // Cancel any running animation
            if (_animCts != null)
            {
                _animCts.Cancel();
                _animCts.Dispose();
            }
            _animCts = new CancellationTokenSource();

            if (immediately)
            {
                var pos = _switcherObject.localPosition;
                pos.x = tab.transform.localPosition.x;
                _switcherObject.localPosition = pos;
            }
            else
            {
                AnimateSwitcher(tab.transform.localPosition.x, _animCts.Token).Forget();
            }

            for (int i = 0; i < _tabs.Count; i++)
            {
                var text = _tabs[i].GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    bool isSelected = i == tabIndex;
                    if (isSelected && _selectedFont != null) text.font = _selectedFont;
                    else if (!isSelected && _regularFont != null) text.font = _regularFont;
                    text.color = isSelected ? _selectedColor : _regularColor;
                }
            }

            if (!quite) OnSwitchTab?.Invoke(tabIndex);
        }

        private async UniTaskVoid AnimateSwitcher(float targetX, CancellationToken token)
        {
            float duration = 0.2f;
            float elapsed = 0f;
            float startX = _switcherObject.localPosition.x;
            
            while (elapsed < duration)
            {
                if (token.IsCancellationRequested) return;
                
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Ease out quad
                t = t * (2f - t);
                
                var pos = _switcherObject.localPosition;
                pos.x = Mathf.Lerp(startX, targetX, t);
                _switcherObject.localPosition = pos;
                
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
            
            var finalPos = _switcherObject.localPosition;
            finalPos.x = targetX;
            _switcherObject.localPosition = finalPos;
        }

        public void SetInteractable(bool isInteractable)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = isInteractable;
            }
        }
        
        private void OnDestroy()
        {
            if (_animCts != null)
            {
                _animCts.Cancel();
                _animCts.Dispose();
            }
        }
    }
}
