// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.02.2026
// =========================================

using Newtonsoft.Json;

namespace ServerData
{
	public class CProjectsDto
	{
		[JsonProperty] public CActiveProjectDto[] Projects { get; set; }

		public CProjectsDto()
		{
		}

		public CProjectsDto(CActiveProjectDto[] projects)
		{
			Projects = projects;
		}
	}
}