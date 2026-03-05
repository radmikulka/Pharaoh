// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.07.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;
using ServerData.Design;
using ServerData.Hits;
using UnityEngine;

namespace TycoonBuilder
{
	public class CContracts : CBaseUserComponent, IInitializable
	{
		private readonly Dictionary<EStaticContractId, CContract> _activeStoryContracts = new();
		private readonly HashSet<EStaticContractId> _completedStoryContracts = new();
		private readonly CDesignStoryContractConfigs _storyContractConfigs;
		private readonly CDynamicContracts _dynamicContracts = new();
		private readonly IRewardQueue _rewardQueue;
		private readonly IServerTime _serverTime;
		private readonly CHitBuilder _hitBuilder;
		private readonly CWorldMap _worldMap;
		private readonly IEventBus _eventBus;
		private readonly IMapper _mapper;

		public int CompletedContractsCount => _completedStoryContracts.Count;

		public CContracts(
			CDesignStoryContractConfigs storyContractConfigs,
			IRewardQueue rewardQueue,
			IServerTime serverTime,
			CHitBuilder hitBuilder,
			IEventBus eventBus,
			CWorldMap worldMap,
			IMapper mapper
			)
		{
			_storyContractConfigs = storyContractConfigs;
			_rewardQueue = rewardQueue;
			_hitBuilder = hitBuilder;
			_serverTime = serverTime;
			_worldMap = worldMap;
			_eventBus = eventBus;
			_mapper = mapper;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CYearSeenSignal>(OnYearSeen);
		}

		public void InitialSync(CContractsDto dto)
		{
			foreach (CContractDto contractDto in dto.StoryContracts)
			{
				CContract contract = _mapper.Map<CContractDto, CContract>(contractDto);
				_activeStoryContracts.Add(contract.StaticData.ContractId, contract);
			}

			SyncContracts(dto.PassengerContracts);

			_completedStoryContracts.UnionWith(dto.CompletedStoryContracts);

			TryGenerateNewStoryContracts(User.Progress.Region);
		}

		private void OnYearSeen(CYearSeenSignal signal)
		{
			TryGenerateNewStoryContracts(User.Progress.Region);
		}

		private void SyncContracts(CContractDto[] dto)
		{
			if(dto == null)
				return;

			foreach (CContractDto passengerContract in dto)
			{
				CContract contract = _mapper.Map<CContractDto, CContract>(passengerContract);
				_dynamicContracts.AddContract(contract);
			}
			_eventBus.Send(new CDynamicContractsSyncedSignal());
		}

		public bool IsContractCompleted(EStaticContractId id)
		{
			bool isCompleted = _completedStoryContracts.Contains(id);
			if (isCompleted)
				return true;
			isCompleted = User.LiveEvents.IsEventContractCompleted(id);
			return isCompleted;
		}

		public bool ContractStartedDelivery(EStaticContractId id)
		{
			bool isCompleted = IsContractCompleted(id);
			if (isCompleted)
				return false;

			CContract contract = GetStaticContract(id);
			if (contract.IsFleetTask)
				return true;
			bool contractStartedDelivery = contract.DeliveredAmount > 0;
			return contractStartedDelivery;
		}

		public bool ContractStartedDelivery(ECity id)
		{
			CContract contract = GetPassengerContractOrDefault(id);
			if (contract == null)
				return false;

			bool contractStartedDelivery = contract.DeliveredAmount > 0;
			return contractStartedDelivery;
		}

		public bool IsContractCompleted(SStaticContractPointer pointer)
		{
			bool isCompleted = IsContractCompleted(pointer.Id);
			if (isCompleted)
				return true;
			CContract contract = GetStaticContractOrDefault(pointer.Id);
			if (contract == null)
				return false;
			return contract.StaticData.Task > pointer.Task;
		}

		public bool IsContractFullyLoaded(EStaticContractId id)
		{
			bool isCompleted = IsContractCompleted(id);
			if (isCompleted)
				return true;
			CContract contract = GetStaticContractOrDefault(id);
			if (contract == null)
				return false;
			if (!contract.StaticData.IsLastTask)
				return false;
			if (contract.IsFleetTask)
				return User.Dispatches.ExistsForContract(id) && !User.Dispatches.AnyDispatchGoingToContract(id);
			return contract.IsCompleted;
		}

		public void TryActivateContract(EStaticContractId id)
		{
			CContract contract = GetStaticContract(id);
			if (contract.StaticData.IsActivated)
				return;
			contract.Activate();
			_eventBus.Send(new CContractActivatedSignal(contract));

			if (contract.Type == EContractType.Story)
			{
				_hitBuilder.GetBuilder(new CCreateStoryContractRequest(id, contract.Uid)).BuildAndSend();
			}

			_hitBuilder.GetBuilder(new CActivateContractRequest(contract.Uid)).BuildAndSend();
			_eventBus.Send(new CStoryContractStateChangedSignal(id));
		}

