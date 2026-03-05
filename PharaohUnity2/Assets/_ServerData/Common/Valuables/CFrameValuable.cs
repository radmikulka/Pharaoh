// =========================================
// AUTHOR: Radek Mikulka
// DATE:   20.01.2026
// =========================================

using Newtonsoft.Json;

namespace ServerData
{
	public class CFrameValuable : IValuable
	{
		public EValuable Id => EValuable.Frame;
		
		[JsonProperty] public EProfileFrame Frame { get; private set; }

		public CFrameValuable()
		{
		}

		public CFrameValuable(EProfileFrame frame)
		{
			Frame = frame;
		}

		public string GetAnalyticsValue()
		{
			return $"fr{Frame.ToString()}";
		}

		public override string ToString()
		{
			return $"{nameof(Id)}: {Id}, {nameof(Frame)}: {Frame}";
		}
	}
}