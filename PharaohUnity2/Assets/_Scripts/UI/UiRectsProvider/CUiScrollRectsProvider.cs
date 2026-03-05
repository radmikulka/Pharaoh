// =========================================
// AUTHOR: Marek Karaba
// DATE:   14.11.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using TycoonBuilder.Ui;
using TycoonBuilder.Ui.DepotMenu;
using TycoonBuilder.Ui.DispatchCenter;
using TycoonBuilder.Ui.DispatchMenu;
using UnityEngine.UI;

namespace TycoonBuilder
{
	public class CUiScrollRectsProvider : IInitializable
	{
		private readonly Dictionary<EUiScrollRect, CUiScrollRect> _scrollRects = new();
		
		private readonly IScreenManager _screenManager;
		private readonly IEventBus _eventBus;

		public CUiScrollRectsProvider(IScreenManager screenManager, IEventBus eventBus)
		{
			_screenManager = screenManager;
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			_eventBus.AddTaskHandler<CGetUiScrollRectRequest, CGetUiScrollRectResponse>(HandleGetUiScrollRectRequest);
		}

		private CGetUiScrollRectResponse HandleGetUiScrollRectRequest(CGetUiScrollRectRequest request)
		{
			ScrollRect scrollRect = GetScrollRect(request.ScrollRectId);
			return new CGetUiScrollRectResponse(scrollRect);
		}

		public void Register(CUiScrollRect scrollRect)
		{
			_scrollRects.Add(scrollRect.ScrollRectId, scrollRect);
		}

		private ScrollRect GetScrollRect(EUiScrollRect scrollRectId)
		{
			switch (scrollRectId)
			{
				case EUiScrollRect.VehicleDepotRegion1:
				{
					CDepotMenu depotMenu = _screenManager.GetMenu<CDepotMenu>((int)EScreenId.Depot);
					return depotMenu.GetRegionScrollRect(ERegion.Region1);
				}
				case EUiScrollRect.DispatchMenuVehicleSection:
				{
					CDispatchMenu dispatchMenu = _screenManager.GetMenu<CDispatchMenu>((int)EScreenId.Dispatch);
					return dispatchMenu.GetVehicleSectionScrollRect();
				}
				case EUiScrollRect.DispatchCenterMenuSection:
				{
					CDispatchCenterMenu dispatchCenterMenu = _screenManager.GetMenu<CDispatchCenterMenu>((int)EScreenId.DispatchCenter);
					return dispatchCenterMenu.GetMenuSectionScrollRect();
				}
				case EUiScrollRect.ContractsMenuStoryContractsSection:
				{
					CContractsMenu contractsMenu = _screenManager.GetMenu<CContractsMenu>((int)EScreenId.Contracts);
					return contractsMenu.GetStoryContractsSectionScrollRect();
				}
				default: return GetRect(scrollRectId).ScrollRect;
			}
		}

		private CUiScrollRect GetRect(EUiScrollRect scrollRectId)
		{
			return _scrollRects[scrollRectId];
		}
	}
}