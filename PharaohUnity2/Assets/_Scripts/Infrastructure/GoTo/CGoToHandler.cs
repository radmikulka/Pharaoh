// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.09.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;
using ServerData.Design;
using TycoonBuilder;
using TycoonBuilder.GoToStates;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CGoToHandler : MonoBehaviour, IConstructable, IGoToHandler
	{
		private CDesignStoryContractConfigs _designContractConfigs;
		private CDesignIndustryConfigs _industryConfigs;
		private CDesignFactoryConfigs _factoryConfigs;
		private CDesignCityConfigs _cityConfigs;
		private CDesignMainCityConfigs _mainCityConfigs;
		private CDesignStoryContractConfigs _storyContractConfigs;
		private CDesignRegionConfigs _regionConfigs;
		private CDesignVehicleConfigs _vehicleConfigs;
		private CFsmStateFactory _fsmFactory;
		private ICtsProvider _ctsProvider;
		private CEventSystem _eventSystem;
		private IEventBus _eventBus;
		private CGoToFsm _activeFsm;
		private CUser _user;
		private IServerTime _serverTime;
		
		private static readonly CInputLock InputLock = new("CGoToHandler", EInputLockLayer.Default);
		
		private CGoToMainCityFsmFactory _goToMainCityFactory;
		private CGoToAdFsmFactory _goToAdFactory;

		[Inject]
		private void Inject(
			CDesignStoryContractConfigs designContractConfigs, 
			CDesignIndustryConfigs industryConfigs,
			CDesignFactoryConfigs factoryConfigs,
			CDesignCityConfigs cityConfigs,
			CDesignMainCityConfigs mainCityConfigs,
			CDesignStoryContractConfigs storyContractConfigs,
			CDesignRegionConfigs regionConfigs,
			CDesignVehicleConfigs vehicleConfigs,
			CEventSystem eventSystem, 
			ICtsProvider ctsProvider, 
			DiContainer container,
			IEventBus eventBus,
			CUser user,
			IServerTime serverTime
			)
		{
			_designContractConfigs = designContractConfigs;
			_fsmFactory = new CFsmStateFactory(container);
			_industryConfigs = industryConfigs;
			_factoryConfigs = factoryConfigs;
			_cityConfigs = cityConfigs;
			_mainCityConfigs = mainCityConfigs;
			_regionConfigs = regionConfigs;
			_vehicleConfigs = vehicleConfigs;
			_ctsProvider = ctsProvider;
			_eventSystem = eventSystem;
			_eventBus = eventBus;
			_user = user;
			_storyContractConfigs = storyContractConfigs;
			_serverTime = serverTime;
		}

		public void Construct()
		{
			_goToMainCityFactory = new CGoToMainCityFsmFactory(_fsmFactory, _ctsProvider, _user);
			_goToAdFactory = new CGoToAdFsmFactory(_fsmFactory, _user, _serverTime, _regionConfigs);
		}

		public void GoToSideCity(ECity city, ERegion region)
		{
			CGoToFsm fsm = GetNewFsm()
					.SetContextEntry(EGoToContextKey.SideCityId, city)
					.SetContextEntry(EGoToContextKey.Region, region)
					.AddState(_fsmFactory.CloseAllMenus)
					.SetBlockInput(true)
					;

			CCityConfig cityCfg = _cityConfigs.GetCityConfig(city);
			if(cityCfg.LiveEvent != ELiveEvent.None && region != ERegion.None)
			{
				fsm.SetContextEntry(EGoToContextKey.LiveEventId, cityCfg.LiveEvent);
				fsm.AddState(_fsmFactory.LoadLiveEvent);
			}
			else
			{
				fsm.AddState(_fsmFactory.LoadCoreGame);
			}

			if (region != ERegion.None && region >= _user.Progress.Region)
			{
				fsm.AddState(_fsmFactory.MoveToRegion)
					.AddState(_fsmFactory.MoveCameraToCity)
					.AddState(_fsmFactory.FocusOnCityDetail);
			}
			else
			{
				AddGoToRegionalOffice(fsm);	
			}
			
			fsm.AddState(_fsmFactory.OpenCityDispatchMenu);
			
			StartFsm(fsm);
		}

		public void GoToContract(EStaticContractId contractId)
		{
			CContract contract = _user.Contracts.GetStaticContract(contractId);
			ERegion regionPlacement = FindContractRegion(contract);

			CGoToFsm fsm = GetNewFsm()
				.SetContextEntry(EGoToContextKey.ContractId, contractId)
				.SetContextEntry(EGoToContextKey.Region, regionPlacement)
				.AddState(_fsmFactory.CloseAllMenus)
				.SetBlockInput(true)
				;

			if (contract.Type == EContractType.Event)
			{
				fsm.SetContextEntry(EGoToContextKey.LiveEventId, contract.EventData.EventId);

				if (regionPlacement != ERegion.None)
				{
					fsm.AddState(_fsmFactory.LoadLiveEvent);
				}
			}
			else
			{
				fsm.AddState(_fsmFactory.LoadCoreGame);
			}

			if (regionPlacement != ERegion.None)
			{
				fsm.AddState(_fsmFactory.MoveToRegion)
					.AddState(_fsmFactory.MoveCameraToContract)
					.AddState(_fsmFactory.FocusOnContractDetail);
			}
			else
			{
				AddGoToRegionalOffice(fsm);
			}
			
			fsm.AddState(_fsmFactory.OpenContractDispatchMenu);
			
			StartFsm(fsm);
		}

		private ERegion FindContractRegion(CContract contract)
		{
			switch (contract.Type)
			{
				case EContractType.Event:
				{
					IEventWithContracts eventData = _user.LiveEvents.GetEventContent<IEventWithContracts>(contract.EventData.EventId);
					return eventData.Region;
				}
				case EContractType.Story:
					return contract.Regions.First();
				default: throw new Exception($"Unknown contract type: {contract}");
			}
		}

		public void GoToContractAndClaim(EStaticContractId contractId)
		{
			CContract contract = _user.Contracts.GetStaticContract(contractId);
			ERegion regionPlacement = FindContractRegion(contract);

			CGoToFsm fsm = GetNewFsm()
					.SetContextEntry(EGoToContextKey.ContractId, contractId)
					.SetContextEntry(EGoToContextKey.Region, regionPlacement)
					.AddState(_fsmFactory.CloseAllMenus)
					.SetBlockInput(true)
					;

			if (contract.Type == EContractType.Event && regionPlacement != ERegion.None)
			{
				fsm.SetContextEntry(EGoToContextKey.LiveEventId, contract.EventData.EventId);
				fsm.AddState(_fsmFactory.LoadLiveEvent);
			}
			else
			{
				fsm.AddState(_fsmFactory.LoadCoreGame);
			}

			if (regionPlacement != ERegion.None)
			{
				fsm.AddState(_fsmFactory.MoveToRegion)
					.AddState(_fsmFactory.MoveCameraToContract);
			}
			else
			{
				AddGoToRegionalOffice(fsm);
			}
			
			fsm.AddState(_fsmFactory.ClaimContractState);
			
			StartFsm(fsm);
		}

		private void AddGoToRegionalOffice(CGoToFsm fsm)
		{
			fsm.SetContextEntry(EGoToContextKey.Region, _user.Progress.Region)
				.SetContextEntry(EGoToContextKey.RegionPoint, ERegionPoint.RegionalOffice);
			fsm.AddState(_fsmFactory.MoveCameraToRegionPoint);
			fsm.AddState(_fsmFactory.FocusOnRegionalOffice);
		}

		public bool GoToResourceMine(EIndustry industry)
		{
			CResourceIndustryConfig industryConfig = _industryConfigs.GetConfig(industry);
			bool isOpened = _user.IsUnlockRequirementMet(industryConfig.UnlockRequirement);
			if (isOpened)
			{
				CGoToFsm fsm = GetNewFsm()
					.SetContextEntry(EGoToContextKey.IndustryId, industry)
					.SetContextEntry(EGoToContextKey.Region, industryConfig.Region)
					.AddState(_fsmFactory.CloseAllMenus)
					.SetBlockInput(true);

				if (industryConfig.LiveEvent != ELiveEvent.None && industryConfig.Region != ERegion.None)
				{
					fsm.SetContextEntry(EGoToContextKey.LiveEventId, industryConfig.LiveEvent);
					fsm.AddState(_fsmFactory.LoadLiveEvent);
				}
				else
				{
					fsm.AddState(_fsmFactory.LoadCoreGame);
				}
				
				if (industryConfig.Region != ERegion.None && industryConfig.Region >= _user.Progress.Region)
				{
					fsm.AddState(_fsmFactory.MoveToRegion)
						.AddState(_fsmFactory.MoveCameraToResourceMine)
						.AddState(_fsmFactory.FocusOnIndustryDetail);
				}
				else
				{
					AddGoToRegionalOffice(fsm);
				}
				
				fsm.AddState(_fsmFactory.OpenResourceDispatchMenu);
			
				StartFsm(fsm);
				return true;
			}

			if (industryConfig.UnlockRequirement is CContractUnlockRequirement contractUnlock)
			{
				CStoryContractConfig contractCfg = _designContractConfigs.GetConfig(contractUnlock.ContractId);
				if (_user.IsUnlockRequirementMet(contractCfg.UnlockRequirements))
				{
					GoToContract(contractUnlock.ContractId);
					return true;
				}
			}
			
			_eventBus.ProcessTask(new CShowTooltipTask("Tooltip.MineNotUnlocked", true));
			return false;
		}

		public void GoToRegionalOfficeDispatch(EIndustry industry)
		{
			CGoToFsm fsm = GetNewFsm()
				.SetContextEntry(EGoToContextKey.IndustryId, industry)
				.AddState(_fsmFactory.CloseAllMenus);
			
			AddGoToRegionalOffice(fsm);
			
			fsm.AddState(_fsmFactory.OpenResourceDispatchMenu);
			
			StartFsm(fsm);
		}

		public void GoToDailyTask(ETaskId taskId)
		{
			switch (taskId)
			{
				case ETaskId.BuiltCityProperty:
					GoToBuildCityProperty();
					break;
				case ETaskId.VehicleFleet:
					GoToVehiclePurchase();
					break;
				case ETaskId.UpgradeCity:
					GoToCityUpgradeMenu();
					break;
				case ETaskId.UpgradeWarehouse:
					GoToUpgradeWarehouse();
					break;
				case ETaskId.FactoryLevel:
					GoToFactories();
					break;
				case ETaskId.DecadePassFreeRewards:
					GoToDecadePassFreeReward();
					break;
				case ETaskId.WatchAd:
					GoToAnyAd();
					break;
				case ETaskId.FullyUpgradedVehicles:
				case ETaskId.UpgradeVehicle:
					GoToVehicleUpgrade();
					break;
				case ETaskId.DispatchCargoVehicle:
					if(GoToStoryContractMenu())
						return;
					if(GoToEventContractsMenu())
						return;
					GoToResourceMine(EIndustry.CoalMine);
					break;
				case ETaskId.CompleteAnyContract:
					if(GoToStoryContractMenu())
						return;
					if(GoToEventContractsMenu())
						return;
					GoToPassengerContractsMenu();
					break;
				case ETaskId.CompleteEventContract:
					GoToEventContractsMenu();
					break;
				case ETaskId.DispatchPassengerVehicle:
					GoToPassengerContractsMenu();
					break;
				case ETaskId.CreateProducts:
					GoToFactories();
					break;
				case ETaskId.SpendGold:
					_eventBus.ProcessTask(new CShowTooltipTask($"CompanyGrowth.DailyTask.SpendGoldGoToTooltip", true));
					break;
				case ETaskId.SpendTycoonCash:
					_eventBus.ProcessTask(new CShowTooltipTask($"CompanyGrowth.DailyTask.SpendCashGoToTooltip", true));
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(taskId), taskId, null);
			}
		}
		
		private void GoToBuildCityProperty()
		{
			if (!_user.IsUnlockRequirementMet(CDesignMainCityConfigs.UnlockRequirement))
			{
				_eventBus.ProcessTask(new CShowTooltipTask("CityMenu.NotYetUnlockedTooltip", true));
				return;
			}
			
			int parcelsWithBuildings = _user.City.GetPlotsWithBuildingCount();
			int currentLevel = _user.City.GetLevel();
			int currentLevelParcels = _mainCityConfigs.GetStatValueAtLevel(ECityStat.BuildingPlots, currentLevel);
			if (parcelsWithBuildings == currentLevelParcels)
			{
				GoToCityUpgradeMenu();
				return;
			}

			CGoToFsm fsm = GetNewFsm();
			int currentUnlockedParcels = _user.City.GetUnlockedBuildingPlotsCount();
			if (parcelsWithBuildings == currentUnlockedParcels)
			{
				fsm.AddState(_fsmFactory.CloseAllMenus)
					.AddState(_fsmFactory.OpenBuildingParcelMenu)
					;
			}
			else
			{
				fsm.AddState(_fsmFactory.CloseAllMenus)
					.AddState(_fsmFactory.OpenSpecialBuildingMenu)
					;
			}
			
			StartFsm(fsm);
		}
		
		private void GoToCityUpgradeMenu()
		{
			if (!_user.IsUnlockRequirementMet(CDesignMainCityConfigs.UnlockRequirement))
			{
				_eventBus.ProcessTask(new CShowTooltipTask("CityMenu.NotYetUnlockedTooltip", true));
				return;
			}
			
			CGoToFsm fsm = GetNewFsm()
					.AddState(_fsmFactory.CloseAllMenus)
					.AddState(_fsmFactory.OpenCityUpgradeMenuState)
				;
			
			StartFsm(fsm);
		}
		
		private void GoToUpgradeWarehouse()
		{
			COpenMenuState openWarehouse = _fsmFactory.OpenMenuState;
			openWarehouse.SetData(EScreenId.Warehouse);
			COpenMenuState openWarehouseUpgrade = _fsmFactory.OpenMenuState;
			openWarehouseUpgrade.SetData(EScreenId.WarehouseUpgrade);
			
			CGoToFsm fsm = GetNewFsm()
					.AddState(_fsmFactory.CloseAllMenus)
					.AddState(openWarehouse)
					.AddState(openWarehouseUpgrade)
				;
			
			StartFsm(fsm);
		}
		
		private void GoToDecadePassFreeReward()
		{
			EYearMilestone currentYear = _user.Progress.Year;
			int ownedXp = _user.Progress.XpInCurrentYear;
			
			CRegionConfig config = _regionConfigs.GetRegionConfig(_user.Progress.Region);
			CDecadePassConfigData decadePassConfig = config.GetDecadePass();

			int numberOfItems = decadePassConfig.FreeRewards.Count;
			int validRewardIndex = -1;

			for (int i = 0; i < numberOfItems; i++)
			{
				if (!IsRewardClaimable(i))
					continue;
				
				validRewardIndex = i;
				break;
			}

			if (validRewardIndex == -1)
			{
				_eventBus.ProcessTask(new CShowTooltipTask("DecadePass.NoRewardToClaim", true));
				return;
			}

			COpenMenuState openMenuState = _fsmFactory.OpenMenuState;
			openMenuState.SetData(EScreenId.DecadePass);
			
			CGoToFsm fsm = GetNewFsm()
					.SetContextEntry(EGoToContextKey.PassRewardIndex, validRewardIndex)
					.AddState(_fsmFactory.CloseAllMenus)
					.AddState(openMenuState)
					.AddState(_fsmFactory.ScrollToPassReward)
				;
			
			StartFsm(fsm);

			bool IsRewardClaimable(int index)
			{
				return _user.DecadePass.CanClaimIndex(index, false, currentYear, ownedXp);
			}
		}
		
		private void GoToAnyAd()
		{
			EAdPlacement[] validPlacements = 
			{
				EAdPlacement.FuelSmallOffer,
				EAdPlacement.SoftCurrencySmallOffer,
				EAdPlacement.PassengerSmallOffer,
				EAdPlacement.DailyDealsRandomOffer,
				EAdPlacement.DecadePassFree,
				EAdPlacement.DispatcherAgnes,
				EAdPlacement.EventPassFree,
				EAdPlacement.WrenchSmallOffer,
				EAdPlacement.EventCoinsSmallOffer,
				EAdPlacement.MachineOilSmallOffer
			};

			for(int i = 0; i < validPlacements.Length; i++)
			{
				CGoToFsm fsm = _goToAdFactory.TryCreateGoToPlacement(validPlacements[i], _ctsProvider.Token);
				if (fsm == null)
					continue;
				
				StartFsm(fsm);
				return;
			}
		}

		private void GoToVehicleUpgrade()
		{
			COpenMenuState openMenuState = _fsmFactory.OpenMenuState;
			openMenuState.SetData(EScreenId.Depot);
			
			CGoToFsm fsm = GetNewFsm()
					.AddState(_fsmFactory.CloseAllMenus)
					.AddState(openMenuState)
				;
			
			EVehicle vehicle = GetVehicleToUpgrade();
			if (vehicle != EVehicle.None)
			{
				fsm.SetContextEntry(EGoToContextKey.Vehicle, vehicle);
				fsm.AddState(_fsmFactory.OpenVehicleUpgrade);
			}
			
			StartFsm(fsm);
			return;

			EVehicle GetVehicleToUpgrade()
			{
				COwnedVehicle result = _user.Vehicles
					.GetVehicles(EMovementType.All, ETransportType.All)
					.OrderBy(v => _user.Vehicles.MissingValuablesForUpgrade(v.Id, _user.Progress.Region))
					.ThenBy(v => v.GetMaxLevel() - v.GetCurrentLevel())
					.FirstOrDefault()
					;
				
				return result?.Id ?? EVehicle.None;
			}
		}

		private void GoToVehiclePurchase()
		{
			COpenMenuState openMenuState = _fsmFactory.OpenMenuState;
			openMenuState.SetData(EScreenId.Depot);
			
			CGoToFsm fsm = GetNewFsm()
					.AddState(_fsmFactory.CloseAllMenus)
					.AddState(openMenuState)
				;
			
			EVehicle vehicle = GetVehicleToPurchase();
			if (vehicle != EVehicle.None)
			{
				fsm.SetContextEntry(EGoToContextKey.Vehicle, vehicle);
				fsm.AddState(_fsmFactory.OpenVehicleUpgrade);
			}
			
			StartFsm(fsm);
			return;

			EVehicle GetVehicleToPurchase()
			{
				Dictionary<EVehicle, CVehicleConfig> configs = _vehicleConfigs.GetConfigs();
				EVehicle cheapest = EVehicle.None;
				CConsumableValuable cheapestPrice = new(EValuable.SoftCurrency, int.MaxValue);
				
				foreach (KeyValuePair<EVehicle, CVehicleConfig> keyValuePair in configs)
				{
					CVehicleConfig vehicleConfig = keyValuePair.Value;
					if (!_user.IsUnlockRequirementMet(vehicleConfig.UnlockRequirement) || _user.Vehicles.IsVehicleOwned(vehicleConfig.Id))
						continue;

					CConsumableValuable consumablePrice = vehicleConfig.Price as CConsumableValuable;
					if(consumablePrice == null || consumablePrice.Value >= cheapestPrice.Value)
						continue;
						
					cheapestPrice = consumablePrice;
					cheapest = vehicleConfig.Id;
				}

				return cheapest;
			}
		}

		private void GoToFactories()
		{
			CGoToFsm fsm = GetNewFsm()
					.AddState(_fsmFactory.CloseAllMenus)
					.AddState(_fsmFactory.OpenFactoriesMenu)
					;
			
			StartFsm(fsm);
		}
		
		private void GoToPassengerContractsMenu()
		{
			CGoToFsm fsm = GetNewFsm()
					.SetContextEntry(EGoToContextKey.CityMenuTab, ECityMenuTab.TransportContracts)
					.AddState(_fsmFactory.CloseAllMenus)
					.AddState(_fsmFactory.OpenCityMenu)
				;
			
			StartFsm(fsm);
		}
		
		private bool GoToEventContractsMenu()
		{
			if(!EventContractAvailable())
				return false;
			
			CGoToFsm fsm = GetNewFsm()
					.SetContextEntry(EGoToContextKey.UiRect, EUiRect.FirstEventContract)
					.AddState(_fsmFactory.CloseAllMenus)
					.AddState(_fsmFactory.OpenContractsMenu)
					.AddState(_fsmFactory.PulseRect)
				;
			
			StartFsm(fsm);
			return true;
			
			bool EventContractAvailable()
			{
				return _user.Contracts.AllStoryContracts()
						.Where(c =>
						{
							if (c.Type != EContractType.Event)
								return false;

							return LiveEventActive(c.EventData.EventId);
						})
						.Any()
					;
			}
			
			bool LiveEventActive(ELiveEvent liveEvent)
			{
				ILiveEvent activeEvent = _user.LiveEvents.GetActiveEventOrDefault(liveEvent);
				if (activeEvent == null)
					return false;
				
				return !activeEvent.IsFinished(_serverTime.GetTimestampInMs());
			}
		}

		private bool GoToStoryContractMenu()
		{
			if(!StoryContractAvailable())
				return false;
			
			CGoToFsm fsm = GetNewFsm()
					.SetContextEntry(EGoToContextKey.UiRect, EUiRect.FirstStoryContract)
					.AddState(_fsmFactory.CloseAllMenus)
					.AddState(_fsmFactory.OpenContractsMenu)
					.AddState(_fsmFactory.PulseRect)
					;
			
			StartFsm(fsm);
			return true;

			bool StoryContractAvailable()
			{
				return _user.Contracts.AllStoryContracts()
						.Where(c =>
						{
							if (c.Type == EContractType.Event)
								return false;

							CStoryContractConfig cfg = _storyContractConfigs.GetConfig(c.StaticData.ContractId);
							return _user.IsUnlockRequirementMet(cfg.UnlockRequirements);
						})
						.Any()
					;
			}
		}
		
		public bool GoToFactory(EFactory factory, EResource resource)
		{
			CFactoryConfig factoryConfig = _factoryConfigs.GetFactoryConfig(factory);
			bool isOpened = _user.IsUnlockRequirementMet(factoryConfig.UnlockRequirement);
			if (!isOpened)
			{
				_eventBus.ProcessTask(new CShowTooltipTask("Tooltip.FactoryNotUnlocked", true));
				return false;
			}
			
			CGoToFsm fsm = GetNewFsm()
					.SetContextEntry(EGoToContextKey.Resource, resource)
					.SetContextEntry(EGoToContextKey.FactoryId, factory)
					.AddState(_fsmFactory.CloseAllMenus)
					.AddState(_fsmFactory.OpenFactoryMenu)
				;
			
			StartFsm(fsm);
			return true;
		}

		public async UniTask GoToRegionPoint(ERegionPoint regionPoint, ERegion region, CancellationToken ct)
		{
			CGoToFsm fsm = GetNewFsm()
					.SetContextEntry(EGoToContextKey.RegionPoint, regionPoint)
					.SetContextEntry(EGoToContextKey.Region, region)
					.AddState(_fsmFactory.MoveToRegion)
					.AddState(_fsmFactory.MoveCameraToRegionPoint)
				;
			
			StartFsm(fsm);

			await UniTask.WaitUntil(() => fsm.IsCompleted, cancellationToken: ct);
		}

		public void GoToRegionPointInstant(ERegionPoint regionPoint, ERegion region)
		{
			CGoToFsm fsm = GetNewFsm()
					.SetContextEntry(EGoToContextKey.RegionPoint, regionPoint)
					.SetContextEntry(EGoToContextKey.Region, region)
					.AddState(_fsmFactory.MoveToRegion)
					.AddState(_fsmFactory.MoveCameraToRegionPointInstant)
				;
			
			StartFsm(fsm);
		}

		public void GetMoreMaterial(SResource resource, string source)
		{
			CGoToFsm fsm = GetNewFsm()
					.SetContextEntry(EGoToContextKey.Resource, resource)
					.SetContextEntry(EGoToContextKey.EventSource, source)
					.AddState(_fsmFactory.OpenGetMoreResourcesMenu)
				;
			
			StartFsm(fsm);
		}

		public void GoToShop(EShopTab tab)
		{
			CGoToFsm fsm = GetNewFsm()
					.SetContextEntry(EGoToContextKey.ShopTab, tab)
					.AddState(_fsmFactory.CloseAllMenus)
					.AddState(_fsmFactory.OpenShopMenu)
				;
			
			StartFsm(fsm);
		}

		public void GoToCityUpgradeItAndGoBackToMenu()
		{
			CGoToFsm fsm = _goToMainCityFactory.CreateGoToCityUpgradeItAndGoBackToMenu();
			StartFsm(fsm);
		}

		public async UniTask GoToCityAndUpgradeIt(CancellationToken ct)
		{
			CGoToFsm fsm = _goToMainCityFactory.CreateGoToCityAndUpgradeIt();
			StartFsm(fsm);
			
			await UniTask.WaitUntil(() => fsm.IsCompleted, cancellationToken: ct);
		}

		public void GoToMainCity(ERegionPoint regionPoint, ERegion region)
		{
			CGoToFsm fsm = _goToMainCityFactory.CreateGoToMainCity(regionPoint, region);
			StartFsm(fsm);
		}

		private CGoToFsm GetNewFsm()
		{
			return new CGoToFsm(_ctsProvider.Token);
		}

		private void Update()
		{
			UpdateActiveFsm();
		}

		private void UpdateActiveFsm()
		{
			if(_activeFsm == null)
				return;
			
			_activeFsm.Tick();
			if (_activeFsm.IsCompleted)
			{
				StopFsm();
			}
		}

		public void TryKillActiveGoTo()
		{
			if(_activeFsm == null)
				return;
			StopFsm();
		}

		private void StartFsm(CGoToFsm fsm)
		{
			_activeFsm?.Cancel();
			
			fsm.Start();
			_activeFsm = fsm;

			if (fsm.BlockInput)
			{
				_eventSystem.AddInputLocker(InputLock);
			}
		}

		private void StopFsm()
		{
			if (_activeFsm.BlockInput)
			{
				_eventSystem.RemoveInputLocker(InputLock);
			}
			
			_activeFsm = null;
		}
	}
}

