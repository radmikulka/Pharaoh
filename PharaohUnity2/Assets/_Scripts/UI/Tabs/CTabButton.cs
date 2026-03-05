// =========================================
// AUTHOR: Juraj Joscak
// DATE:   07.07.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;

namespace TycoonBuilder.Ui
{
	public class CTabButton : MonoBehaviour, IConstructable, IInitializable
	{
		private ITabButtonVisualSwapper[] _visualSwappers;
		private CUiButton _button;
		private CTabs _tabsParent;
		private int _tabIndex;
		
		public void Construct()
		{
			_button = GetComponent<CUiButton>();
			_tabsParent = GetComponentInParent<CTabs>(true);
			_visualSwappers = GetComponentsInChildren<ITabButtonVisualSwapper>(true);
		}

		public void Initialize()
		{
			_button.AddClickListener(OnClick);
		}
		
		public void SetIndex(int index)
		{
			_tabIndex = index;
		}
		
		public void Select()
		{
			foreach (ITabButtonVisualSwapper swapper in _visualSwappers)
			{
				swapper.Select();
			}
		}
		
		public void Deselect()
		{
			foreach (ITabButtonVisualSwapper swapper in _visualSwappers)
			{
				swapper.Deselect();
			}
		}

		private void OnClick()
		{
			_tabsParent.SwitchTab(_tabIndex);
		}
	}

	public interface ITabButtonVisualSwapper
	{
		public void Select();
		public void Deselect();
	}
}