// =========================================
// AUTHOR: Juraj Joscak
// DATE:   04.08.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CDropdownWindow : MonoBehaviour, IInitializable, IConstructable, IEscapable
	{
		[SerializeField] private CUiComponentText _selectedText;
		[SerializeField] private CUiButton _closeButton;
		[SerializeField] private CUiButton _backgroundButton;
		[SerializeField] private CDropdownItem _dropdownItemPrefab;
		[SerializeField] private float paddingTop;
		[SerializeField] private float paddingBottom;
		[SerializeField] private float spacing;

		private ITranslation _translation;
		private CEventSystem _eventSystem;
		private CEscapeHandler _escapeHandler;
		private CAldaInstantiator _instantiator;
		private DiContainer _diContainer;
		
		private int _currentSelectedValue;
		private CDropdownItemData[] _choices;
		private List<CDropdownItem> _dropdownItems;
		private CInputLock _inputLock;
		private RectTransform _rectTransform;

		[Inject]
		private void Inject(ITranslation translation, CEventSystem eventSystem, CEscapeHandler escapeHandler, CAldaInstantiator instantiator, DiContainer diContainer)
		{
			_translation = translation;
			_eventSystem = eventSystem;
			_escapeHandler = escapeHandler;
			_instantiator = instantiator;
			_diContainer = diContainer;
		}
		
		public void Construct()
		{
			_inputLock = new("DropdownWindow", EInputLockLayer.Dropdown, new[] { transform }, false);
			_dropdownItems = new List<CDropdownItem>();
			_rectTransform = (RectTransform)transform;
		}

		public void Initialize()
		{
			_closeButton.AddClickListener(OnCloseButton);
			_backgroundButton.AddClickListener(OnCloseButton);
		}

		public void Open()
		{
			gameObject.SetActiveObject(true);
			_eventSystem.AddInputLocker(_inputLock);
			_escapeHandler.RegisterEscapable(this);
		}
		
		public void OnEscape()
		{
			Close();
		}

		public void Close()
		{
			_escapeHandler.UnregisterEscapable(this);
			_eventSystem.RemoveInputLocker(_inputLock);
			gameObject.SetActiveObject(false);
		}
		
		public void SetSelectedValue(int value)
		{
			_currentSelectedValue = value;
			CDropdownItemData choice = _choices.First(choice => choice.Id.Equals(_currentSelectedValue));
			_selectedText.SetValue(_translation.GetText(choice.LangKey));
			RepaintChoices();
		}
		
		public void SetChoices(CDropdownItemData[] choices)
		{
			_choices = choices;

			for (int i = 0; i < _choices.Length; i++)
			{
				if (_dropdownItems.Count == i)
				{
					SpawnNewItem(i);
				}
				
				_dropdownItems[i].Set(choices[i], choices[i].Id == _currentSelectedValue);
			}
			
			for(int i = choices.Length; i < _dropdownItems.Count; i++)
			{
				_dropdownItems[i].gameObject.SetActiveObject(false);
			}

			SetSize(choices.Length);
		}

		private void RepaintChoices()
		{
			for (int i = 0; i < _dropdownItems.Count; i++)
			{
				if(!_dropdownItems[i].gameObject.activeSelf)
					continue;
				
				_dropdownItems[i].Set(_choices[i], _choices[i].Id == _currentSelectedValue);
			}
		}
		
		private void SpawnNewItem(int index)
		{
			CDropdownItem newItem = _instantiator.Instantiate(_dropdownItemPrefab, transform, _diContainer);
			RectTransform rectTransform = (RectTransform)newItem.transform;
			float verticalPosition = paddingTop + index * (spacing + rectTransform.rect.height);
			rectTransform.localPosition = new Vector3(0, -verticalPosition, 0);
			_dropdownItems.Add(newItem);
		}

		private void SetSize(int itemCount)
		{
			float itemHeight = ((RectTransform)_dropdownItems[0].transform).rect.height;
			float size = paddingTop + itemCount * (spacing + itemHeight) + paddingBottom - spacing;
			_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
		}
		
		private void OnCloseButton()
		{
			Close();
		}
	}
}