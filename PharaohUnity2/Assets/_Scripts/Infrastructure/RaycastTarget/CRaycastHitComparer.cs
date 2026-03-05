// =========================================
// AUTHOR: Marek Karaba
// DATE:   03.07.2025
// =========================================

using System.Collections.Generic;
using UnityEngine;

namespace TycoonBuilder
{
	public class CRaycastHitComparer : IComparer<RaycastHit>
	{
		public int Compare(RaycastHit x, RaycastHit y)
		{
			return x.distance.CompareTo(y.distance);
		}
	}
}