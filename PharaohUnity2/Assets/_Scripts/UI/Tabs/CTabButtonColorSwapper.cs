// =========================================
// AUTHOR: Juraj Joscak
// DATE:   25.07.2025
// =========================================

using AldaEngine.AldaFramework;
using UnityEngine;

namespace TycoonBuilder.Ui
{
	public class CTabButtonColorSwapper : MonoBehaviour, ITabButtonVisualSwapper, IConstructable
	{
		[SerializeField] private Color _selectedColor;
		[SerializeField] private Color _deselectedColor;

		private IUiComponentGraphics _graphic;
		
		public void Construct()
		{
			_graphic = GetComponent<IUiComponentGraphics>();
		}
		
		public void Select()
		{
			_graphic.SetColor(_selectedColor, true);
		}

		public void Deselect()
		{
			_graphic.SetColor(_deselectedColor, true);
		}
	}
}