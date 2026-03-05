// =========================================
// AUTHOR: Juraj Joscak
// DATE:   04.07.2025
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CUiMarker : MonoBehaviour, IInitializable, IConstructable
	{
		[SerializeField] private CUiComponentText _text;
		[SerializeField] private CUiComponentImage _bg;
		[SerializeField] private GameObject _checkmark;

		private ITranslation _translation;
		
		private Transform _originalParent;
		private CTweener _tweener;
		
		[Inject]
		private void Inject(ITranslation translation)
		{
			_translation = translation;
		}

		public void Construct()
		{
			_tweener = GetComponentInChildren<CTweener>(true);
		}
		
		public void Initialize()
		{
			_originalParent = transform.parent;
		}
		
		public void Set(RectTransform target, bool pulse, bool isMaskable = true)
		{
			transform.SetParent(target, false);
			transform.localPosition = Vector3.zero;
			gameObject.SetActive(true);
			SetMaskable(isMaskable);

			if (pulse)
			{
				_tweener.Enable();
			}
			else
			{
				_tweener.Disable();
			}
		}

		private void SetMaskable(bool isMaskable)
		{
			if (_text.Component.maskable != isMaskable)
			{
				_text.Component.maskable = isMaskable;
				_text.Component.SetMaterialDirty();
				_text.Component.RecalculateClipping();
			}

			if (_bg.Component.maskable != isMaskable)
			{
				_bg.Component.maskable = isMaskable;
				_bg.Component.SetMaterialDirty();
				_bg.Component.RecalculateClipping();
			}
		}

		public void Disable()
		{
			_tweener.Disable();
			
			transform.SetParent(_originalParent, false);
			gameObject.SetActive(false);
		}
		
		public void UpdateValue(int value, EMarkerType type, Sprite sprite)
		{
			string valueText;
			switch (type)
			{
				case EMarkerType.Number:
					valueText = value.ToString();
					break;
				case EMarkerType.ExclamationMark:
					valueText = "!";
					break;
				case EMarkerType.CheckMark:
					valueText = "";
					break;
				case EMarkerType.New:
					valueText = _translation.GetText("Generic.New");
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
			
			_checkmark.SetActiveObject(type == EMarkerType.CheckMark);
			_text.SetValue(valueText);
			_bg.SetSprite(sprite);
		}
	}
}