		public void CompleteContractTier(EStaticContractId id)
		{
			CContract contract = GetStaticContract(id);
			ValidateContractCompletion(id, contract);

			RemoveContract(contract);
			if (contract.StaticData.IsLastTask)
			{
				AddCompletedContract(contract);
				TryGenerateNewStoryContracts(User.Progress.Region);
				_eventBus.Send(new CStaticContractCompletedSignal(contract));
			}
			else
			{
				TryIncreaseTask(contract);
			}
			_eventBus.Send(new CStoryContractTierCompletedSignal(contract));
			_eventBus.Send(new CAfterStoryContractTierCompletedSignal(contract));
			_eventBus.Send(new CStoryContractStateChangedSignal(id));
		}

		private void AddCompletedContract(CContract contract)
		{
			switch (contract.Type)
			{
				case EContractType.Story:
					_completedStoryContracts.Add(contract.StaticData.ContractId);
					return;
				case EContractType.Event:
					User.LiveEvents.AddCompletedContract(contract);
					break;
			}
		}

		private void RemoveContract(IContract contract)
		{
			if (contract is CContract cContract && cContract.StaticData != null)
			{
				bool removed = _activeStoryContracts.Remove(cContract.StaticData.ContractId);
				if (removed)
				{
					return;
				}

				removed = User.LiveEvents.RemoveContract(cContract.StaticData.ContractId);
				if(removed)
					return;
			}

			_dynamicContracts.RemoveContract(contract.Uid);
		}

		private void TryGenerateNewStoryContracts(ERegion region)
		{
			// original logic moved from CStoryContractsSectionContent
			CStoryContractConfig[] configs = _storyContractConfigs.GetConfigs(region)
					.Where(cfg => !User.Contracts.IsContractCompleted(cfg.Id))
					.OrderBy(Order)
					.ToArray()
				;

			int unlockedItems = configs.Count(item => !IsLocked(item));
			configs = configs.Take(unlockedItems + 3).ToArray();

			foreach (CStoryContractConfig config in configs)
			{
				if(_completedStoryContracts.Contains(config.Id))
					continue;

				bool exists = _activeStoryContracts.ContainsKey(config.Id);
				if(exists)
					continue;

				CreateNewContract(config.Id, 1, null);
			}
			return;

			int Order(CStoryContractConfig cfg)
			{
				return IsLocked(cfg) ? 1 : 0;
			}

			bool IsLocked(CStoryContractConfig cfg)
			{
				return !User.IsUnlockRequirementMet(cfg.UnlockRequirements);
			}

		}

		public void CompletePassengerContractTier(ECity cityId)
		{
			CContract contract = _dynamicContracts.GetPassengerContractOrDefault(cityId);
			ValidatePassengerContractCompletion(cityId, contract);

			_eventBus.Send(new CPassengerContractCompletedSignal(contract));
			_eventBus.Send(new CPassengerContractStateChangedSignal(contract.PassengerData.CityId));
			_dynamicContracts.RemoveContract(contract.Uid);
		}

		private void ValidatePassengerContractCompletion(ECity cityId, CContract contract)
		{
			bool dispatchesExist = User.Dispatches.AnyDispatchGoingToCity(cityId);
			if (dispatchesExist)
			{
				throw new Exception($"Cannot complete passenger contract for city {cityId}. Not all dispatches are completed.");
			}

			if(contract.DeliveredAmount < contract.Requirement.Amount)
			{
				throw new Exception($"Cannot complete passenger contract for city {cityId}. Not enough resources delivered. " +
				                    $"Delivered: {contract.DeliveredAmount}, Required: {contract.Requirement.Amount}");
			}
		}

		private void TryIncreaseTask(CContract contract)
		{
			if(contract.Type != EContractType.Story)
				return;

			CreateNewContract(contract.StaticData.ContractId, contract.StaticData.Task + 1, contract.Uid);
		}

		public bool IsCurrentTaskCompleted(EStaticContractId id)
		{
			CContract contract = GetStaticContract(id);
			bool dispatchesExist = User.Dispatches.AnyDispatchGoingToContract(id);
			if (contract.IsFleetTask)
				return !dispatchesExist && User.Dispatches.ExistsForContract(id);
			bool allResourcesDelivered = contract.RemainingAmount <= 0;
			return !dispatchesExist && allResourcesDelivered;
		}

