// =========================================
// AUTHOR: Marek Karaba
// DATE:   10.12.2025
// =========================================

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServerData.Logging
{
	public class CCsvWriter : IFileWriter
	{
		private const char ItemSeparator = ',';

		private readonly List<object> _items = new ();
		private readonly StringBuilder _stringBuilder = new ();

		public void AddItem(object itemText)
		{
			_items.Add(itemText);
		}

		public void AppendLine()
		{
			string line = string.Join(ItemSeparator.ToString(), _items);
			_stringBuilder.AppendLine(line);
			_items.Clear();
		}

		public void WriteToFile(string directory, string fileName)
		{
			string fullPath = Path.Combine(directory, fileName);
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
				
			File.WriteAllText(fullPath, _stringBuilder.ToString());
		}
	}
}

