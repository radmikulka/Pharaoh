using System.Collections.Generic;
using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CRemoteConfigDto
	{
		[JsonProperty] public Dictionary<string, string> Values { get; set; }
	}
}