		public bool IsContractReadyToClaim(EStaticContractId id)
		{
			CContract contract = GetStaticContractOrDefault(id);
			if (contract.IsFleetTask)
				return User.Dispatches.ExistsForContract(id) && !User.Dispatches.AnyDispatchGoingToContract(id);
			return contract.RemainingAmount <= 0;
		}

		public IValuable[] GetCurrentTaskRewards(EStaticContractId id)
		{
			CContract contract = GetStaticContract(id);
			return contract.Rewards;
		}

		public IValuable[] GetCurrentTaskRewards(ECity cityId)
		{
			CContract contract = _dynamicContracts.GetPassengerContractOrDefault(cityId);
			if (contract == null)
			{
				throw new Exception($"No passenger contract found for city {cityId}");
			}
			return contract.Rewards.ToArray();
		}

		private void ValidateContractCompletion(EStaticContractId id, CContract contract)
		{
			bool isAlreadyCompleted = IsContractCompleted(id);
			if (isAlreadyCompleted)
			{
				throw new Exception($"Contract {id} is already completed.");
			}

			bool dispatchesExist = User.Dispatches.AnyDispatchGoingToContract(id);
			if (dispatchesExist)
			{
				throw new Exception($"Cannot complete contract tier {id}. Not all dispatches are completed.");
			}

			if(!contract.IsFleetTask && contract.DeliveredAmount < contract.Requirement.Amount)
			{
				throw new Exception($"Cannot complete contract tier {id}. Not enough resources delivered. " +
				                    $"Delivered: {contract.DeliveredAmount}, Required: {contract.Requirement.Amount}");
			}
		}

		public CContract GetPassengerContract(ECity cityId)
		{
			CContract contract = _dynamicContracts.GetPassengerContractOrDefault(cityId);
			if(contract == null)
				throw new Exception($"No passenger contract found for city {cityId}");
			return contract;
		}

		public CContract GetPassengerContractOrDefault(ECity cityId)
		{
			return _dynamicContracts.GetPassengerContractOrDefault(cityId);
		}

		public CContract[] GetAllPassengerContracts()
		{
			return _dynamicContracts.GetAllPassengerContracts();
		}

		public IEnumerable<CContract> AllStoryContracts()
		{
			foreach (CContract eventContract in User.LiveEvents.AllActiveContracts())
			{
				yield return eventContract;
			}

			foreach (var contract in _activeStoryContracts)
			{
				yield return contract.Value;
			}
		}

		public void SendVehicle(ECity cityId, EVehicle vehicleId, long timestampInMs)
		{
			CContract contract = GetPassengerContract(cityId);

			CObservableRecharger populationGenerator = User.City.GetPassengersGenerator(timestampInMs);
			int amountToSend = GetAmountToSendForContract(contract.RemainingAmount, vehicleId);
			int availableAmount = populationGenerator.CurrentAmount;
			int finalAmount = Math.Min(amountToSend, availableAmount);

			contract.AddDeliveredAmount(finalAmount);
			populationGenerator.Remove(finalAmount, timestampInMs);

			User.Dispatches.AddPassengerDispatch(cityId, vehicleId, finalAmount);
			_eventBus.Send(new CPassengerContractStateChangedSignal(cityId));
		}

		public void SendVehicle(EStaticContractId contractId, EVehicle vehicleId)
		{
			CContract contract = GetStaticContract(contractId);
			if (contract.IsFleetTask)
				throw new Exception($"SendVehicle called for fleet contract {contractId}. Use fleet dispatch instead.");

			int amountToSend = GetAmountToSendForContract(contract.RemainingAmount, vehicleId);
			int availableAmount = User.Warehouse.GetResourceAmount(contract.Requirement.Id);
			int finalAmount = Math.Min(amountToSend, availableAmount);

			contract.AddDeliveredAmount(finalAmount);

			CResourceValuable consumable = new (contract.Requirement.Id, finalAmount);
			_rewardQueue.ChargeValuable(EModificationSource.StoryContractDispatch, new IValuable[]{consumable});
			User.Dispatches.AddContractDispatch(contractId, vehicleId, finalAmount);

			if (contract.IsCompleted)
			{
				SStaticContractPointer pointer = new(contractId, contract.StaticData.Task);
				TryGenerateNewStoryContracts(User.Progress.Region);
				_eventBus.Send(new CStoryContractFullyLoadedSignal(pointer));
			}

			_eventBus.Send(new CStoryContractStateChangedSignal(contractId));
		}

