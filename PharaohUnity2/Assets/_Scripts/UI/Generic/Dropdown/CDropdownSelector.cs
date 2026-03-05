// =========================================
// AUTHOR: Juraj Joscak
// DATE:   30.07.2025
// =========================================

using System.Linq;
using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CDropdownSelector : MonoBehaviour, IInitializable, IConstructable
	{
		[SerializeField] private CUiComponentText _selectedText;
		[SerializeField] private CUiButton _openButton;

		private ITranslation _translation;
		
		private CField<int> _boundField;
		private int _currentSelectedValue;
		private CDropdownItemData[] _choices;
		private CDropdownWindow _dropdownWindow;

		[Inject]
		private void Inject(ITranslation translation)
		{
			_translation = translation;
		}

		public void Construct()
		{
			_dropdownWindow = GetComponentInChildren<CDropdownWindow>();
		}
		
		public void Initialize()
		{
			_openButton.AddClickListener(OnOpenButton);
			_dropdownWindow.gameObject.SetActiveObject(false);
		}
		
		private void OnDestroy()
		{
			if (_boundField != null)
			{
				_boundField.OnValueChanged -= SetValue;
			}
		}
		
		public void Select(int value)
		{
			_boundField.Value = value;
			_dropdownWindow.Close();
		}
		
		public void BindTo(CField<int> value, CDropdownItemData[] choices)
		{
			_choices = choices;
			_dropdownWindow.SetChoices(_choices);
			
			_boundField = value;
			_boundField.OnValueChanged += SetValue;
			SetValue(_boundField.Value);
		}
		
		private void SetValue(int value)
		{
			_currentSelectedValue = value;
			Repaint();
		}

		private void Repaint()
		{
			CDropdownItemData choice = _choices.First(choice => choice.Id.Equals(_currentSelectedValue));
			_selectedText.SetValue(_translation.GetText(choice.LangKey));
			_dropdownWindow.SetSelectedValue(_currentSelectedValue);
		}

		private void OnOpenButton()
		{
			_dropdownWindow.Open();
		}
	}

	public class CDropdownItemData
	{
		public readonly int Id;
		public readonly string LangKey;
		
		public CDropdownItemData(int id, string langKey)
		{
			Id = id;
			LangKey = langKey;
		}
	}
}