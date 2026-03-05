// =========================================
// AUTHOR: Radek Mikulka
// DATE:   20.01.2026
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData
{
	public class COwnedFramesDto : IMapAble
	{
		[JsonProperty] public EProfileFrame[] OwnedFrames { get; set; }

		public COwnedFramesDto()
		{
		}

		public COwnedFramesDto(EProfileFrame[] ownedFrames)
		{
			OwnedFrames = ownedFrames;
		}
	}
}