// =========================================
// AUTHOR: Marek Karaba
// DATE:   06.11.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using ServerData.Design;
using UnityEngine;

namespace TycoonBuilder
{
	public class CContractsAnalytics : IInitializable
	{
		private readonly CDesignResourceConfigs _designResourceConfigs;
		private readonly CDesignCityConfigs _designCityConfigs;
		private readonly CResourceConfigs _resourceConfigs;
		private readonly IAnalytics _analytics;
		private readonly IEventBus _eventBus;
		private readonly CWorldMap _worldMap;
		private readonly CUser _user;
		
		private readonly Dictionary<string, object> _cachedParams = new();
		private CContract _currentlyAnimatedContract;
		private long _animationStartTime;
		private string _openingSource;
		private int _clipsDuration;

		public CContractsAnalytics(
			CDesignResourceConfigs designResourceConfigs,
			CDesignCityConfigs designCityConfigs,
			CResourceConfigs resourceConfigs,
			IAnalytics analytics,
			IEventBus eventBus,
			CWorldMap worldMap,
			CUser user)
		{
			_designResourceConfigs = designResourceConfigs;
			_designCityConfigs = designCityConfigs;
			_resourceConfigs = resourceConfigs;
			_analytics = analytics;
			_eventBus = eventBus;
			_worldMap = worldMap;
			_user = user;
		}
		
		public void Initialize()
		{
			_eventBus.Subscribe<CDispatchForContractOpenedSignal>(OnStoryContractShow);
			_eventBus.Subscribe<CFirstVehicleSentForStoryContractSignal>(OnFirstVehicleSentForStoryContract);
			_eventBus.Subscribe<CStoryContractRewardsClaimedSignal>(OnStoryContractRewardsClaimed);
			_eventBus.Subscribe<CDispatchForPassengersOpenedSignal>(OnPassengersContractShow);
			_eventBus.Subscribe<CFirstVehicleSentForPassengerContractSignal>(OnFirstVehicleSentForPassengerContract);
			_eventBus.Subscribe<CPassengerContractRewardsClaimedSignal>(OnPassengerContractRewardsClaimed);
			
			_eventBus.Subscribe<CContractAnimationStartedSignal>(OnAnimationStarted);
			_eventBus.Subscribe<CContractAnimationCompletedSignal>(OnAnimationCompleted);
			_eventBus.Subscribe<CAnimationSkipRequestedSignal>(OnSkipRequested);
			
			_eventBus.AddTaskHandler<CSetDispatchMenuAnalyticsSourceTask>(OnSetDispatchMenuAnalyticsSourceTask);
		}

		private void OnSetDispatchMenuAnalyticsSourceTask(CSetDispatchMenuAnalyticsSourceTask signal)
		{
			_openingSource = signal.OpeningSource;
		}

		private void OnStoryContractShow(CDispatchForContractOpenedSignal signal)
		{
			CContract contract = (CContract)signal.Contract;
			bool isContractStarted = _user.Contracts.ContractStartedDelivery(contract.StaticData.ContractId);
			if (isContractStarted)
				return;

			_cachedParams.Clear();

			_cachedParams.Add("ContractSource", _openingSource);
			AddCommonParamsForStoryContract(contract, false);
			
			_analytics.SendData("ContractShow", _cachedParams);
		}

		private void OnFirstVehicleSentForStoryContract(CFirstVehicleSentForStoryContractSignal signal)
		{
			CContract contract = (CContract)signal.Contract;
			_cachedParams.Clear();
			 AddCommonParamsForStoryContract(contract, false);
			
			_analytics.SendData("ContractStart", _cachedParams);
		}

		private void OnStoryContractRewardsClaimed(CStoryContractRewardsClaimedSignal signal)
		{
			CContract contract = (CContract)signal.Contract;
			_cachedParams.Clear();
			AddCommonParamsForStoryContract(contract, true);
			
			_analytics.SendData("ContractFinish", _cachedParams);
		}

		private void AddCommonParamsForStoryContract(CContract contract, bool isFinished)
		{
			EMovementType movementType = contract.MovementType;

			IEnumerable<string> selectedFlags = Enum.GetValues(typeof(EMovementType))
				.Cast<EMovementType>()
				.Where(flag => flag != EMovementType.None && flag != EMovementType.All && movementType.HasFlag(flag))
				.Select(flag => flag.ToString());

			string movementTypesString = string.Join(",", selectedFlags);

			CResourceResourceConfig resourceResourceConfig = _resourceConfigs.Resources.GetConfig(contract.Requirement.Id);
			CResourceConfig resourceConfig = _designResourceConfigs.GetResourceConfig(resourceResourceConfig.Id);
			ETransportType transportType = resourceConfig.TransportType;

			int availableVehiclesCount = _user.Vehicles.GetAvailableVehiclesCount(movementType, transportType);
			int contractTaskIndex = contract.RemainingAmount == 0 ? contract.StaticData.Task : contract.StaticData.Task - 1;
			if (isFinished)
			{
				contractTaskIndex--;
			}

			SResource requirement = contract.Requirement;
			bool isEventContract = contract.Type == EContractType.Event;
			string contractType = isEventContract ? "EventContract" : "StoryContract";
			string contractName = GetContractName(contract);

			_cachedParams.Add("ContractType", contractType);
			_cachedParams.Add("ContractTaskIndex", contractTaskIndex);
			_cachedParams.Add("ContractName", contractName);
			_cachedParams.Add("TransportType", transportType.ToString());
			_cachedParams.Add("MovementType", movementTypesString);
			_cachedParams.Add("RequiredResource", requirement.Id);
			_cachedParams.Add("RequiredResourceAmount", requirement.Amount);
			_cachedParams.Add("AvailableVehicles", availableVehiclesCount.ToString());
		}

