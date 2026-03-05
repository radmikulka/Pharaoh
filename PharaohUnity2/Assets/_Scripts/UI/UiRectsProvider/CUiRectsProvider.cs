using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using TycoonBuilder.Ui;
using TycoonBuilder.Ui.CityMenu;
using TycoonBuilder.Ui.DepotMenu;
using TycoonBuilder.Ui.DispatchCenter;
using TycoonBuilder.Ui.DispatchMenu;
using TycoonBuilder.Ui.SpecialBuildings;
using UnityEngine;

namespace TycoonBuilder
{
    public class CUiRectsProvider : IInitializable
    {
        private readonly Dictionary<EUiRect, CUiRect> _rects = new();
        private readonly IScreenManager _screenManager;
        private readonly IEventBus _eventBus;

        public CUiRectsProvider(IScreenManager screenManager, IEventBus eventBus)
        {
            _screenManager = screenManager;
            _eventBus = eventBus;
        }

        public void Initialize()
        {
            _eventBus.AddTaskHandler<CGetUiRectRequest, CGetUiRectResponse>(HandleGetUiRectRequest);
        }

        private CGetUiRectResponse HandleGetUiRectRequest(CGetUiRectRequest task)
        {
            RectTransform rectTransform = GetRectTransform(task.RectId);
            return new CGetUiRectResponse(rectTransform);
        }

        private RectTransform GetRectTransform(EUiRect rectId)
        {
            switch (rectId)
            {
                case EUiRect.VehicleInDispatchMenu:
                {
                    CDispatchMenu dispatchMenu = _screenManager.GetMenu<CDispatchMenu>((int)EScreenId.Dispatch);
                    return dispatchMenu.GetFirstVehicleRect();
                }
                case EUiRect.BrokenVehicleRect:
                {
                    CDispatchMenu dispatchMenu = _screenManager.GetMenu<CDispatchMenu>((int)EScreenId.Dispatch);
                    return dispatchMenu.GetFirstBrokenVehicleRectOrDefault();
                }
                case EUiRect.DispatchInDispatchCentralMenu:
                {
                    CDispatchCenterMenu dispatchCenterMenu = _screenManager.GetMenu<CDispatchCenterMenu>((int)EScreenId.DispatchCenter);
                    return dispatchCenterMenu.GetFirstResourceDispatchRect();
                }
                case EUiRect.TutorialDepotVehicle:
                {
                    CDepotMenu depotMenu = _screenManager.GetMenu<CDepotMenu>((int)EScreenId.Depot);
                    return depotMenu.GetTutorialVehicleRect();
                }
                case EUiRect.FoundryFactoryButton:
                {
                    CFactoriesMenu factoriesMenu = _screenManager.GetMenu<CFactoriesMenu>((int)EScreenId.Factories);
                    return factoriesMenu.GetFoundryFactoryButtonRect();
                }
                case EUiRect.CityMenuContractsTabButton:
                {
                    CCityMenu cityMenu = _screenManager.GetMenu<CCityMenu>((int)EScreenId.City);
                    return cityMenu.GetContractsButtonRect();
                }
                case EUiRect.FirstPassengerContract:
                {
                    CCityMenu cityMenu = _screenManager.GetMenu<CCityMenu>((int)EScreenId.City);
                    return cityMenu.GetFirstPassengerContractRect();
                }
                case EUiRect.FirstEventContract:
                {
                    CContractsMenu contractsMenu = _screenManager.GetMenu<CContractsMenu>((int)EScreenId.Contracts);
                    return contractsMenu.GetFirstEventContractRect();
                }
                case EUiRect.FirstStoryContract:
                {
                    CContractsMenu contractsMenu = _screenManager.GetMenu<CContractsMenu>((int)EScreenId.Contracts);
                    return contractsMenu.GetFirstStoryContractRect();
                }
                case EUiRect.RoadSlopeContractButton:
                {
                    CContractsMenu contractsMenu = _screenManager.GetMenu<CContractsMenu>((int)EScreenId.Contracts);
                    return contractsMenu.GetContractButtonRect(EStaticContractId._1930_RoadSlope);
                }
                case EUiRect.FirstSpecialBuildingRect:
                {
                    CSpecialBuildingsMenu specialBuildingsMenu = _screenManager.GetMenu<CSpecialBuildingsMenu>((int)EScreenId.SpecialBuildings);
                    return specialBuildingsMenu.GetFirstSpecialBuildingRect();
                }
                case EUiRect.VehicleToUpgradeRect:
                {
                    CDepotMenu depotMenu = _screenManager.GetMenu<CDepotMenu>((int)EScreenId.Depot);
                    return depotMenu.GetUpgradeTutorialVehicleRect();
                }
                default: return GetRect(rectId).RectTransform;
            }
        }

        public void Register(CUiRect rect)
        {
            _rects.Add(rect.RectId, rect);
        }
        
        private CUiRect GetRect(EUiRect rectId)
        {
            return _rects[rectId];
        }
    }
}