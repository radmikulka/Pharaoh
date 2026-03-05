// =========================================
// AUTHOR: Marek Karaba
// DATE:   07.08.2025
// =========================================

using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public class CResourceFloatingWindowData : CFloatingWindowData
	{
		public EIndustry Industry { get; }

		public CResourceFloatingWindowData(Transform worldPoint, Transform ownerTransform, EIndustry industry) : base(worldPoint, ownerTransform)
		{
			Industry = industry;
		}
	}
}