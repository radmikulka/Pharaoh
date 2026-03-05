// =========================================
// AUTHOR: Marek Karaba
// DATE:   07.11.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;

namespace TycoonBuilder.Ui
{
	public class CScreenHider : IInitializable
	{
		private readonly IScreenManager _screenManager;
		private readonly IEventBus _eventBus;

		public CScreenHider(IScreenManager screenManager, IEventBus eventBus
		)
		{
			_screenManager = screenManager;
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CScreenCloseStartSignal>(OnScreenCloseStart);
			_eventBus.Subscribe<CScreenOpenStartSignal>(OnScreenOpenStart);
		}
		
		private void OnScreenOpenStart(CScreenOpenStartSignal signal)
		{
			IReadOnlyList<IScreen> activeMenus = _screenManager.ActiveMenus;
			if (activeMenus.Count == 0)
				return;
			
			IReadOnlyList<IScreen> menus = _screenManager.ActiveMenus;
			foreach (IScreen iScreen in menus)
			{
				SetAlpha(iScreen, 0f);
			}
		}

		private void OnScreenCloseStart(CScreenCloseStartSignal signal)
		{
			IReadOnlyList<IScreen> activeMenus = _screenManager.ActiveMenus;
			if (activeMenus.Count <= 1)
				return;
			
			IScreen topmostMenu = activeMenus[^2];
			SetAlpha(topmostMenu, 1f);
		}
		
		private void SetAlpha(IScreen iScreen, float f)
		{
			if (iScreen is not CTycoonBuilderScreen tycoonBuilderScreen)
				return;
			
			tycoonBuilderScreen.SetAlpha(f);
		}
	}
}