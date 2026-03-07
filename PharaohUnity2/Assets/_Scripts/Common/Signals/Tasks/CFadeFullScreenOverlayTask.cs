// =========================================
// AUTHOR: Radek Mikulka
// DATE:   01.11.2023
// =========================================

using System;
using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;

namespace Pharaoh
{
	public class CFadeFullScreenOverlayTask
	{
		public readonly Func<CancellationToken, UniTask> OnComplete;
		public readonly float? OverrideBlendInDuration;
		public readonly float? OverrideBlendOutDuration;
		
		public CFadeFullScreenOverlayTask(Func<CancellationToken, UniTask> onComplete, float? overrideBlendInDuration = null, float? overrideBlendOutDuration = null)
		{
			OverrideBlendInDuration = overrideBlendInDuration;
			OverrideBlendOutDuration = overrideBlendOutDuration;
			OnComplete = onComplete;
		}
	}
}