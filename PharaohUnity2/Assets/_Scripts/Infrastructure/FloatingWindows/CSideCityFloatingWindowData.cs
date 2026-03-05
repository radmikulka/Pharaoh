// =========================================
// NAME: Marek Karaba
// DATE: 03.10.2025
// =========================================

using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public class CSideCityFloatingWindowData : CFloatingWindowData
	{
		public ECity CityId { get; }

		public CSideCityFloatingWindowData(Transform worldPoint, Transform ownerTransform, ECity cityId) : base(worldPoint, ownerTransform)
		{
			CityId = cityId;
		}
	}
}