// =========================================
// AUTHOR: Marek Karaba
// DATE:   25.07.2025
// =========================================

using System;
using ServerData;

namespace TycoonBuilder.MenuTriggers
{
	public class CYearProvider : IYearProvider
	{
		private readonly CUser _user;
		public CYearProvider(CUser user)
		{
			_user = user;
		}

		public int GetCurrentYear()
		{
			EYearMilestone year = _user.Progress.Year;
			int yearValue = GetYear(year);
			return yearValue;
		}
		
		public int GetYear(EYearMilestone yearMilestone)
		{
			int year = ParseYear(yearMilestone);
			return year;
		}
		
		private int ParseYear(EYearMilestone yearMilestone)
		{
			string milestoneName = yearMilestone.ToString().TrimStart('_');
			int parseYear = int.Parse(milestoneName);
			return parseYear;
		}
		
		private EYearMilestone ParseYearToEnum(int year)
		{
			string yearString = year.ToString();
			if (Enum.TryParse<EYearMilestone>(yearString, out var yearMilestone))
			{
				return yearMilestone;
			}
			throw new ArgumentException($"Invalid year: {year}");
		}

		public EYearMilestone GetNextYear(EYearMilestone currentYear)
		{
			int year = ParseYear(currentYear);
			int nextYear = year + 1;
			
			EYearMilestone nextYearMilestone = ParseYearToEnum(nextYear);
			return nextYearMilestone;
		}
	}
}