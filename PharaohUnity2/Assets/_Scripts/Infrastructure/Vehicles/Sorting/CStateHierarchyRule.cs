// =========================================
// AUTHOR: Marek Karaba
// DATE:   01.02.2026
// =========================================

using System.Collections.Generic;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public class CStateHierarchyRule : IVehicleSortingRule
	{
		private readonly List<EVehicleState> _priorityOrder = new ()
		{
			EVehicleState.ReadyToCollect,
			EVehicleState.ReadyToSend,
			EVehicleState.DispatchedCurrent,
			EVehicleState.LowDurability,
			EVehicleState.DispatchedOther,
			EVehicleState.DispatchedTransportFleet,
        
			EVehicleState.NotOwned,
			EVehicleState.Locked
		};

		public int Compare(SVehicleSortContext a, SVehicleSortContext b)
		{
			if (a.State == b.State)
				return 0;

			int rankA = _priorityOrder.IndexOf(a.State);
			int rankB = _priorityOrder.IndexOf(b.State);
			
			if (rankA == -1) rankA = int.MaxValue;
			if (rankB == -1) rankB = int.MaxValue;
			return rankA.CompareTo(rankB);
		}
	}
}