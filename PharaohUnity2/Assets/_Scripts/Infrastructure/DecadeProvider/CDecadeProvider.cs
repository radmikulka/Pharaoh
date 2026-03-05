// =========================================
// AUTHOR: Marek Karaba
// DATE:   25.07.2025
// =========================================

using System;
using ServerData;

namespace TycoonBuilder
{
	public class CDecadeProvider : IDecadeProvider
	{
		private readonly IYearProvider _yearProvider;

		public CDecadeProvider(IYearProvider yearProvider)
		{
			_yearProvider = yearProvider;
		}

		public EDecadeMilestone GetDecade(EYearMilestone yearMilestone)
		{
			int year = _yearProvider.GetYear(yearMilestone);
			switch (year)
			{
				case < 1940:
					return EDecadeMilestone._1930s;
				case < 1950:
					return EDecadeMilestone._1940s;
				case < 1960:
					return EDecadeMilestone._1950s;
				case < 1970:
					return EDecadeMilestone._1960s;
				case < 1980:
					return EDecadeMilestone._1970s;
				case < 1990:
					return EDecadeMilestone._1980s;
				case < 2000:
					return EDecadeMilestone._1990s;
				case < 2010:
					return EDecadeMilestone._2000s;
				case < 2020:
					return EDecadeMilestone._2010s;
				case < 2030:
					return EDecadeMilestone._2020s;
			}
			
			throw new ArgumentOutOfRangeException(nameof(yearMilestone), $"Year milestone {yearMilestone} is not defined.");
		}
		
		public EYearMilestone GetCurrentDecadeYearMilestone(ERegion regionId)
		{
			switch (regionId)
			{
				case ERegion.Region1:
					return EYearMilestone._1930;
				case ERegion.Region2:
					return EYearMilestone._1940;
				case ERegion.Region3:
					return EYearMilestone._1950;
				case ERegion.Region4:
					return EYearMilestone._1960;
				case ERegion.Region5:
					return EYearMilestone._1970;
				case ERegion.Region6:
					return EYearMilestone._1980;
				case ERegion.Region7:
					return EYearMilestone._1990;
				case ERegion.Region8:
					return EYearMilestone._2000;
				case ERegion.Region9:
					return EYearMilestone._2010;
			}
			
			throw new ArgumentOutOfRangeException(nameof(regionId), $"Next decade year {regionId} is not defined.");
		}

		public EYearMilestone GetNextDecadeYearMilestone(ERegion regionId)
		{
			switch (regionId)
			{
				case ERegion.Region1:
					return EYearMilestone._1940;
				case ERegion.Region2:
					return EYearMilestone._1950;
				case ERegion.Region3:
					return EYearMilestone._1960;
				case ERegion.Region4:
					return EYearMilestone._1970;
				case ERegion.Region5:
					return EYearMilestone._1980;
				case ERegion.Region6:
					return EYearMilestone._1990;
				case ERegion.Region7:
					return EYearMilestone._2000;
				case ERegion.Region8:
					return EYearMilestone._2010;
				case ERegion.Region9:
					return EYearMilestone._2020;
			}
			
			throw new ArgumentOutOfRangeException(nameof(regionId), $"Next decade year {regionId} is not defined.");
		}

		public EDecadeMilestone GetNextDecade(EYearMilestone yearMilestone)
		{
			int year = _yearProvider.GetYear(yearMilestone);
			switch (year)
			{
				case < 1940:
					return EDecadeMilestone._1940s;
				case < 1950:
					return EDecadeMilestone._1950s;
				case < 1960:
					return EDecadeMilestone._1960s;
				case < 1970:
					return EDecadeMilestone._1970s;
				case < 1980:
					return EDecadeMilestone._1980s;
				case < 1990:
					return EDecadeMilestone._1990s;
				case < 2000:
					return EDecadeMilestone._2000s;
				case < 2010:
					return EDecadeMilestone._2010s;
				case < 2020:
					return EDecadeMilestone._2020s;
			}
			
			throw new ArgumentOutOfRangeException(nameof(yearMilestone), $"Year milestone {yearMilestone} is not defined.");
		}
	}
}