		public int GetAmountToSendForContract(EStaticContractId contractId, EVehicle vehicleId)
		{
			CContract contract = GetStaticContract(contractId);
			int remainingAmount = contract.RemainingAmount;
			return GetAmountToSendForContract(remainingAmount, vehicleId);
		}

		public int GetAmountToSendForPassengerContract(ECity cityId, EVehicle vehicleId)
		{
			CContract contract = GetPassengerContract(cityId);
			int remainingAmount = contract.RemainingAmount;
			return GetAmountToSendForContract(remainingAmount, vehicleId);
		}

		private int GetAmountToSendForContract(int remainingAmount, EVehicle vehicleId)
		{
			int vehicleCapacity = User.Vehicles.GetCapacity(vehicleId);
			int amountToSend = Math.Min(remainingAmount, vehicleCapacity);
			return amountToSend;
		}

		public CContract GetStaticContract(EStaticContractId id)
		{
			CContract staticContract = GetStaticContractOrDefault(id);
			if(staticContract == null)
				throw new Exception($"No static contract found for city {id}");
			return staticContract;
		}

		public bool ContractExists(EStaticContractId id)
		{
			return GetStaticContractOrDefault(id) != null;
		}

		public CContract GetStaticContractOrDefault(EStaticContractId id)
		{
			CContract staticContract = _activeStoryContracts.GetValueOrDefault(id);
			if (staticContract == null)
			{
				staticContract = User.LiveEvents.GetEventContractOrDefault(id);
			}

			return staticContract;
		}

		public float GetDeliveredContractProgress(EStaticContractId id)
		{
			CContract contract = GetStaticContract(id);
			if (contract.IsFleetTask)
				return 1f;
			long currentTime = _serverTime.GetTimestampInMs();
			int deliveredAmount = contract.DeliveredAmount;
			foreach (CDispatch dispatch in User.Dispatches.GetDispatchesForContract(id))
			{
				float allowedTimeDiff = 2 * CTimeConst.Minute.InMilliseconds;
				bool arrivedToDestination = dispatch.TripCompletionTime - allowedTimeDiff <= currentTime;
				if (!arrivedToDestination)
				{
					deliveredAmount -= dispatch.ContractData.ResourceAmount;
				}
			}
			float progress = (float)deliveredAmount / contract.Requirement.Amount;
			return progress;
		}

		public int GetContractTaskCount(EStaticContractId id)
		{
			CContract contract = GetStaticContract(id);
			return contract.StaticData.TotalTasksCount;
		}

		private void CreateNewContract(EStaticContractId id, int task, string overrideUid)
		{
			CStoryContractConfig contractConfig = _storyContractConfigs.GetConfig(id);
			EMovementType movementType = contractConfig.OverrideMovementType != default ? contractConfig.OverrideMovementType : _worldMap.GetContractPoint(id).MovementType;
			CContractTask desiredTask = contractConfig.Tasks[task - 1];
			bool isFirstTask = task == 1;
			CStaticContractData metaData = new(id, !isFirstTask, task, contractConfig.TasksCount, desiredTask.AllowGoTo, contractConfig.StoryDialogue);
			string uid = overrideUid ?? Guid.NewGuid().ToString();
			SResource requirement = desiredTask.IsFleetTask
				? default
				: ((CStandardContractTaskComponent)desiredTask.Component).Resource;
			CFleetContractData fleetData = null;
			if (desiredTask.IsFleetTask)
			{
				CTransportFleetTaskConfig fleetTask = (CTransportFleetTaskConfig)desiredTask.Component;
				fleetData = new CFleetContractData(fleetTask.TotalPower, fleetTask.Slots);
			}
			CContract contract = new(
				EContractType.Story,
				requirement,
				0,
				contractConfig.TripPrice,
				desiredTask.Rewards.ToArray(),
				contractConfig.Customer,
				uid,
				!isFirstTask,
				new []{contractConfig.Region},
				movementType,
				staticData: metaData,
				fleetData: fleetData
				);

			_activeStoryContracts.Add(id, contract);
			_eventBus.Send(new CNewStoryContractCreatedSignal(id));
		}

		public void ClaimStoryContractRewards(CContract storyContract, IParticleSource[] particleSources)
		{
			EModificationSource source = EModificationSource.ClaimContract;
			_rewardQueue.AddRewardsWithSources(source, storyContract.Rewards, particleSources);

			_hitBuilder.GetBuilder(new CClaimContractRequest(storyContract.Uid))
				.SetExecuteImmediately()
				.SetOnSuccess<CClaimContractResponse>(OnStoryContractSuccess)
				.BuildAndSend();

			_eventBus.Send(new CStoryContractRewardsClaimedSignal(storyContract));
			return;

			void OnStoryContractSuccess(CClaimContractResponse response)
			{
				SyncContracts(response.GeneratedContracts);
				User.LiveEvents.SyncEvents(response.LiveEventsDto);
				User.LiveEvents.SyncContracts(response.NewEventContracts);
				_eventBus.Send(new CContractsSyncedSignal());
			}
		}

