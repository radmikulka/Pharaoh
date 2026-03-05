// =========================================
// AUTHOR: Marek Karaba
// DATE:   22.07.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using TycoonBuilder;

namespace ServerData
{
	public class CBaseMenuTriggersConfig
	{
		private readonly Dictionary<IUnlockRequirement, List<EMenuTrigger>> _menuTriggers = new();
		private readonly List<CLoopingTrigger> _loopingTriggers = new();

		protected void AddTrigger(int year, EMenuTrigger trigger)
		{
			CYearUnlockRequirement requirement = IUnlockRequirement.Year((EYearMilestone)year);
			if (!_menuTriggers.TryGetValue(requirement, out List<EMenuTrigger> triggerList))
			{
				triggerList = new List<EMenuTrigger>();
				_menuTriggers[requirement] = triggerList;
			}

			triggerList.Add(trigger);
		}
		
		protected void AddTrigger(EStaticContractId contractId, EMenuTrigger trigger)
		{
			CContractUnlockRequirement requirement = IUnlockRequirement.Contract(contractId);
			if (!_menuTriggers.TryGetValue(requirement, out List<EMenuTrigger> triggerList))
			{
				triggerList = new List<EMenuTrigger>();
				_menuTriggers[requirement] = triggerList;
			}

			triggerList.Add(trigger);
		}
		
		protected void AddLoopTrigger(int startYear, int interval, EMenuTrigger trigger)
		{
			_loopingTriggers.Add(new CLoopingTrigger
			{
				StartYear = startYear,
				Interval = interval,
				Trigger = trigger
			});
		}
		
		public List<EMenuTrigger> GetActiveTriggers(IUnlockRequirement requirement)
		{
			if (CDebugConfig.Instance.ShouldSkip(EEditorSkips.Popups))
				return new List<EMenuTrigger>();
			
			List<EMenuTrigger> standardTriggers = AddStandardTriggers(requirement);
			List<EMenuTrigger> loopingTriggers = AddLoopingTriggers(requirement);
			
			List<EMenuTrigger> allTriggers = standardTriggers.Concat(loopingTriggers).ToList();
			return allTriggers;
		}

		private List<EMenuTrigger> AddStandardTriggers(IUnlockRequirement requirement)
		{
			switch (requirement)
			{
				case CYearUnlockRequirement yearRequirement:
					return GetYearStandardTriggers((int)yearRequirement.Year);
				case CContractUnlockRequirement contractRequirement:
					return GetContractStandardTriggers(contractRequirement.ContractId);
				default:
				throw new System.Exception($"Unsupported requirement type {requirement.GetType()}");
			}
		}

		private List<EMenuTrigger> GetContractStandardTriggers(EStaticContractId contractId)
		{
			List<EMenuTrigger> result = new();
			CContractUnlockRequirement[] contractUnlockRequirements = _menuTriggers.Where(kvp => kvp.Key is CContractUnlockRequirement)
				.Select(kvp => (CContractUnlockRequirement)kvp.Key)
				.Where(req => req.ContractId == contractId)
				.ToArray();
			
			if (contractUnlockRequirements.Length == 0)
				return result;
			
			result.AddRange(contractUnlockRequirements.SelectMany(req => _menuTriggers[req]));
			return result;
		}

		private List<EMenuTrigger> GetYearStandardTriggers(int year)
		{
			List<EMenuTrigger> result = new();
			CYearUnlockRequirement[] yearUnlockRequirements = _menuTriggers.Where(kvp => kvp.Key is CYearUnlockRequirement)
				.Select(kvp => (CYearUnlockRequirement)kvp.Key)
				.Where(req => (int)req.Year == year)
				.ToArray();
			
			if (yearUnlockRequirements.Length == 0)
				return result;
			
			result.AddRange(yearUnlockRequirements.SelectMany(req => _menuTriggers[req]));
			return result;
		}

		private List<EMenuTrigger> AddLoopingTriggers(IUnlockRequirement requirement)
		{
			if (requirement is not CYearUnlockRequirement)
				return new List<EMenuTrigger>();
			
			List<EMenuTrigger> loopingTriggers = new();
			int year = (int)((CYearUnlockRequirement)requirement).Year;
			foreach (CLoopingTrigger loopingTrigger in _loopingTriggers)
			{
				if (year == loopingTrigger.StartYear)
				{
					loopingTriggers.Add(loopingTrigger.Trigger);
				}
				
				if (year > loopingTrigger.StartYear && (year - loopingTrigger.StartYear) % loopingTrigger.Interval == 0)
				{
					loopingTriggers.Add(loopingTrigger.Trigger);
				}
			}
			return loopingTriggers;
		}

		private class CLoopingTrigger
		{
			public int StartYear { get; set; }
			public int Interval { get; set; }
			public EMenuTrigger Trigger { get; set; }
		}
	}
}