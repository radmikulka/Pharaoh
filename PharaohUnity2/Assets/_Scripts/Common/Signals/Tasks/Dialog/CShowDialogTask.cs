namespace Pharaoh
{
	public class CShowDialogTask
	{
		public string Header { get; private set; }
		public bool IsHeaderLocalized { get; private set; }

		public string Content { get; private set; }
		public bool IsContentLocalized { get; private set; }
		public object[] ContentArgs { get; private set; }

		public bool IsOverlay { get; private set; }
		public bool CanBeClosed { get; private set; } = true;
		public string AnalyticsId { get; private set; }

		public CDialogButtonData ButtonOne { get; private set; }
		public CDialogButtonData ButtonTwo { get; private set; }

		public CShowDialogTask SetHeader(string header)
		{
			Header = header;
			IsHeaderLocalized = false;
			return this;
		}

		public CShowDialogTask SetHeaderLocalized(string key)
		{
			Header = key;
			IsHeaderLocalized = true;
			return this;
		}

		public CShowDialogTask SetContent(string content)
		{
			Content = content;
			IsContentLocalized = false;
			ContentArgs = null;
			return this;
		}

		public CShowDialogTask SetContentLocalized(string key, params object[] args)
		{
			Content = key;
			IsContentLocalized = true;
			ContentArgs = args;
			return this;
		}

		public CShowDialogTask SetOverlay()
		{
			IsOverlay = true;
			return this;
		}

		public CShowDialogTask SetCanBeClosed(bool canBeClosed)
		{
			CanBeClosed = canBeClosed;
			return this;
		}

		public CShowDialogTask SetAnalyticsId(string analyticsId)
		{
			AnalyticsId = analyticsId;
			return this;
		}

		public CShowDialogTask SetOneButton(CDialogButtonData button)
		{
			ButtonOne = button;
			ButtonTwo = null;
			return this;
		}

		public CShowDialogTask SetTwoButtons(CDialogButtonData buttonOne, CDialogButtonData buttonTwo)
		{
			ButtonOne = buttonOne;
			ButtonTwo = buttonTwo;
			return this;
		}
	}
}