		private string GetContractName(CContract contract)
		{
			string fullContractName = contract.StaticData.ContractId.ToString();
			string contractName = fullContractName.Contains('_') 
				? fullContractName[(fullContractName.LastIndexOf('_') + 1)..] 
				: fullContractName;
			return contractName;
		}

		private void OnPassengersContractShow(CDispatchForPassengersOpenedSignal signal)
		{
			bool isContractStarted = _user.Contracts.ContractStartedDelivery(signal.CityId);
			if (isContractStarted)
				return;
			
			_cachedParams.Clear();
			_cachedParams.Add("ContractSource", _openingSource);
			CContract contract = _user.Contracts.GetPassengerContract(signal.CityId);
			AddCommonParamsForPassengerContract(contract);

			_analytics.SendData("ContractShow", _cachedParams);
		}

		private void OnFirstVehicleSentForPassengerContract(CFirstVehicleSentForPassengerContractSignal signal)
		{
			_cachedParams.Clear();
			CContract contract = _user.Contracts.GetPassengerContract(signal.CityId);
			AddCommonParamsForPassengerContract(contract);
			
			_analytics.SendData("ContractStart", _cachedParams);
		}

		private void OnPassengerContractRewardsClaimed(CPassengerContractRewardsClaimedSignal signal)
		{
			_cachedParams.Clear();
			AddCommonParamsForPassengerContract(signal.Contract);
			
			_analytics.SendData("ContractFinish", _cachedParams);
		}

		private void AddCommonParamsForPassengerContract(CContract contract)
		{
			ECity cityId = contract.PassengerData.CityId;
			
			IEnumerable<string> selectedFlags = Enum.GetValues(typeof(EMovementType))
				.Cast<EMovementType>()
				.Where(flag => flag != EMovementType.None && flag != EMovementType.All && contract.MovementType.HasFlag(flag))
				.Select(flag => flag.ToString());

			string movementTypesString = string.Join(",", selectedFlags);
			
			CResourceResourceConfig resourceResourceConfig = _resourceConfigs.Resources.GetConfig(contract.Requirement.Id);
			CResourceConfig resourceConfig = _designResourceConfigs.GetResourceConfig(resourceResourceConfig.Id);
			ETransportType transportType = resourceConfig.TransportType;
			
			int availableVehiclesCount = _user.Vehicles.GetAvailableVehiclesCount(contract.MovementType, transportType);
			
			_cachedParams.Add("ContractType", "PassengerContract");
			_cachedParams.Add("ContractName", cityId.ToString());
			_cachedParams.Add("TransportType", transportType.ToString());
			_cachedParams.Add("MovementType", movementTypesString);
			_cachedParams.Add("RequiredResource", contract.Requirement.Id);
			_cachedParams.Add("RequiredResourceAmount", contract.Requirement.Amount);
			_cachedParams.Add("AvailableVehicles", availableVehiclesCount.ToString());
		}
		
		private void OnAnimationStarted(CContractAnimationStartedSignal signal)
		{
			_animationStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			_currentlyAnimatedContract = (CContract)signal.Contract;
			_clipsDuration = signal.Anim.GetClipsLengthInMs();
			
			_cachedParams.Clear();
			AddAnimationParams(_currentlyAnimatedContract);
			_cachedParams.Add("CanBeSkipped", signal.CanShowSkipButton);
			
			_analytics.SendData("ContractAnimationStart", _cachedParams);
		}

		private void OnAnimationCompleted(CContractAnimationCompletedSignal signal)
		{
			_currentlyAnimatedContract = null;
			_clipsDuration = 0;
		}

		private void OnSkipRequested(CAnimationSkipRequestedSignal signal)
		{
			if (_currentlyAnimatedContract == null)
				return;
			
			_cachedParams.Clear();
			AddAnimationParams(_currentlyAnimatedContract);
			
			long timeElapsed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _animationStartTime;
			_cachedParams.Add("SkipPointInMS", timeElapsed);
			
			_analytics.SendData("ContractAnimationSkip", _cachedParams);
		}
		
		private void AddAnimationParams(CContract contract)
		{
			int contractTaskIndex = contract.RemainingAmount == 0 ? contract.StaticData.Task : contract.StaticData.Task - 1;
			contractTaskIndex--;
			
			string contractName = GetContractName(contract);
			_cachedParams.Add("ContractType", "StoryContract");
			_cachedParams.Add("ContractTaskIndex", contractTaskIndex);
			_cachedParams.Add("ContractName", contractName);
			_cachedParams.Add("TotalLengthInMS", _clipsDuration);
		}
	}
}