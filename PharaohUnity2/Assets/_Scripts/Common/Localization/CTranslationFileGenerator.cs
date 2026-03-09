#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Pharaoh
{
	public static class CTranslationFileGenerator
	{
		[MenuItem("Tools/Localizations/Generate Runtime Files")]
		public static void GenerateRuntimeFiles()
		{
			string sourcePath = "Assets/_Sources/Localizations";
			string runtimePath = "Assets/_Sources/Localizations/Generated";

			Directory.CreateDirectory(runtimePath);

			string[] sourceFiles = Directory.GetFiles(sourcePath, "*.txt");
			StringBuilder buildFileContent = new();

			foreach (string sourceFile in sourceFiles)
			{
				string fileName = Path.GetFileNameWithoutExtension(sourceFile);
				string[] lines = File.ReadAllLines(sourceFile);
				StringBuilder runtimeContent = new();

				foreach (string line in lines)
				{
					string trimmed = line.Trim();
					if (string.IsNullOrEmpty(trimmed))
					{
						continue;
					}

					int firstSeparator = trimmed.IndexOf(';');
					if (firstSeparator < 0)
					{
						continue;
					}

					string key = trimmed.Substring(0, firstSeparator);
					string remaining = trimmed.Substring(firstSeparator + 1);

					int secondSeparator = remaining.IndexOf(';');
					string value = secondSeparator >= 0 ? remaining.Substring(0, secondSeparator) : remaining;

					runtimeContent.AppendLine($"{key};{value}");
				}

				string outputPath = Path.Combine(runtimePath, $"{fileName}.txt");
				File.WriteAllText(outputPath, runtimeContent.ToString());

				buildFileContent.AppendLine(fileName);
			}

			string buildFilePath = Path.Combine(runtimePath, "build.txt");
			File.WriteAllText(buildFilePath, buildFileContent.ToString());

			AssetDatabase.Refresh();
			Debug.Log($"[CTranslationFileGenerator] Generated runtime files in {runtimePath}");
		}
	}
}
#endif
