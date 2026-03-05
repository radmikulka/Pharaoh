// =========================================
// AUTHOR: Jan Krejsa
// DATE:   24.12.2025
// =========================================

using System;
using UnityEditor;
using UnityEngine;

namespace Editor.ServerLauncher
{
	public enum EServerLauncherLogLevelId
	{
		BuildInformation,
		Trace,
		Debug,
		Information,
		Warning,
		Error,
		Critical,
	}

	public class CServerLauncherLogLevel
	{
		public readonly EServerLauncherLogLevelId Id;
		public readonly string Name;
		public readonly string Tag;
		public readonly LogType UnityLogType;

		public readonly Texture2D IconActive;
		public readonly Texture2D IconInactive;

		public static readonly CServerLauncherLogLevel BuildInformation =
			new(EServerLauncherLogLevelId.BuildInformation, "Build Information", "[BUILD INFO] ", LogType.Log,
				EditorGUIUtility.FindTexture("d_CustomTool@2x"),
				EditorGUIUtility.FindTexture("CustomTool"));
		public static readonly CServerLauncherLogLevel Trace =
			new(EServerLauncherLogLevelId.Trace, "Trace", "[TRACE] ", LogType.Log,
				EditorGUIUtility.FindTexture("d_CollabChanges Icon"),
				EditorGUIUtility.FindTexture("CollabChangesDeleted Icon"));
		public static readonly CServerLauncherLogLevel Debug =
			new(EServerLauncherLogLevelId.Debug, "Debug", "[DEBUG] ", LogType.Log,
				EditorGUIUtility.FindTexture("d_DebuggerEnabled@2x"),
				EditorGUIUtility.FindTexture("d_DebuggerDisabled@2x"));
		public static readonly CServerLauncherLogLevel Information =
			new(EServerLauncherLogLevelId.Information, "Information", "[INFO] ", LogType.Log,
				EditorGUIUtility.FindTexture("console.infoicon.sml@2x"),
				EditorGUIUtility.FindTexture("console.infoicon.inactive.sml@2x"));
		public static readonly CServerLauncherLogLevel Warning =
			new(EServerLauncherLogLevelId.Warning, "Warning", "[WARNING] ", LogType.Warning,
				EditorGUIUtility.FindTexture("console.warnicon.sml@2x"),
				EditorGUIUtility.FindTexture("console.warnicon.inactive.sml@2x"));
		public static readonly CServerLauncherLogLevel Error =
			new(EServerLauncherLogLevelId.Error, "Error", "[ERROR] ", LogType.Error,
				EditorGUIUtility.FindTexture("d_console.erroricon.sml@2x"),
				EditorGUIUtility.FindTexture("d_console.erroricon.inactive.sml@2x"));
		public static readonly CServerLauncherLogLevel Critical =
			new(EServerLauncherLogLevelId.Critical, "Critical", "[CRITICAL] ", LogType.Error,
				EditorGUIUtility.FindTexture("d_CollabConflict Icon"),
				EditorGUIUtility.FindTexture("CollabExclude Icon"));
		
		public static readonly CServerLauncherLogLevel[] AllLogLevels =
		{
			BuildInformation,
			Trace,
			Debug,
			Information,
			Warning,
			Error,
			Critical
		};

		private CServerLauncherLogLevel(EServerLauncherLogLevelId id, string name, string tag, LogType unityLogType, Texture2D iconActive, Texture2D iconInactive)
		{
			Id = id;
			Name = name;
			Tag = tag;
			UnityLogType = unityLogType;
			IconActive = iconActive;
			IconInactive = iconInactive;
		}
		
		public static bool TryExtractLogLevel(ref string message, out CServerLauncherLogLevel result)
		{
			foreach (CServerLauncherLogLevel logLevel in AllLogLevels)
			{
				string tag = logLevel.Tag;
				if (message.StartsWith(tag, StringComparison.OrdinalIgnoreCase))
				{
					message = message.Substring(tag.Length);
					result = logLevel;
					return true;
				}
			}
			
			result = null;
			return false;
		}
		
		public Texture2D GetIcon(bool active)
		{
			return active ? IconActive : IconInactive;
		}
	}
	
	public static class CServerLauncherLogLevelExtensions
	{
		public static CServerLauncherLogLevel GetLogLevel(this EServerLauncherLogLevelId logLevel)
		{
			foreach (var level in CServerLauncherLogLevel.AllLogLevels)
			{
				if (level.Id == logLevel)
					return level;
			}
			return null;
		}
		
		public static LogType ToLogType(this EServerLauncherLogLevelId logLevel)
		{
			var data = GetLogLevel(logLevel);
			return data.UnityLogType;
		}
		
		public static Texture2D GetIcon(this EServerLauncherLogLevelId logLevel, bool active)
		{
			var data = GetLogLevel(logLevel);
			return data.GetIcon(active);
		}

		public static string GetTag(this EServerLauncherLogLevelId logLevel)
		{
			var data = GetLogLevel(logLevel);
			return data.Tag;
		}
	}
}
