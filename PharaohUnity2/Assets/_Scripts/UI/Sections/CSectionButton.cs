// =========================================
// AUTHOR: Marek Karaba
// DATE:   15.07.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using UnityEngine;

namespace TycoonBuilder.Ui
{
	[RequireComponent(typeof(RectTransform))]
	public class CSectionButton : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField] private Color _selectedColor;
		[SerializeField] private Color _unselectedColor;
		
		[SerializeField, Self] private CUiComponentImage _image;
		[SerializeField, Self] private RectTransform _rectTransform;
		[SerializeField, Self] private CUiButton _button;
		[SerializeField] private CUiComponentImage _iconImage;
		
		protected CSectionContentBase SectionContentBase;
		private CSectionHandler _sectionHandler;
		
		private CSectionEntry _sectionEntry;

		public virtual void Initialize()
		{
			_button.AddClickListener(OnClick);
		}
		
		public void SetContent(CSectionContentBase sectionContentBase)
		{
			SectionContentBase = sectionContentBase;
		}

		public void SetTabHandler(CSectionHandler sectionHandler)
		{
			_sectionHandler = sectionHandler;
			_sectionEntry = new CSectionEntry(SectionContentBase, this);
			_sectionHandler.AddTab(_sectionEntry);
		}

		public void SetIcon(Sprite sprite)
		{
			if(sprite == null || _iconImage == null)
				return;
			
			_iconImage.SetSprite(sprite);
		}

		public virtual void Select()
		{
			SetSize(true);
			_image.SetColor(_selectedColor, false);
		}

		public virtual void Deselect()
		{
			SetSize(false);
			_image.SetColor(_unselectedColor, false);
		}

		private void SetSize(bool selected)
		{
			float size = selected ? 160f : 150f;
			_rectTransform.sizeDelta = new Vector2(size, _rectTransform.sizeDelta.y);
		}

		protected virtual void OnClick()
		{
			_sectionHandler.SwitchSection(_sectionEntry);
		}
	}
}