// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.07.2024
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CGlobalVariableDto : IMapAble
	{
		[JsonProperty] public EGlobalVariable Id { get; set; }
		[JsonProperty] public string StringValue { get; set; }

		public CGlobalVariableDto()
		{
		}

		public CGlobalVariableDto(EGlobalVariable id, string value)
		{
			Id = id;
			StringValue = value;
		}
		
		public CGlobalVariableDto(EGlobalVariable id, bool value)
		{
			Id = id;
			StringValue = value ? "1" : "0";
		}
	}
}