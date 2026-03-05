// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.12.2023
// =========================================

using AldaEngine;
using UnityEngine;

namespace TycoonBuilder
{
	public class CShowTutorialArrowTask
	{
		public readonly ITutorialGraphicsTarget Target;
		public Vector2 AnchoredOffset { get; private set; }
		public float ClockwiseArrowRotation { get; private set; }
		public float DelayInSecsBeforeShow { get; private set; } = 0.3f;

		public CShowTutorialArrowTask(ITutorialGraphicsTarget target)
		{
			Target = target;
		}

		public CShowTutorialArrowTask(RectTransform target)
		{
			Target = new CTutorialRectTarget(target);
		}
		
		public CShowTutorialArrowTask SetAnchoredOffset(float x, float y)
		{
			AnchoredOffset = new Vector2(x, y);
			return this;
		}
		
		public CShowTutorialArrowTask SetDelayBeforeShowInSecs(float delayInSecs)
		{
			DelayInSecsBeforeShow = delayInSecs;
			return this;
		}

		public CShowTutorialArrowTask SetClockwiseArrowRotation(float clockwiseArrowRotation)
		{
			ClockwiseArrowRotation = clockwiseArrowRotation;
			return this;
		}
	}
}