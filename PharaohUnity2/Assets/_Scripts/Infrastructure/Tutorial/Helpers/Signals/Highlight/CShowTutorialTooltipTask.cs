// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.12.2023
// =========================================

using AldaEngine;
using UnityEngine;

namespace TycoonBuilder
{
	public class CShowTutorialTooltipTask : IEventBusSignal
	{
		public readonly Transform Target;
		public readonly string TextLangKey;
		
		public Vector2 AnchoredOffset { get; private set; }
		public Vector2 SizeOffset { get; private set; }
		public ETutorialTooltipSide Side { get; private set; }
		public bool ShowAvatar { get; private set; }
		public bool ShowContinueButton { get; private set; }

		public CShowTutorialTooltipTask(Transform target, string textLangKey)
		{
			Target = target;
			TextLangKey = textLangKey;
		}
		
		public CShowTutorialTooltipTask SetAnchoredOffset(float x, float y)
		{
			// tady je hnusny hack - tooltip okno nebylo vycentovane v prefabu a vsude je to
			// už nastavene s tim ze to pocita s offsetem :( tak se tooltip vycentroval
			// a ten ofset se dal zde at se to nemu vsude predelavat
			AnchoredOffset = new Vector2(x, y) + new Vector2(244f, -86f);
			return this;
		}
		
		public CShowTutorialTooltipTask SetSide(ETutorialTooltipSide side)
		{
			Side = side;
			return this;
		}
		
		public CShowTutorialTooltipTask SetShowAvatar(bool showAvatar)
		{
			ShowAvatar = showAvatar;
			return this;
		}
		
		public CShowTutorialTooltipTask SetShowContinueButton(bool showContinueButton)
		{
			ShowContinueButton = showContinueButton;
			return this;
		}
		
		public CShowTutorialTooltipTask SetSizeOffset(float x, float y)
		{
			SizeOffset = new Vector2(x, y);
			return this;
		}
	}
}