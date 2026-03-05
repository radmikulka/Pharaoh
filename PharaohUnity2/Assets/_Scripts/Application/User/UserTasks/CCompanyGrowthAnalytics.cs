// =========================================
// AUTHOR: Juraj Joscak
// DATE:   19.02.2026
// =========================================

using System.Collections.Generic;
using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CCompanyGrowthAnalytics
	{
		private readonly IAnalytics _analytics;
		
		private readonly Dictionary<string, object> _cachedParams = new();
		
		public CCompanyGrowthAnalytics(IAnalytics analytics)
		{
			_analytics = analytics;
		}
		
		public void CompanyDailyTasksGenerate(ETaskId[] tasks, long dayRefreshTime)
		{
			long alreadySentDay = CPlayerPrefs.Get("LastSentDailyTasksGenerationDay", (long)-1);
			if(dayRefreshTime == alreadySentDay)
				return;

			for (int i = 0; i < tasks.Length; i++)
			{
				_cachedParams.Clear();
				_cachedParams.Add("Name", tasks[i].ToString());
				_analytics.SendData("CompanyDailyTaskGenerate", _cachedParams);
			}
			
			CPlayerPrefs.Set("LastSentDailyTasksGenerationDay", dayRefreshTime);
			CPlayerPrefs.Save();
		}
		
		public void CompanyDailyTaskAvailable(ETaskId taskId)
		{
			_cachedParams.Clear();
			_cachedParams.Add("Name", taskId.ToString());
			_analytics.SendData("CompanyDailyTaskAvailable", _cachedParams);
		}
		
		public void CompanyDailyTaskClaim(ETaskId taskId)
		{
			_cachedParams.Clear();
			_cachedParams.Add("Name", taskId.ToString());
			_analytics.SendData("CompanyDailyTaskClaim", _cachedParams);
		}
		
		public void CompanyWeeklyTaskGenerate(int[] milestones, long weekRefreshTime)
		{
			long alreadySentWeek = CPlayerPrefs.Get("LastSentWeeklyChallengeGenerationWeek", (long)-1);
			if(weekRefreshTime == alreadySentWeek)
				return;

			for (int i = 0; i < milestones.Length; i++)
			{
				_cachedParams.Clear();
				_cachedParams.Add("Milestone", milestones[i]);
				_analytics.SendData("CompanyWeeklyTaskGenerate", _cachedParams);
			}

			CPlayerPrefs.Set("LastSentWeeklyChallengeGenerationWeek", weekRefreshTime);
			CPlayerPrefs.Save();
		}
		
		public void CompanyWeeklyTaskAvailable(int milestone)
		{
			_cachedParams.Clear();
			_cachedParams.Add("Milestone", milestone);
			_analytics.SendData("CompanyWeeklyTaskAvailable", _cachedParams);
		}
		
		public void CompanyWeeklyTaskClaim(int milestone)
		{
			_cachedParams.Clear();
			_cachedParams.Add("Milestone", milestone);
			_analytics.SendData("CompanyWeeklyTaskClaim", _cachedParams);
		}
	}
}