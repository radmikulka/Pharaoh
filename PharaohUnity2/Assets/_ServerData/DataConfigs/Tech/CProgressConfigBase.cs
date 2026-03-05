// =========================================
// AUTHOR: Radek Mikulka
// DATE:   15.07.2025
// =========================================

using System.Collections.Generic;

namespace ServerData
{
	public class CProgressConfigBase
	{
		private readonly Dictionary<EYearMilestone, int> _xpByYear = new();
		private readonly List<EYearMilestone> _orderedYears = new(30);

		public int GetXpInYear(EYearMilestone year)
		{
			return _xpByYear[year];
		}

		public EYearMilestone GetNextYear(EYearMilestone year)
		{
			for (int i = 0; i < _orderedYears.Count; i++)
			{
				EYearMilestone currentYear = _orderedYears[i];
				if (currentYear != year)
					continue;
				return _orderedYears[i + 1];
			}
			
			throw new KeyNotFoundException($"Year {year} not found in the ordered years list.");
		}

		protected void AddYear(EYearMilestone year, int xp)
		{
			_xpByYear.Add(year, xp);
			_orderedYears.Add(year);
		}
	}
}