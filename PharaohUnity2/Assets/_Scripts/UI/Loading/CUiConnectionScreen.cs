// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System;
using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Pharaoh
{
    public class CUiConnectionScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;

        private void Awake()
        {
            if(CPlatform.IsEditor)
                return;
            _canvasGroup.alpha = 0f;
        }

        private void Start()
        {
            _canvasGroup.DOFade(1f, 0.2f);
        }

        private void OnDestroy()
        {
            _canvasGroup.DOKill();
        }
    }
}