// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.08.2025
// =========================================

using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public class CFocusCameraOnPointTask
	{
		public readonly Vector3 TargetPoint;
		public readonly float Zoom;
        
		public CFocusCameraOnPointTask(Vector3 targetPoint, float zoom = 0)
		{
			TargetPoint = targetPoint;
			Zoom = zoom;
		}
	}
	
	public class CFocusCameraOnRegionPointTask
	{
		public readonly ERegionPoint TargetPoint;
		public readonly ERegion Region;
		
		public CFocusCameraOnRegionPointTask(ERegionPoint targetPoint, ERegion region)
		{
			TargetPoint = targetPoint;
			Region = region;
		}
	}
	
	public class CFocusCameraOnRegionPointInstantTask
	{
		public readonly ERegionPoint TargetPoint;
		public readonly ERegion Region;
		
		public CFocusCameraOnRegionPointInstantTask(ERegionPoint targetPoint, ERegion region)
		{
			TargetPoint = targetPoint;
			Region = region;
		}
	}
}