// =========================================
// AUTHOR: Radek Mikulka
// DATE:   21.12.2023
// =========================================

using AldaEngine;
using ServiceEngine;

namespace Pharaoh
{
	public class CFirebaseRemoteConfig : CBaseFirebaseRemoteConfig<ERemoteConfigKey>, IRemoteConfig
	{
		public CFirebaseRemoteConfig(ILogger logger) : base(logger)
		{
			InitializeDefaults();
		}

		private void InitializeDefaults()
		{
			//AddKey(ERemoteConfigKey.Test, "test", "test");		
			AddKey(ERemoteConfigKey.ShowStoryDialogs, "abtest_show_storydialogs", "1");
		}
	}
}