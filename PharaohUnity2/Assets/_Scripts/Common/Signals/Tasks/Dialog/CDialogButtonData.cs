using System;

namespace Pharaoh
{
	public class CDialogButtonData
	{
		public readonly string Label;
		public readonly Action OnClick;
		public readonly EDialogButtonColor Color;
		public readonly bool IsLocalized;

		public CDialogButtonData(string label, Action onClick, EDialogButtonColor color, bool isLocalized = false)
		{
			Label = label;
			OnClick = onClick;
			Color = color;
			IsLocalized = isLocalized;
		}
	}
}
