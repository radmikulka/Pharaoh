// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.07.2024
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CGlobalVariablesDto : IMapAble
	{
		[JsonProperty] public CGlobalVariableDto[] GlobalVariables { get; set; }

		public CGlobalVariablesDto()
		{
		}

		public CGlobalVariablesDto(CGlobalVariableDto[] globalVariables)
		{
			GlobalVariables = globalVariables;
		}
	}
}