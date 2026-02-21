// // =========================================
// // AUTHOR: Radek Mikulka
// // DATE:   06.09.2023
// // =========================================

using System;
using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using UnityEngine;
using Zenject;

namespace Pharaoh.Ui
{
    public abstract class CTycoonBuilderScreen : CBaseScreen<CTycoonBuilderScreen>, IUiScreen
    {
        private CEscapeHandler _escapeHandler;
        
        private CancellationTokenSource _cts;
        private CanvasGroup _canvasGroup;
        
        protected ICtsProvider CtsProvider;
        
        public bool IsOpened { get; private set; }
        
        [Inject]
        private void Inject(CEscapeHandler escapeHandler, ICtsProvider ctsProvider)
        {
            _escapeHandler = escapeHandler;
            CtsProvider = ctsProvider;
        }
        
        private void OnValidate()
        {
            this.ValidateRefs();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void OnEscape()
        {
            bool canBeClosedByEscape = CanBeClosedByEscape();
            if (!canBeClosedByEscape)
            {
                ProcessDeniedEscapeTrigger();
                return;
            }
            CloseThisScreen();
        }
        
        public void TryCloseThisScreen()
        {
            bool canBeClosedByEscape = CanBeClosedByEscape();
            if (!canBeClosedByEscape)
            {
                ProcessDeniedEscapeTrigger();
                return;
            }
            CloseThisScreen();
        }
        
        public void SetAlpha(float alpha)
        {
            if (!_canvasGroup)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }
            
            _cts?.Cancel();
            _cts = CancellationTokenSource.CreateLinkedTokenSource(CtsProvider.Token);
            AnimateAlpha(alpha, _cts.Token).Forget();
        }

        private async UniTask AnimateAlpha(float alpha, CancellationToken ct)
        {
            float duration = 0.15f;
            float initialAlpha = _canvasGroup.alpha;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                ct.ThrowIfCancellationRequested();
                
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _canvasGroup.alpha = Mathf.Lerp(initialAlpha, alpha, t);
                await UniTask.Yield();
            }
            _canvasGroup.alpha = alpha;
        }

        public override void OnScreenOpenStart()
        {
            base.OnScreenOpenStart();
            EventBus.Send(new CScreenOpenedSignal(this));
            _escapeHandler.RegisterEscapable(this);
            
            IsOpened = true;
        }

        public override void OnScreenCloseEnd()
        {
            base.OnScreenCloseEnd();
            EventBus.Send(new CScreenClosedSignal(this));
            _escapeHandler.UnregisterEscapable(this);
            
            IsOpened = false;
        }
    }
}