// =========================================
// AUTHOR: Radek Mikulka
// DATE:   30.01.2026
// =========================================

using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using ServerData.Dto;
using ServerData.Hits;
using ServiceEngine.Ads;
using UnityEngine;

namespace TycoonBuilder
{
	public class CTasks : CBaseUserComponent, IInitializable
	{
		private readonly CHitBuilder _hitBuilder;
		private readonly IEventBus _eventBus;
		private readonly IMapper _mapper;
		private readonly CCompanyGrowthAnalytics _analytics;
		private readonly IServerTime _serverTime;
		
		private CActiveTask[] _activeTasks;
		private CWeeklyTask[] _weeklyTasks;
		public bool IsFinalDailyRewardClaimed { get; private set; }
		public long WeekRefreshTime { get; private set; }
		public int ClaimedDailyTasksThisWeek { get; private set; }
		private IValuable[] _dailyCompletionRewards;
		public IReadOnlyList<IValuable> DailyCompletionRewards => _dailyCompletionRewards;
		public EStaticContractId UnlockContract { get; private set; }

		public CTasks(IMapper mapper, IEventBus eventBus, CHitBuilder hitBuilder, CCompanyGrowthAnalytics analytics, IServerTime serverTime)
		{
			_eventBus = eventBus;
			_mapper = mapper;
			_hitBuilder = hitBuilder;
			_analytics = analytics;
			_serverTime = serverTime;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CFactoryProductionStartedSignal>(OnFactoryProductionStarted);
			_eventBus.Subscribe<CStaticContractCompletedSignal>(OnStoryContractCompleted);
			_eventBus.Subscribe<COwnedValuableChangedSignal>(OwnedValuableChanged);
			_eventBus.Subscribe<CVehicleDispatchedSignal>(OnVehicleDispatched);
			_eventBus.Subscribe<CAdSucceededSignal>(OnAdSucceeded);
		}

		public void InitialSync(CTasksDto dto)
		{
			_activeTasks = _mapper.Map<CActiveTaskDto, CActiveTask>(dto.DailyTasks);
			_weeklyTasks = _mapper.Map<CWeeklyTaskDto, CWeeklyTask>(dto.WeeklyTasks);
			IsFinalDailyRewardClaimed = dto.IsFinalDailyRewardClaimed;
			WeekRefreshTime = dto.WeekRefreshTime;
			ClaimedDailyTasksThisWeek = dto.ClaimedDailyTasksThisWeek;
			_dailyCompletionRewards = _mapper.FromJson<IValuable>(dto.DailyCompletionRewards);
			UnlockContract = dto.UnlockContract;
			
			_analytics.CompanyDailyTasksGenerate(_activeTasks.Select(t => t.TaskId).ToArray(), _serverTime.GetDayRefreshTimeInMs());
			_analytics.CompanyWeeklyTaskGenerate(_weeklyTasks.Select(w => w.RequiredPoints).ToArray(), WeekRefreshTime);
		}
		
		public CActiveTask[] GetActiveDailyTasks()
		{
			return _activeTasks;
		}
		
		public CWeeklyTask[] GetWeeklyTasks()
		{
			return _weeklyTasks;
		}

		private void OnFactoryProductionStarted(CFactoryProductionStartedSignal signal)
		{
			TryIncreaseProgress(ETaskRequirement.CreateProducts);
		}

		private void OwnedValuableChanged(COwnedValuableChangedSignal signal)
		{
			if(signal.Valuable is not CConsumableValuable consumable)
				return;
			
			if(consumable.Value >= 0)
				return;
			
			switch (consumable.Id)
			{
				case EValuable.SoftCurrency:
					User.Tasks.TryIncreaseProgress(ETaskRequirement.SpendTycoonCash, -consumable.Value);
					break;
				case EValuable.HardCurrency:
					User.Tasks.TryIncreaseProgress(ETaskRequirement.SpendGold, -consumable.Value);
					break;
			}
		}

