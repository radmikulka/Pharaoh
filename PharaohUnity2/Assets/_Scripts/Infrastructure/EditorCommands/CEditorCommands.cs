// =========================================
// AUTHOR: Radek Mikulka
// DATE:   2023-09-07
// =========================================

using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using ServerData;
using ServerData.Design;
using ServerData.Dto;
using ServerData.Hits;
using TycoonBuilder;
using UnityEditor;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace TycoonBuilder
{
	public class CEditorCommands : CBaseEditorCommands, IInitializable
	{
		private CDesignResourceConfigs _resourceConfigs;
		private CDesignProgressConfig _progressConfig;
		private CDesignVehicleConfigs _vehicleConfigs;
		private ISceneManager _sceneManager;
		private IRewardQueue _rewardQueue;
		private IServerTime _serverTime;
		private CHitBuilder _hitBuilder;
		private IEventBus _eventBus;
		private IMapper _mapper;
		private CUser _user;
		
		private SStaticContractPointer _contractPointer;
		private UniTask _activeClaimContractTask;

		[Inject]
		private void Inject(
			CDesignResourceConfigs resourceConfigs, 
			CDesignProgressConfig progressConfig, 
			CDesignVehicleConfigs vehicleConfigs, 
			ISceneManager sceneManager, 
			IRewardQueue rewardQueue,
			IServerTime serverTime, 
			CHitBuilder hitBuilder, 
			IEventBus eventBus,
			IMapper mapper,
			CUser user
			)
		{
			_resourceConfigs = resourceConfigs;
			_progressConfig = progressConfig;
			_vehicleConfigs = vehicleConfigs;
			_sceneManager = sceneManager;
			_rewardQueue = rewardQueue;
			_serverTime = serverTime;
			_hitBuilder = hitBuilder;
			_eventBus = eventBus;
			_mapper = mapper;
			_user = user;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CDispatchForContractOpenedSignal>(OnDispatchForContractOpened);
		}

		private void Update()
		{
			ApplyCheats();
		}

		private void ApplyCheats()
		{
			if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				TryCompleteContract(_contractPointer);
			}
		}

		public void TryCompleteContract(SStaticContractPointer contractPointer)
		{
			if(_activeClaimContractTask.Status == UniTaskStatus.Pending)
				return;

			CIsScreenOpenedResponse response = _eventBus.ProcessTask<CIsScreenOpenedRequest, CIsScreenOpenedResponse>(
					new CIsScreenOpenedRequest(EScreenId.ContractRewards));
			if (response.IsActive)
			{
				_eventBus.ProcessTask(new CClaimContractsRewardsScreen());
				_eventBus.ProcessTask(new CCloseTopmostScreenTask());
				return;
			}
			
			CContract activeContract = _user.Contracts.GetStaticContractOrDefault(contractPointer.Id);
			if(activeContract == null || activeContract.StaticData.Task != contractPointer.Task)
				return;

			bool isCompleted = _user.Contracts.IsContractCompleted(contractPointer);
			if (isCompleted)
				return;
			
			bool isReadyToClaim = _user.Contracts.IsContractReadyToClaim(contractPointer.Id);
			if (isReadyToClaim)
			{
				_user.Contracts.CompleteContractTier(contractPointer.Id);
				return;
			}

			_activeClaimContractTask = CompleteContract(contractPointer.Id);
		}

		private void OnDispatchForContractOpened(CDispatchForContractOpenedSignal signal)
		{
			_contractPointer = ((CContract)signal.Contract).GetPointer();
		}

		public override void SavePreset()
		{
			#if UNITY_EDITOR
			string presetName = GetPresetName();
			if (EditorUtility.DisplayDialog("Presset", $"Určitě? Přepíše se preset {presetName}", "Ano", "Ne"))
			{
				CSavePresetRequest hit = new(presetName);
				CHitRecordBuilder builder = _hitBuilder.GetBuilder(hit)
					.SetSendAsSingleHit();
				_hitBuilder.BuildAndSend(builder);
			}
			#endif
		}

		public override void AddXp(float normalizedValue)
		{
			int totalXp = _progressConfig.GetXpInYear(_user.Progress.Year);
			int xpToAdd = (int)(normalizedValue * totalXp);
			
			_user.Progress.AddXp(xpToAdd);

			CHitRecordBuilder hitBuilder = _hitBuilder.GetBuilder(new CAddXpRequest(xpToAdd));
			hitBuilder.BuildAndSend();
		}

		public override void CheatVehicle(EVehicle vehicle)
		{
			CHitRecordBuilder hitBuilder = _hitBuilder.GetBuilder(new CCheatVehicleRequest(vehicle));
			hitBuilder.BuildAndSend();
			
			CValueModifyParams modifyParams= new CValueModifyParams().Add(new CVehicleChangeParam(EVehicleObtainSource.VehiclesMenu));
			_rewardQueue.AddRewards(EModificationSource.BuyVehicle, new IValuable[]
			{
				CValuableFactory.Vehicle(vehicle)
			}, modifyParams);

			_eventBus.Send(new CVehicleBoughtSignal(vehicle));
		}

		[Button]
		public override void RunMemoryDebug()
		{
			_eventBus.Send(new CGameResetSignal(null));
			_sceneManager.ResetGame();
			Resources.UnloadUnusedAssets();
			GC.Collect();
		}

		[Button]
		public override void ForceInternalHitError()
		{
			CInternalErrorRequest hit = new();
			CHitRecordBuilder builder = _hitBuilder.GetBuilder(hit)
				.SetSendAsSingleHit();
			_hitBuilder.BuildAndSend(builder);
		}

		private async UniTask CompleteContract(EStaticContractId contractId)
		{
			CancellationToken ct = CancellationToken.None;
			
			long time = _serverTime.GetTimestampInMs();
			CContract contract = _user.Contracts.GetStaticContract(contractId);
			EMovementType movementType = contract.MovementType;
			CResourceConfig resourceConfig = _resourceConfigs.GetResourceConfig(contract.Requirement.Id);

			while (contract.RemainingAmount > 0)
			{
				bool sent = await TrySendVehicle();
				if (sent) 
					continue;
				
				Debug.LogError($"Not enough available resources to complete contract {contractId}");
				break;
			}
			return;

			async UniTask<bool> TrySendVehicle()
			{
				IOrderedEnumerable<COwnedVehicle> filteredVehicles = _user.Vehicles.GetVehicles(movementType, resourceConfig.TransportType)
					.Where(vehicle => !_user.Dispatches.IsVehicleDispatched(vehicle.Id) && vehicle.IsOwned)
					.OrderBy(vehicle => vehicle.GetStatValue(EVehicleStat.Capacity));

				foreach (COwnedVehicle vehicle in filteredVehicles)
				{
					int capacity = vehicle.GetStatValue(EVehicleStat.Capacity);
					int resourceToTransport = CMath.Min(contract.RemainingAmount, capacity);
					SResource resource = new(contract.Requirement.Id, resourceToTransport);
					bool haveResource = _user.Warehouse.HaveResource(resource);
					if (!haveResource)
					{
						await BuyMaterial(resource, ct);
					}
					
					int fuelPrice = _vehicleConfigs.GetFuelEfficiency(
						contract.TripPrice.FuelPriceValue,
						vehicle.GetStatIndex(EVehicleStat.FuelEfficiency)
					);
					bool haveFuel = _user.FuelStation.HaveFuel(fuelPrice);
					if (!haveFuel)
					{
						await BuyFuel(ct);
					}

					bool haveDurability = vehicle.GetAndRefreshDurability(time) >= contract.TripPrice.DurabilityPrice;
					if (!haveDurability)
						continue;
					
					_user.Contracts.SendVehicle(contractId, vehicle.Id);

					CDispatch dispatch = _user.Dispatches.GetDispatchForVehicle(vehicle.Id);
					IValuable speedupPrice = _user.Dispatches.GetDispatchSpeedUpPrice(dispatch.Uid);
					if (!_user.OwnedValuables.HaveValuable(speedupPrice))
					{
						throw new Exception($"Not enough valuable {speedupPrice} to speedup dispatch {dispatch.Uid}");
					}
					_user.Dispatches.SpeedupDispatch(dispatch.Uid);
					return true;
				}

				return false;
			}
		}

		private async UniTask BuyMaterial(SResource resource, CancellationToken ct)
		{
			IValuable price = _resourceConfigs.GetGetMoreMaterialPrice(resource);
			bool haveMoney = _user.OwnedValuables.HaveValuable(price);
			if (!haveMoney)
				throw new Exception($"Not enough money {price} to buy more material {resource}");
			
			_user.Warehouse.BuyMoreMaterial(resource);
			await _rewardQueue.WaitWhileIsRunning(ct);
		}

		private async UniTask BuyFuel(CancellationToken ct)
		{
			CConsumableValuable fuelToBuy = CValuableFactory.Fuel(100);
			CValuableDto fuelDto = _mapper.ToJson<IValuable, CValuableDto>(fuelToBuy);
			_hitBuilder.GetBuilder(new CCheatValuableRequest(fuelDto)).BuildAndSend();
			_rewardQueue.AddRewards(EModificationSource.Cheat,new IValuable[]{fuelToBuy });
			await _rewardQueue.WaitWhileIsRunning(ct);
		}

		#if UNITY_EDITOR
		private string GetPresetName()
		{
			return !CServerConfig.Instance.ManualPresetId.IsNullOrEmpty() ? CServerConfig.Instance.ManualPresetId : CServerConfig.Instance.PresetId.ToString();
		}
		#endif

	}
}