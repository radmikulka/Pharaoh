// =========================================
// NAME: Marek Karaba
// DATE: 02.10.2025
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using TycoonBuilder.Configs;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	[RequireComponent(typeof(CUiButton))]
	public class CTycoonUiButton : ValidatedMonoBehaviour, IAldaFrameworkComponent
	{
		[SerializeField] private CUiComponentImage _buttonImage;
		[SerializeField, Self] private CUiButton _button;

		private CUiGlobalsConfig _globalsConfig;
		
		private Sprite _originalSprite;
		private EButtonSize _originalSize;

		public void AddPointerDownListener(Action action) => _button.AddPointerDownListener(action);
		public void AddPointerUpListener(Action action) => _button.AddPointerUpListener(action);
		public void AddClickListener(Action action) => _button.AddClickListener(action);
		public void RemoveClickListener(Action action) => _button.RemoveClickListener(action);
		public void RemovePointerDownListener(Action action) => _button.RemovePointerDownListener(action);
		public void RemovePointerUpListener(Action action) => _button.RemovePointerUpListener(action);
		public void SetInteractable(bool isInteractable) => _button.SetInteractable(isInteractable);
		
		[Inject]
		public void Inject(CUiGlobalsConfig globalsConfig)
		{
			_globalsConfig = globalsConfig;
		}

		public void ToggleState(bool active, bool toggleInteractable = true)
		{
			TryGetOriginalSprite();
			
			if (active)
			{
				_buttonImage.SetSprite(_originalSprite);
				if (toggleInteractable)
				{
					_button.SetInteractable(true);
				}
			}
			else
			{
				Sprite inactiveSprite = _globalsConfig.GetInactiveSprite(_originalSize);
				_buttonImage.SetSprite(inactiveSprite);
				if (toggleInteractable)
				{
					_button.SetInteractable(false);
				}
			}
		}
		
		public void ResetOriginalSprite()
		{
			_originalSprite = _buttonImage.Sprite;
			_originalSize = GetButtonSize();
		}

		private void TryGetOriginalSprite()
		{
			if (_originalSprite)
				return;
			
			ResetOriginalSprite();
		}

		private EButtonSize GetButtonSize()
		{
			string spriteName = _originalSprite.name;
			if (spriteName.EndsWith("S"))
				return EButtonSize.S;
			
			if (spriteName.EndsWith("M"))
				return EButtonSize.M;
			
			if (spriteName.EndsWith("L"))
				return EButtonSize.L;
			
			return EButtonSize.None;
		}
	}
}