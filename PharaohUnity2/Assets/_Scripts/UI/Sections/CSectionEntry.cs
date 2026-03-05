// =========================================
// AUTHOR: Marek Karaba
// DATE:   17.07.2025
// =========================================

namespace TycoonBuilder.Ui
{
	public class CSectionEntry
	{
		public readonly CSectionContentBase Content;
		public readonly CSectionButton Button;

		public CSectionEntry(CSectionContentBase content, CSectionButton button)
		{
			Content = content;
			Button = button;
		}
	}
}