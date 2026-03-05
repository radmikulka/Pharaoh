// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.12.2023
// =========================================

using AldaEngine;
using UnityEngine;
using UnityEngine.UI;

namespace TycoonBuilder
{
	public class CShowTutorialHighlightTask
	{
		public readonly ITutorialHighlightPoint Target;
		public Vector2 AnchoredOffset { get; private set; }
		public Vector2 SizeOffset { get; private set; }
		public ETutorialHighlightRectType Type { get; private set; }

		public CShowTutorialHighlightTask(RectTransform target, ETutorialHighlightRectType type)
		{
			Target = new CRectTutorialHighlightPoint(target);
			Type = type;
		}
		
		public CShowTutorialHighlightTask(ITutorialHighlightPoint target, ETutorialHighlightRectType type)
		{
			Target = target;
			Type = type;
		}
		
		public CShowTutorialHighlightTask SetAnchoredOffset(float x, float y)
		{
			AnchoredOffset = new Vector2(x, y);
			return this;
		}
		
		public CShowTutorialHighlightTask SetSizeOffset(float x, float y)
		{
			SizeOffset = new Vector2(x, y);
			return this;
		}
	}
}