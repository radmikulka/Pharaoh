// =========================================
// AUTHOR: Jan Krejsa
// DATE:   24.12.2025
// =========================================

using System;
using System.Diagnostics;
using AldaEngine;
using Newtonsoft.Json;

namespace Editor.ServerLauncher
{
	[JsonObject(MemberSerialization.OptIn)]
	public class CServerLauncherLogEntry
	{
		[JsonProperty("@t")]
		public DateTime LogTime { get; }
		public EServerLauncherLogLevelId LogLevel { get; private set; }
		public string Message { get; private set; }
		public int LinkedBuildProcessId { get; set; }
		
		[JsonProperty("@m")]
		private string MessageWithLevel
		{
			get => $"{LogLevel.GetTag()}{Message}";
			set
			{
				LogLevel = CServerLauncherLogLevel.TryExtractLogLevel(ref value, out var logLevel) ? logLevel.Id : EServerLauncherLogLevelId.Information;
				Message = value;
			}
		}

		public CServerLauncherLogEntry(DateTime logTime, EServerLauncherLogLevelId logLevel, string message, int linkedBuildProcessId)
		{
			LogTime = logTime;
			LogLevel = logLevel;
			Message = message;
			LinkedBuildProcessId = linkedBuildProcessId;
		}
	}
}
