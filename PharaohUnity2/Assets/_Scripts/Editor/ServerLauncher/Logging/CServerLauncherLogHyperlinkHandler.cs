// =========================================
// AUTHOR: Jan Krejsa
// DATE:   24.12.2025
// =========================================

using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Editor.ServerLauncher
{
	public static class CServerLauncherLogHyperlinkHandler
	{
		private const string HyperlinkScriptKey = "script";
		private const string HyperlinkServerNameKey = "server";
		
		static CServerLauncherLogHyperlinkHandler()
		{
			EditorGUI.hyperLinkClicked += HandleHyperlinkClicked;
		}
		
		public static void AddHyperlinkTags(ref string log, string serverName)
		{
			const int maxReplacements = 5;
			int count = 0;
			
			const string pattern = @"(?<path>[A-Za-z]:[^\r\n:]+?\.cs)(?::(?:line\s*)?(?<line>\d+)|\((?<line>\d+)(?:,\d+)?\))";
			log = Regex.Replace(log, pattern, match =>
			{
				if (count++ >= maxReplacements)
					return match.Value; // stop replacing after maxReplacements reached
				
				string rawPath = match.Groups["path"].Value.Trim();
				string line = match.Groups["line"].Value;
				string openPath = rawPath;
				int idx = openPath.IndexOf(" (at ", StringComparison.Ordinal);
				if (idx >= 0) openPath = openPath.Substring(idx + 5).Trim(' ', '\'', '"', ')');
				openPath = openPath.Trim('(', ')', ' ');
				string path = $"{openPath.Replace("\\", "/")}";
				return $"<a href=\" \" {HyperlinkScriptKey}=\"{path}\" line=\"{line}\" {HyperlinkServerNameKey}=\"{serverName}\">{match.Value}</a>";
			});
		}

		private static void HandleHyperlinkClicked(EditorWindow editorWindow, HyperLinkClickedEventArgs args)
		{
			try
			{
				if (args == null || !args.hyperLinkData.TryGetValue(HyperlinkScriptKey, out string filePath) || string.IsNullOrEmpty(filePath)) 
					return;
				if (!args.hyperLinkData.TryGetValue("line", out string lineNumStr) || !int.TryParse(lineNumStr, out int lineNum)) 
					lineNum = 1;
				
				string absPath = filePath.Replace("/", Path.DirectorySeparatorChar.ToString());
				if (args.hyperLinkData.TryGetValue(HyperlinkServerNameKey, out string serverName) && CServerLauncherConfig.Instance.TryGetServerByName(serverName, out CServerLauncherInstance serverInstance) 
					|| TryGetServerInstanceFromLogWindow(editorWindow, out serverInstance))
				{
					serverInstance.OpenSolutionInIde(absPath, lineNum);
				}
				else
				{
					UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(absPath, lineNum);
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"Failed to open hyperlink target: {ex.Message}");
			}
			
			bool TryGetServerInstanceFromLogWindow(EditorWindow window, out CServerLauncherInstance serverInstance)
			{
				serverInstance = null;
				if (window is CServerLauncherLogWindow logWindow)
				{
					serverInstance = logWindow.LinkedServerInstance;
					return true;
				}
				return false;
			}
		}
	}
}
