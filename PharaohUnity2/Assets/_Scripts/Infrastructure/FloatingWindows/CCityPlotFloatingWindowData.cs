// =========================================
// AUTHOR: Marek Karaba
// DATE:   15.08.2025
// =========================================

using UnityEngine;

namespace TycoonBuilder
{
	public class CCityPlotFloatingWindowData : CFloatingWindowData
	{
		public int Index { get; private set; }
		
		public CCityPlotFloatingWindowData(Transform worldPoint, Transform ownerTransform) : base(worldPoint, ownerTransform)
		{
		}
		
		public void SetIndex(int index)
		{
			Index = index;
		}
	}
}