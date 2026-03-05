// =========================================
// AUTHOR: Juraj Joscak
// DATE:   14.10.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using ServerData;

namespace TycoonBuilder.Ui
{
	public class CUiParticleCountsBase
	{
		private readonly Dictionary<EValuable, CCurrencyCounts> _countsDb = new();

		protected void AddCounts(EValuable id, int[] thresholds, int[] counts)
		{
			_countsDb[id] = new CCurrencyCounts(thresholds, counts);
		}
		
		public int GetCount(EValuable id, int amount)
		{
			if (!_countsDb.ContainsKey(id))
			{
				id = EValuable.None;
			}

			CCurrencyCounts counts = _countsDb[id];
			for (int i = counts.Thresholds.Length-1; i >= 0 ; i--)
			{
				if(amount >= counts.Thresholds[i])
					return counts.Counts[i];
			}
			
			return amount;
		}

		private class CCurrencyCounts
		{
			public readonly int[] Thresholds;
			public readonly int[] Counts;
			
			public CCurrencyCounts(int[] thresholds, int[] counts)
			{
				Thresholds = thresholds;
				Counts = counts;
			}
		}
	}
}