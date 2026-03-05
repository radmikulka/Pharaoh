// =========================================
// AUTHOR: Radek Mikulka
// DATE:   30.01.2026
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData
{
	public class CTasksDto
	{
		[JsonProperty] public CActiveTaskDto[] DailyTasks { get; set; }
		[JsonProperty] public CWeeklyTaskDto[] WeeklyTasks { get; set; }
		[JsonProperty] public CValuableDto[] DailyCompletionRewards { get; set; }
		[JsonProperty] public bool IsFinalDailyRewardClaimed { get; set; }
		[JsonProperty] public int ClaimedDailyTasksThisWeek { get; set; }
		[JsonProperty] public long WeekRefreshTime { get; set; }
		[JsonProperty] public EStaticContractId UnlockContract { get; set; }

		public CTasksDto()
		{
		}

		public CTasksDto(
			CActiveTaskDto[] dailyTasks, 
			CWeeklyTaskDto[] weeklyTasks, 
			CValuableDto[] dailyCompletionRewards, 
			bool isFinalDailyRewardClaimed, 
			int claimedDailyTasksThisWeek, 
			long weekRefreshTime,
			EStaticContractId unlockContract
			)
		{
			DailyTasks = dailyTasks;
			WeeklyTasks = weeklyTasks;
			WeekRefreshTime = weekRefreshTime;
			DailyCompletionRewards = dailyCompletionRewards;
			ClaimedDailyTasksThisWeek = claimedDailyTasksThisWeek;
			IsFinalDailyRewardClaimed = isFinalDailyRewardClaimed;
			UnlockContract = unlockContract;
		}
	}
}