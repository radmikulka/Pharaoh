// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System;
using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
    public class CUiLoadingScreen : MonoBehaviour, IConstructable, IInitializable, ILoadingScreen
    {
        [SerializeField] private CLoadingProgressBar _progressBar;
        [SerializeField] private CUiComponentText _infoText;
        [SerializeField] private CanvasGroup _canvasGroup;

        private CLoadingScreenProxy _loadingScreen;
        private ITranslation _translation;
        
        [Inject]
        private void Inject(CLoadingScreenProxy loadingScreen, ITranslation translation)
        {
            _loadingScreen = loadingScreen;
            _translation = translation;
        }

        public void Construct()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = false;
        }

        public void Initialize()
        {
            _loadingScreen.RegisterInstance(this);
            SetInfoText(string.Empty, false);
            SetActiveProgressBar(false);
        }

        public void SetInfoText(string text, bool localized)
        {
            string content = localized ? _translation.GetText(text) : text;
            _infoText.SetValue(content);
        }

        public void UpdateProgressBar(float progress)
        {
            _progressBar.UpdateProgress(progress);
        }

        public void SetActiveProgressBar(bool active)
        {
            _progressBar.SetActive(active);
        }

        public async UniTask Show(CancellationToken ct, float duration)
        {
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            if (duration <= 0f)
            {
                _canvasGroup.alpha = 1f;
                return;
            }
            await _canvasGroup.DOFade(1f, duration * (1f - _canvasGroup.alpha));
            await UniTask.NextFrame(ct);
        }

        public async UniTask Hide(CancellationToken ct, float duration)
        {
            if (duration <= 0f)
            {
                _canvasGroup.alpha = 0f;
            }
            else
            {
                await _canvasGroup.DOFade(0f, duration * _canvasGroup.alpha);
            }
            
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }
}