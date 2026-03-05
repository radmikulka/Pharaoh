// =========================================
// AUTHOR: Marek Karaba
// DATE:   10.12.2025
// =========================================

namespace ServerData.Logging
{
	public interface IFileWriter
	{
		public void AddItem(object itemText);
		public void AppendLine();
		void WriteToFile(string directory, string fileName);
	}
}

