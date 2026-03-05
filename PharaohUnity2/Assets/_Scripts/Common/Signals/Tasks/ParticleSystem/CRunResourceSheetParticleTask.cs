// =========================================
// AUTHOR: Juraj Joscak
// DATE:   07.10.2025
// =========================================

using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public class CRunResourceSheetParticleTask
	{
		public readonly RectTransform Start;
		public readonly SResource Resource;

		public CRunResourceSheetParticleTask(RectTransform start, SResource resource)
		{
			Start = start;
			Resource = resource;
		}
	}
}