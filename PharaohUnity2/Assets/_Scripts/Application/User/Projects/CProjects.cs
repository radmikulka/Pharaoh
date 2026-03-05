// =========================================
// AUTHOR: Radek Mikulka
// DATE:   24.02.2026
// =========================================

using System;
using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using ServerData.Hits;
using ServiceEngine.Ads;

namespace TycoonBuilder
{
	public class CProjects : CBaseUserComponent, IInitializable
	{
		private readonly IMapper _mapper;
		private readonly IEventBus _eventBus;
		private readonly CHitBuilder _hitBuilder;

		private CActiveProject[] _activeProjects = Array.Empty<CActiveProject>();

		public CProjects(IMapper mapper, IEventBus eventBus, CHitBuilder hitBuilder)
		{
			_mapper = mapper;
			_eventBus = eventBus;
			_hitBuilder = hitBuilder;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CFactoryProductionStartedSignal>(OnFactoryProductionStarted);
			_eventBus.Subscribe<CStaticContractCompletedSignal>(OnStoryContractCompleted);
			_eventBus.Subscribe<COwnedValuableChangedSignal>(OnOwnedValuableChanged);
			_eventBus.Subscribe<CVehicleDispatchedSignal>(OnVehicleDispatched);
			_eventBus.Subscribe<CAdSucceededSignal>(OnAdSucceeded);
		}

		public void InitialSync(CProjectsDto dto)
		{
			if (dto == null) return;
			_activeProjects = _mapper.Map<CActiveProjectDto, CActiveProject>(dto.Projects);
		}

		public void Sync(CProjectsDto dto) => InitialSync(dto);

		public IReadOnlyList<CActiveProject> GetActiveProjects() => _activeProjects;

		public void TryIncreaseProjectProgress(ETaskRequirement requirementId, int amount = 1)
		{
			foreach (CActiveProject project in _activeProjects)
			foreach (CActiveTask task in project.ActiveTasks)
			foreach (CCountableTaskRequirement req in task.Requirements)
			{
				if (req.Id == requirementId)
				{
					req.IncreaseCount(amount);
				}
			}
		}

		public void ClaimProject(EProjectId projectId)
		{
			_hitBuilder.GetBuilder(new CClaimProjectRequest(projectId))
				.BuildAndSend();
		}

		private void OnFactoryProductionStarted(CFactoryProductionStartedSignal signal)
		{
			TryIncreaseProjectProgress(ETaskRequirement.CreateProducts);
		}

		private void OnStoryContractCompleted(CStaticContractCompletedSignal signal)
		{
			if (signal.Contract.Type == EContractType.Event)
			{
				TryIncreaseProjectProgress(ETaskRequirement.CompleteEventContract);
			}
			TryIncreaseProjectProgress(ETaskRequirement.CompleteAnyContract);
		}

		private void OnOwnedValuableChanged(COwnedValuableChangedSignal signal)
		{
			if (signal.Valuable is not CConsumableValuable consumable)
				return;

			if (consumable.Value >= 0)
				return;

			switch (consumable.Id)
			{
				case EValuable.SoftCurrency:
					TryIncreaseProjectProgress(ETaskRequirement.SpendTycoonCash, -consumable.Value);
					break;
				case EValuable.HardCurrency:
					TryIncreaseProjectProgress(ETaskRequirement.SpendGold, -consumable.Value);
					break;
			}
		}

		private void OnVehicleDispatched(CVehicleDispatchedSignal signal)
		{
			if (signal.Dispatch.Type == EDispatchType.Passenger)
			{
				TryIncreaseProjectProgress(ETaskRequirement.DispatchPassengerVehicle);
				return;
			}
			TryIncreaseProjectProgress(ETaskRequirement.DispatchCargoVehicle);
		}

		private void OnAdSucceeded(CAdSucceededSignal signal)
		{
			TryIncreaseProjectProgress(ETaskRequirement.WatchAd);
		}
	}
}
