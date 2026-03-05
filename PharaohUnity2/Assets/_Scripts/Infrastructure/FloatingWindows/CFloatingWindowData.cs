// =========================================
// AUTHOR: Radek Mikulka
// DATE:   15.08.2025
// =========================================

using UnityEngine;

namespace TycoonBuilder
{
	public abstract class CFloatingWindowData : IFloatingWindowData
	{
		public Transform WorldPoint { get; }
		public Transform OwnerTransform { get; }

		protected CFloatingWindowData(Transform worldPoint, Transform ownerTransform)
		{
			WorldPoint = worldPoint;
			OwnerTransform = ownerTransform;
		}
	}
}