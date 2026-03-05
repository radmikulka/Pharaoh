// =========================================
// AUTHOR: Juraj Joscak
// DATE:   27.08.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using TycoonBuilder.Configs;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CUiRequirement : MonoBehaviour, IConstructable, IRequirementVisual
	{
		private CUiValuablePrice _price;
		private CYearVisualSetter _yearVisual;
		private EYearMilestone _year;
		private IEventBus _eventBus;
		private IYearProvider _yearProvider;

		private CUser _user;
		
		[Inject]
		private void Inject(CUser user, IEventBus eventBus, IYearProvider yearProvider)
		{
			_user = user;
			_eventBus = eventBus;
			_yearProvider = yearProvider;
		}
		
		public void Construct()
		{
			_price = GetComponentInChildren<CUiValuablePrice>();
			_yearVisual = GetComponentInChildren<CYearVisualSetter>(true);
		}
		
		public void SetValuable(IValuable valuable)
		{
			_price.gameObject.SetActive(true);
			_yearVisual.gameObject.SetActive(false);
			
			_price.SetValue(valuable);
			_year = EYearMilestone.None;
			_yearVisual.gameObject.SetActiveObject(false);
		}
		
		public void SetYear(EYearMilestone year, bool isUiRequirement)
		{
			_price.gameObject.SetActive(false);
			_yearVisual.gameObject.SetActive(true);
			
			_price.SetValue(null);
			_year = year;
			_yearVisual.gameObject.SetActiveObject(true);
			_yearVisual.SetRequirementYear(_year, isUiRequirement);
		}
		
		public bool TryShowUnaffordableTooltip()
		{
			if (_year == EYearMilestone.None)
				return _price.TryShowUnaffordableTooltip();
			
			if(_user.Progress.Year >= _year)
				return false;

			int desiredYearValue = _yearProvider.GetYear(_year);
			_eventBus.ProcessTask(new CShowTooltipTask("YearUnlock.ForPurchasingThisVehicleYouHaveToReachYear", true, desiredYearValue));
			return true;
		}

		public bool IsRequirementSatisfied()
		{
			if(_year != EYearMilestone.None)
				return _user.Progress.Year >= _year;
			
			if(_price.Price != null)
				return _price.CanAfford();

			return true;
		}
	}
}