		public void ClaimPassengerContractRewards(CContract contract, IParticleSource[] particleSources)
		{
			EModificationSource source = EModificationSource.ClaimPassengerContract;
			_rewardQueue.AddRewardsWithSources(source, contract.Rewards, particleSources);

			_hitBuilder.GetBuilder(new CClaimContractRequest(contract.Uid))
				.SetExecuteImmediately()
				.SetOnSuccess<CClaimContractResponse>(OnPassengerContractSuccess)
				.BuildAndSend();

			_eventBus.Send(new CPassengerContractRewardsClaimedSignal(contract));
			return;

			void OnPassengerContractSuccess(CClaimContractResponse response)
			{
				User.Contracts.SyncContracts(response.GeneratedContracts);
				_eventBus.Send(new CPassengerContractStateChangedSignal(contract.PassengerData.CityId));
			}
		}

		public void MarkPassengerContractAsSeen(ECity city)
		{
			CContract contract = GetPassengerContract(city);
			contract.MarkAsSeen();
			_eventBus.Send(new CPassengerContractMarkedAsSeenSignal(city));

			_hitBuilder.GetBuilder(new CMarkContractAsSeenRequest(contract.Uid))
				.BuildAndSend();
		}

		public int UnseenStoryContractsCount()
		{
			IReadOnlyList<CStoryContractConfig> configs = _storyContractConfigs.GetConfigs(User.Progress.Region);
			int count = 0;
			for (int i = 0; i < configs.Count; i++)
			{
				if (IsContractUnseen(configs[i]))
				{
					count++;
				}
			}

			return count;

			bool IsContractUnseen(CStoryContractConfig cfg)
			{
				if (User.Progress.Region < cfg.Region)
					return false;

				if (User.Contracts.IsContractCompleted(cfg.Id))
					return false;

				if(!User.IsUnlockRequirementMet(cfg.UnlockRequirements))
					return false;

				if(_activeStoryContracts.TryGetValue(cfg.Id, out CContract contract))
				{
					return !contract.IsSeen;
				}

				return true;
			}
		}

		public int StoryContractsToClaimCount()
		{
			int result = 0;
			foreach (KeyValuePair<EStaticContractId, CContract> contract in _activeStoryContracts)
			{
				if(IsContractCompleted(contract.Key))
					continue;

				if(!IsContractReadyToClaim(contract.Key))
					continue;

				bool isRunning = User.Dispatches.AnyDispatchGoingToContract(contract.Key);
				if(isRunning)
					continue;

				result++;
			}

			return result;
		}

		public int EventContractsToClaimCount()
		{
			int result = 0;
			foreach (CContract contract in User.LiveEvents.AllActiveContracts())
			{
				if(User.LiveEvents.IsEventContractCompleted(contract.StaticData.ContractId))
					continue;

				if(contract.RemainingAmount > 0)
					continue;

				bool isRunning = User.Dispatches.AnyDispatchGoingToContract(contract.StaticData.ContractId);
				if(isRunning)
					continue;

				result++;
			}

			return result;
		}

		public int UnseenEventContractsCount()
		{
			IEnumerable<CContract> contracts = User.LiveEvents.AllActiveContracts();
			int count = 0;
			foreach (CContract contract in contracts)
			{
				if (IsContractUnseen(contract))
				{
					count++;
				}
			}

			return count;

			bool IsContractUnseen(CContract eventContract)
			{
				ILiveEvent eventData = User.LiveEvents.GetActiveEvent(eventContract.EventData.EventId);
				if(_serverTime.GetTimestampInMs() > eventData.EndTimeInMs)
					return false;

				if(_serverTime.GetTimestampInMs() < eventData.StartTimeInMs)
					return false;

				if (User.LiveEvents.IsEventContractCompleted(eventContract.StaticData.ContractId))
					return false;

				return !eventContract.IsSeen;
			}
		}

		public void MarkContractSeen(EStaticContractId id)
		{
			CContract contract = GetStaticContract(id);
			if(contract.IsSeen)
				return;

			contract.MarkAsSeen();
			_hitBuilder.GetBuilder(new CMarkContractAsSeenRequest(contract.Uid)).BuildAndSend();
			_eventBus.Send(new CStoryContractsMarkedAsSeenSignal(id));
		}
	}
}