		private void OnVehicleDispatched(CVehicleDispatchedSignal signal)
		{
			if (signal.Dispatch.Type == EDispatchType.Passenger)
			{
				TryIncreaseProgress(ETaskRequirement.DispatchPassengerVehicle);
				return;
			}
			TryIncreaseProgress(ETaskRequirement.DispatchCargoVehicle);
		}

		private void OnStoryContractCompleted(CStaticContractCompletedSignal signal)
		{
			CContract contract = (CContract)signal.Contract;
			if(contract.StaticData.ContractId == UnlockContract)
				return;

			if (contract.Type == EContractType.Event)
			{
				TryIncreaseProgress(ETaskRequirement.CompleteEventContract);
			}
			TryIncreaseProgress(ETaskRequirement.CompleteAnyContract);
		}

		public void TryIncreaseProgress(ETaskRequirement requirementId, int amount = 1)
		{
			if(!User.IsUnlockRequirementMet(IUnlockRequirement.Contract(UnlockContract)))
				return;
			
			bool changed = false;
			foreach (CActiveTask task in GetAllActiveTasks())
			{
				bool justFulfilledRequirement = false;
				foreach (CCountableTaskRequirement requirement in task.Requirements)
				{
					if (requirement.Id == requirementId)
					{
						requirement.IncreaseCount(amount);
						changed = true;
						if (requirement.TargetCount == requirement.CurrentCount)
						{
							justFulfilledRequirement = true;
						}
					}
				}

				if (justFulfilledRequirement && task.Requirements.All(r => r.CurrentCount >= r.TargetCount))
				{
					_analytics.CompanyDailyTaskAvailable(task.TaskId);
				}
			}
			
			if (changed)
			{
				_eventBus.Send(new CDailyTasksChangedSignal());
			}
		}

		private void OnAdSucceeded(CAdSucceededSignal signal)
		{
			TryIncreaseProgress(ETaskRequirement.WatchAd);
		}

		private IEnumerable<CActiveTask> GetAllActiveTasks()
		{
			foreach (CActiveTask dailyTask in _activeTasks)
			{
				yield return dailyTask;
			}
		}

		public void MarkFinalDailyRewardAsClaimed()
		{
			IsFinalDailyRewardClaimed = true;
			_eventBus.Send(new CDailyTasksChangedSignal());
		}

		public void ClaimDailyTask(string uid)
		{
			_hitBuilder.GetBuilder(new CClaimDailyTaskRequest(uid))
				.BuildAndSend();

			ClaimedDailyTasksThisWeek++;
			CActiveTask data = _activeTasks.First(t => t.Uid == uid);
			data.MarkAsClaimed();
			_analytics.CompanyDailyTaskClaim(data.TaskId);

			CWeeklyTask justFulfilledWeekly = _weeklyTasks.FirstOrDefault(t => t.RequiredPoints == ClaimedDailyTasksThisWeek);
			if (justFulfilledWeekly != null)
			{
				_analytics.CompanyWeeklyTaskAvailable(justFulfilledWeekly.RequiredPoints);
			}
			
			_eventBus.Send(new CDailyTasksChangedSignal());
		}

		public void ClaimWeeklyChallengeReward(string uid)
		{
			CWeeklyTask data = _weeklyTasks.First(t => t.Uid == uid);
			
			data.MarkAsClaimed();
			_hitBuilder.GetBuilder(new CClaimWeeklyTaskRequest(data.Uid))
				.BuildAndSend();
			
			_analytics.CompanyWeeklyTaskClaim(data.RequiredPoints);
			_eventBus.Send(new CDailyTasksChangedSignal());
		}

		public int DailyTasksToClaim()
		{
			if(!IsFinalDailyRewardClaimed && _activeTasks.All(t => t.IsClaimed))
				return 1;
			
			return _activeTasks.Count(t => !t.IsClaimed && t.Requirements.All(r => r.CurrentCount >= r.TargetCount));
		}

		public int WeeklyChallengeRewardsToClaim()
		{
			return _weeklyTasks.Count(t => !t.IsClaimed && t.RequiredPoints <= ClaimedDailyTasksThisWeek);
		}
	}
}