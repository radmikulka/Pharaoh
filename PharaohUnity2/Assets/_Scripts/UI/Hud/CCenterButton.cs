// =========================================
// AUTHOR: Juraj Joscak
// DATE:   20.08.2025
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using ServerData.Design;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CCenterButton : MonoBehaviour, IConstructable, IInitializable
	{
		private ICtsProvider _ctsProvider;
		private IGoToHandler _goToHandler;
		private CGoToAnalytics _goToAnalytics;

		private CUiButton _button;

		[Inject]
		private void Inject(
			ICtsProvider ctsProvider, 
			IGoToHandler goToHandler,
			CGoToAnalytics goToAnalytics
			)
		{
			_ctsProvider = ctsProvider;
			_goToHandler = goToHandler;
			_goToAnalytics = goToAnalytics;
		}
		
		public void Construct()
		{
			_button = GetComponent<CUiButton>();
		}
		
		public void Initialize()
		{
			_button.AddClickListener(OnButtonClick);
		}

		private void OnButtonClick()
		{
			_goToHandler.GoToRegionPoint(ERegionPoint.MainCity, ERegion.Region1, _ctsProvider.Token);
			_goToAnalytics.UXCityFindClick();
		}
	}
}