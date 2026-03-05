// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.10.2023
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CDeviceDataDto
	{
		[JsonProperty] public EPlatform Platform { get; set; }
		[JsonProperty] public ELanguageCode LanguageCode { get; set; }
		[JsonProperty] public string PlatformVersion { get; set; }
		[JsonProperty] public string InstallerName { get; set; }
		[JsonProperty] public long TimeZoneOffsetInSecs { get; set; }
		[JsonProperty] public int MemorySize { get; set; }
		[JsonProperty] public string Model { get; set; }

		public CDeviceDataDto()
		{
		}

		public CDeviceDataDto(
			ELanguageCode languageCode, 
			long timeZoneOffsetInSecs, 
			string platformVersion, 
			string installerName, 
			EPlatform platform, 
			int memorySize, 
			string model
			)
		{
			TimeZoneOffsetInSecs = timeZoneOffsetInSecs;
			PlatformVersion = platformVersion;
			InstallerName = installerName;
			LanguageCode = languageCode;
			MemorySize = memorySize;
			Platform = platform;
			Model = model;
		}
	}
}