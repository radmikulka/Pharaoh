// =========================================
// AUTHOR: Marek Karaba
// DATE:   22.07.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using UnityEngine;

namespace Pharaoh.Ui
{
	public class CScreenCloseButton : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField, Self] private CUiButton _button;
		[SerializeField, Parent] private CPharaohScreen _screen;
		
		public void Initialize()
		{
			_button.AddClickListener(CloseThisScreen);
		}

		private void CloseThisScreen()
		{
			_screen.TryCloseThisScreen();
		}
	}
}