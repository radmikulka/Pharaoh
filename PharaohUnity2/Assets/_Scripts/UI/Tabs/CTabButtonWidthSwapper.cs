// =========================================
// AUTHOR: Juraj Joscak
// DATE:   25.07.2025
// =========================================

using AldaEngine.AldaFramework;
using UnityEngine;

namespace TycoonBuilder.Ui
{
	public class CTabButtonWidthSwapper : MonoBehaviour, ITabButtonVisualSwapper, IConstructable
	{
		[SerializeField] private float _selectedWidthOffset;
		
		private RectTransform _rectTransform;
		private float _defaultWidth;
		
		public void Construct()
		{
			_rectTransform = (RectTransform)transform;
			_defaultWidth = _rectTransform.rect.width;
		}
		
		public void Select()
		{
			_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _defaultWidth+_selectedWidthOffset);
		}

		public void Deselect()
		{
			_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _defaultWidth);
		}
	}
}