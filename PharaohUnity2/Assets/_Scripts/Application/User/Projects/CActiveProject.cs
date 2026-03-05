// =========================================
// AUTHOR: Radek Mikulka
// DATE:   24.02.2026
// =========================================

using System.Linq;
using ServerData;

namespace TycoonBuilder
{
	public class CActiveProject
	{
		public readonly EProjectId ProjectId;
		public readonly CActiveTask[] ActiveTasks;
		public readonly CRetroactiveProjectTask[] RetroactiveTasks;

		public CActiveProject(EProjectId projectId, CActiveTask[] activeTasks, CRetroactiveProjectTask[] retroactiveTasks)
		{
			ProjectId = projectId;
			ActiveTasks = activeTasks;
			RetroactiveTasks = retroactiveTasks;
		}

		public bool IsClaimable(CUser user)
		{
			bool allActiveTasksClaimed = ActiveTasks.All(t => t.IsClaimed);
			bool allRetroactiveCompleted = RetroactiveTasks.All(t => t.IsCompleted(user));
			return allActiveTasksClaimed && allRetroactiveCompleted;
		}
	}
}
