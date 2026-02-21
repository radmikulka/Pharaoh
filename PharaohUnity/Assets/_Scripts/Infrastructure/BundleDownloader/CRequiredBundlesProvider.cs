// =========================================
// AUTHOR: Radek Mikulka
// DATE:   11.11.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using ServerData;

namespace Pharaoh
{
	public class CRequiredBundlesProvider : IRequiredBundlesProvider
	{
		public int[] GetBundles(EMissionId mission)
		{
			HashSet<EBundleId> bundles = new();
			GetBaseBundles(bundles);

			return bundles.Select(id => (int)id).ToArray();
		}

		private void GetBaseBundles(HashSet<EBundleId> target)
		{
			target.Add(EBundleId.CoreGame);
			target.Add(EBundleId.CoreGameScenes);
			target.Add(EBundleId.BaseGame);
			target.Add(EBundleId.BaseGameScene);
			target.Add(EBundleId.Mission1_1_Environment);
			target.Add(EBundleId.Mission1_1_Scene);
		}
	}
}