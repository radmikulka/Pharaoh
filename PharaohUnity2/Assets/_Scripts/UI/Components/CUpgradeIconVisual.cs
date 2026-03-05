// =========================================
// AUTHOR: Marek Karaba
// DATE:   03.09.2025
// =========================================

using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using KBCore.Refs;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CUpgradeIconVisual : ValidatedMonoBehaviour
	{
		[SerializeField] private GameObject _upgradeAvailableGradient;
		[SerializeField] private CUiComponentImage _upgradeIcon;
		[SerializeField] private CUiSpriteSwapper _spriteSwapper;
		[SerializeField] private CUiColorSwapper _colorSwapper;
		[SerializeField, Child] private CTweener _tweener;
		
		private bool _isEnabled;

		public void ToggleIcon(bool upgradeAvailable, bool isInHeader, bool isCompleted)
		{
			SetCorrectState(upgradeAvailable, isInHeader, isCompleted);
		}

		private void SetCorrectState(bool upgradeAvailable, bool isInHeader, bool isCompleted)
		{
			_upgradeIcon.RectTransform.anchoredPosition = Vector3.zero;
			bool active = upgradeAvailable || isCompleted;
			_upgradeAvailableGradient.SetActive(active && !isInHeader);
			if (active)
			{
				SetEnabledState(isCompleted);
				EnableTweener();
			}
			else
			{
				SetDisabledState(isInHeader);
				DisableTweener();
			}
		}

		private void EnableTweener()
		{
			if(_isEnabled)
				return;
			
			_isEnabled = true;
			_tweener.Enable();
		}

		private void DisableTweener()
		{
			if (!_isEnabled)
				return;
			
			_isEnabled = false;
			_tweener.Disable();
			_upgradeIcon.transform.localScale = Vector3.one;
		}

		private void SetEnabledState( bool isCompleted)
		{
			_spriteSwapper.SetSprite(isCompleted ? 2 : 0);
			_colorSwapper.SetColor(0);
		}

		private void SetDisabledState(bool isInHeader)
		{
			_spriteSwapper.SetSprite(1);
			_colorSwapper.SetColor(isInHeader ? 2 : 1);
		}
	}
}