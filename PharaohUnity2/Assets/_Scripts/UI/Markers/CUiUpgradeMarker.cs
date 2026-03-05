// =========================================
// AUTHOR: Juraj Joscak
// DATE:   22.10.2025
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
	public enum EUpgradeMarkerState
	{
		None = 0,
		Locked = 1,
		Unlocked = 2,
		Available = 3,
		Running = 4,
		Completed = 5,
	}
	
	public interface IUpgradeMarkerValueSource
	{
		EUpgradeMarkerState GetUpgradeMarkerState();
	}

	public class CUiUpgradeMarker : ValidatedMonoBehaviour, IConstructable, IInitializable
	{
		[SerializeField, Child] private CTweener _tweener;
		[SerializeField, Self] private CUiComponentGraphicGroup _graphicGroup;
		[SerializeField] private CUiComponentImage[] _glow;
		[SerializeField] private CUiComponentImage _icon;
		[SerializeField] private Sprite _availableIconSprite;
		[SerializeField] private Sprite _completedIconSprite;
		
		private CUiGlobalsConfig _uiGlobalsConfig;
		
		private IUpgradeMarkerValueSource _valueSource;
		private bool _visible = true;
		
		[Inject]
		private void Inject(CUiGlobalsConfig uiGlobalsConfig)
		{
			_uiGlobalsConfig = uiGlobalsConfig;
		}
		
		public void Construct()
		{
			_valueSource = GetComponentInParent<IUpgradeMarkerValueSource>();
		}
		
		public void Initialize()
		{
			SetInvisible();
			_tweener.Disable();
			ShowGlow(false);
		}

		private void Update()
		{
			EUpgradeMarkerState state = _valueSource.GetUpgradeMarkerState();
			SetCorrectIcon(state);
			
			switch (state)
			{
				case EUpgradeMarkerState.Running:
				case EUpgradeMarkerState.Locked:
					SetLocked();
					break;
				case EUpgradeMarkerState.Unlocked:
					SetUnlocked();
					break;
				case EUpgradeMarkerState.Completed:
				case EUpgradeMarkerState.Available:
					SetAvailable();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void SetCorrectIcon(EUpgradeMarkerState state)
		{
			switch (state)
			{
				case EUpgradeMarkerState.Completed:
					_icon.SetSprite(_completedIconSprite);
					_icon.Component.material = null;
					return;
				case EUpgradeMarkerState.Unlocked:
					_icon.SetSprite(_availableIconSprite);
					_icon.Component.material = _uiGlobalsConfig.GrayScaleMaterial;
					return;
				default:
					_icon.SetSprite(_availableIconSprite);
					_icon.Component.material = null;
					return;
			}
		}

		private void SetLocked()
		{
			SetInvisible();
			_tweener.Disable();
			ShowGlow(false);
		}

		private void SetUnlocked()
		{
			SetVisible();
			_tweener.Disable();
			ShowGlow(false);
		}

		private void SetAvailable()
		{
			SetVisible();
			_tweener.Enable();
			ShowGlow(true);
		}

		private void SetVisible()
		{
			if(_visible)
				return;
			
			_visible = true;
			_graphicGroup.Enable();
		}

		private void SetInvisible()
		{
			if(!_visible)
				return;
			
			_visible = false;
			_graphicGroup.Disable();
		}

		private void ShowGlow(bool state)
		{
			foreach (CUiComponentImage glow in _glow)
			{
				glow.gameObject.SetActiveObject(state);
			}
		}
	}
}