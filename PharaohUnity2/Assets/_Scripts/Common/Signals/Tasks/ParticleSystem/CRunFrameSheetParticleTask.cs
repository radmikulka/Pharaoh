// =========================================
// AUTHOR: Marek Karaba
// DATE:   19.02.2026
// =========================================

using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public class CRunFrameSheetParticleTask
	{
		public readonly RectTransform Start;
		public readonly EProfileFrame FrameId;

		public CRunFrameSheetParticleTask(RectTransform start, EProfileFrame frameId)
		{
			Start = start;
			FrameId = frameId;
		}
	}
}