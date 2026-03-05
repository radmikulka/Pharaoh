// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.11.2023
// =========================================

using System.Collections.Generic;
using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CAdData : ICustomAdData
	{
		public readonly EAdPlacement Placement;

		public CAdData(EAdPlacement placement)
		{
			Placement = placement;
		}

		public IEnumerable<(string key, object value)> GetAnalyticsData()
		{
			yield return ("Placement", Placement.ToString());
		}

		public override string ToString()
		{
			return Placement.ToString();
		}
	}
}