// =========================================
// AUTHOR: Marek Karaba
// DATE:   06.01.2026
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Testing
{
	public class CMarekTestButton : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField, Self] private CUiButton _button;

		private IGoToHandler _goToHandler;
		
		[Inject]
		private void Inject(IGoToHandler goToHandler)
		{
			_goToHandler = goToHandler;
		}

		public void Initialize()
		{
			_button.AddClickListener(OnClick);
		}
		
		private void OnClick()
		{
			_goToHandler.GoToRegionPointInstant(ERegionPoint.RegionalOffice, ERegion.Region2);
		}
	}
}