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
		public int[] GetBundles(EMissionId region)
		{
			HashSet<EBundleId> bundles = new();
			GetBaseBundles(bundles);
			return bundles.Select(id => (int)id).ToArray();
		}

		private void GetBaseBundles(HashSet<EBundleId> target)
		{
			target.Add(EBundleId.BaseGame);
		}
	}
}