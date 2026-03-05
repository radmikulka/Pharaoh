// =========================================
// AUTHOR: Juraj Joscak
// DATE:   06.10.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using DG.Tweening;
using JetBrains.Annotations;
using KBCore.Refs;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public enum EPulsatingRequirementPlacement
	{
		None = 0,
		FactoryCrafting,
		FactoryBuySlot,
		FactoryRepair,
		ContractDispatch,
		BuyVehicle,
		UpgradeFuelStation,
		UpgradeDepot,
		UpgradeCity,
		UpgradeWarehouse,
		UpgradeFactory,
		RepairVehicle,
		UpgradeCapacity,
		UpgradeFuelEfficiency,
		UpgradeDurability,
		UpgradeFactorySpeedUp,
		UpgradeCitySpeedUp,
		UpgradeDepotSpeedUp,
		UpgradeFuelStationSpeedUp,
		UpgradeWarehouseSpeedUp,
		BuyBuildingPlot,
		BuyDispatcherOffer,
	}
	
	public class CPulsatingMissingRequirement : ValidatedMonoBehaviour, IConstructable, IInitializable
	{
		[SerializeField] private EPulsatingRequirementPlacement _placement;
		[SerializeField] private Transform[] _targetTransforms;

		private IEventBus _eventBus;

		private Sequence _punchTween;
		private IRequirementVisual _requirementVisual;
		
		[Inject]
		private void Inject(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}
		
		public void Construct()
		{
			_requirementVisual = GetComponent<IRequirementVisual>();
			if (_targetTransforms.IsEmpty())
			{
				_targetTransforms = new []{ transform };
			}
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CPulseMissingRequirementSignal>(OnPulseSignal);
		}
		
		public void TryPulse()
		{
			if(IsRequirementSatisfied())
				return;
			
			if(_punchTween != null)
				return;

			_punchTween = DOTween.Sequence();
			foreach (Transform target in _targetTransforms)
			{
				_punchTween.Join(target.DOPunchScale(Vector3.one * 0.3f, 0.3f, 0, 0));
			}
			_punchTween.OnComplete(() => _punchTween = null);
		}

		private void OnPulseSignal(CPulseMissingRequirementSignal signal)
		{
			if(signal.Placement != _placement)
				return;
			
			TryPulse();
		}

		private bool IsRequirementSatisfied()
		{
			if(_requirementVisual == null)
				return false;
			
			return _requirementVisual.IsRequirementSatisfied();
		}
	}
}