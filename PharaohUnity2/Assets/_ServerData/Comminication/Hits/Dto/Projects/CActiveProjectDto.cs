// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.02.2026
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData
{
	public class CActiveProjectDto : IMapAble
	{
		[JsonProperty] public CActiveTaskDto[] ActiveTasks { get; set; }
		[JsonProperty] public IRetroactiveTaskDto[] RetroactiveTasks { get; set; }
		[JsonProperty] public EProjectId ProjectId { get; set; }

		public CActiveProjectDto()
		{
		}

		public CActiveProjectDto(CActiveTaskDto[] activeTasks, IRetroactiveTaskDto[] retroactiveTasks, EProjectId projectId)
		{
			ActiveTasks = activeTasks;
			RetroactiveTasks = retroactiveTasks;
			ProjectId = projectId;
		}
	}
}