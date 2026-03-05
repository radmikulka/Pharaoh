// =========================================
// AUTHOR: Marek Karaba
// DATE:   14.08.2025
// =========================================

using UnityEngine;

namespace TycoonBuilder
{
	public class CCityBuildingFloatingWindowData : CFloatingWindowData
	{
		public string CityName { get; }

		public CCityBuildingFloatingWindowData(Transform worldPoint, Transform ownerTransform, string cityName) : base(worldPoint, ownerTransform)
		{
			CityName = cityName;
		}
	}
}