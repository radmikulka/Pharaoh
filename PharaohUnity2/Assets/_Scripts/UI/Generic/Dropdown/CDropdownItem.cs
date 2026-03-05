// =========================================
// AUTHOR: Juraj Joscak
// DATE:   30.07.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CDropdownItem : MonoBehaviour, IConstructable, IInitializable
	{
		[SerializeField] private CUiComponentText _text;
		[SerializeField] private CUiComponentImage _checkmark;

		private ITranslation _translation;
		
		private CUiButton _button;
		private CUiSpriteSwapper _swapper;
		private CDropdownSelector _parentSelector;

		private int _id;

		[Inject]
		private void Inject(ITranslation translation)
		{
			_translation = translation;
		}
		
		public void Construct()
		{
			_button = GetComponent<CUiButton>();
			_swapper = GetComponentInChildren<CUiSpriteSwapper>(true);
			_parentSelector = GetComponentInParent<CDropdownSelector>(true);
		}

		public void Initialize()
		{
			_button.AddClickListener(OnClick);
		}
		
		public void Set(CDropdownItemData data, bool isSelected)
		{
			gameObject.SetActiveObject(true);
			_id = data.Id;
			_text.SetValue(_translation.GetText(data.LangKey));
			_checkmark.SetAlpha(isSelected ? 1f : 0f);
			_swapper.SetSprite(isSelected ? 1 : 0);
		}

		private void OnClick()
		{
			_parentSelector.Select(_id);
		}
	}
}