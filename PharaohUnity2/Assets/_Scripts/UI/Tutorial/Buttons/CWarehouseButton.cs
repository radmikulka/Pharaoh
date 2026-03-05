// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.10.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using AldaEngine.AldaFramework;
using DG.Tweening;
using KBCore.Refs;
using ServerData;
using TycoonBuilder.Ui;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CWarehouseButton : CTutorialButton, IInitializable, IUpgradeMarkerValueSource
	{
		[SerializeField, Self] private CUiButton _button;
		
		private IEventBus _eventBus;
		private	CUser _user;
		private CWarehouseHandler _warehouseHandler;

		[Inject]
		private void Inject(IEventBus eventBus, CUser user, CWarehouseHandler warehouseHandler)
		{
			_eventBus = eventBus;
			_user = user;
			_warehouseHandler = warehouseHandler;
		}

		public void Initialize()
		{
			LoadInitialState();
			_eventBus.AddTaskHandler<CActivateWarehouseButtonTask>(ActivateWarehouseButton);
			_button.AddClickListener(OnClick);
		}

		private void ActivateWarehouseButton(CActivateWarehouseButtonTask task)
		{
			ActivateAnimated();
		}

		private void LoadInitialState()
		{
			SetActive(_user.Tutorials.IsTutorialCompleted(EDispatchCenterTutorialStep.Completed));
		}
		
		private void OnClick()
		{
			_eventBus.ProcessTask(new CShowScreenTask(EScreenId.Warehouse));
		}

		public bool ShowUpgradeMarker()
		{
			if (_warehouseHandler.IsFullyUpgraded())
				return false;
			
			IReadOnlyList<IUpgradeRequirement> requirements = _warehouseHandler.GetNextLevelUpgradeRequirements();
			return CLevelData.CanAffordRequirements(requirements, _user);
		}

		public bool UpgradeCompleted()
		{
			return _warehouseHandler.IsUpgradeFinished();
		}

		public bool UpgradeRunning()
		{
			return _warehouseHandler.IsUpgradeInProgress();
		}

		public EUpgradeMarkerState GetUpgradeMarkerState()
		{
			if (_warehouseHandler.IsUpgradeFinished())
				return EUpgradeMarkerState.Completed;
			
			if(_warehouseHandler.IsUpgradeInProgress())
				return EUpgradeMarkerState.Running;

			if (_warehouseHandler.IsFullyUpgraded())
				return EUpgradeMarkerState.Locked;
			
			IReadOnlyList<IUpgradeRequirement> requirements = _warehouseHandler.GetNextLevelUpgradeRequirements();
			if(CLevelData.CanAffordRequirements(requirements, _user))
				return EUpgradeMarkerState.Available;
			
			CYearMilestoneRequirement yearRequirement = requirements.FirstOrDefault(r => r is CYearMilestoneRequirement) as CYearMilestoneRequirement;
			if (yearRequirement == null || CLevelData.CanAffordRequirements(new[] { yearRequirement }, _user))
				return EUpgradeMarkerState.Unlocked;

			return EUpgradeMarkerState.Locked;
		}
	}
}