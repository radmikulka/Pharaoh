// =========================================
// AUTHOR: Jan Krejsa
// DATE:   12.12.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Editor.ServerLauncher
{
	public class CServerLauncherLogger
	{
		public List<CServerLauncherLogEntry> LogEntries { get; } = new();
		public bool HasBuildFinished
		{
			get => _hasBuildFinishedBackingField;
			private set
			{
				_hasBuildFinishedBackingField = value;
				EventBuildFinishedChanged?.Invoke();
			}
		}
		public Action EventBuildFinishedChanged { get; set; }
		public Action<CServerLauncherLogEntry> EventLogEntryReceived { get; set; }
		
		private string ServerName { get; set; }
		public Process AttachedProcess { get; set; }

		private StringBuilder StandardLogBuilder { get; } = new();
		private StringBuilder ErrorLogBuilder { get; } = new();
		
		private DateTime LastStandardLogReceiveTime { get; set; }
		private DateTime LastErrorLogReceiveTime { get; set; }
		
		private const string LogColor = "#FFA000";

		private readonly Dictionary<CServerLauncherLogLevel, int> _logCountByLevel = new();
		private readonly TimeSpan _logFlushDelay = TimeSpan.FromSeconds(0.1);
		private string LogHeader { get; set; }
		private CancellationTokenSource CancellationTokenSource { get; } = new();
		private bool _hasBuildFinishedBackingField;
		private bool _suppressBuildErrorDialog;
		
		public void Initialize(string serverName, Process buildProcess, bool suppressBuildErrorDialog)
		{ 
			HasBuildFinished = false;
			ServerName = serverName;
			AttachedProcess = buildProcess;
			_suppressBuildErrorDialog = suppressBuildErrorDialog;
			AttachedProcess.OutputDataReceived += HandleOutputDataReceived;
			AttachedProcess.ErrorDataReceived += HandleErrorDataReceived;
			LogHeader = $"<color={LogColor}>[{ServerName}]</color>";
			AttachedProcess.Exited += HandleProcessExited;
		}

		public void BeginLogging()
		{
			AttachedProcess.BeginOutputReadLine();
			AttachedProcess.BeginErrorReadLine();
			EditorApplication.update += EditorUpdate;
		}
		
		public void ClearLogs()
		{
			LogEntries.Clear();
			_logCountByLevel.Clear();
		}

		public void ClearOldLogs(int keepBuildProcessId)
		{
			LogEntries.RemoveAll(e => e.LinkedBuildProcessId != keepBuildProcessId);
			RecalculateLogCountByLevel();
		}

		public int GetLogCountByLevel(CServerLauncherLogLevel level) => _logCountByLevel.GetValueOrDefault(level, 0);

		private void HandleOutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			ProcessLine(e.Data, StandardLogBuilder);
			LastStandardLogReceiveTime = DateTime.Now;
		}

		private void HandleErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			ProcessLine(e.Data, ErrorLogBuilder);
			LastErrorLogReceiveTime = DateTime.Now;
		}

		[HideInCallstack]
		private void ProcessLine(string line, StringBuilder logBuilder)
		{
			if (line == "----------------------------------------")
			{
				Flush(logBuilder);
				return;
			}
			
			if (!string.IsNullOrWhiteSpace(line))
			{
				logBuilder.AppendLine(line);
			}
		}
		
		[HideInCallstack]
		private void Flush(StringBuilder logBuilder)
		{
			if (logBuilder.Length > 0)
			{
				ProcessLog(logBuilder.ToString(), logBuilder == ErrorLogBuilder);
				logBuilder.Clear();
			}
		}
		
		[HideInCallstack]
		private void ProcessLog(string log, bool isFromErrorStream)
		{
			string lowerCase = log.ToLower();

			if (lowerCase.Contains("system cannot find the file"))
			{
				// expected error during 'rmdir' command when cleaning build folders - ignore
				return;
			}

			const int maxLength = 1024;
			if (log.Length > maxLength)
			{
				log = log.Substring(0, maxLength);
			}

			var logLevel = ExtractLogLevel(ref log, isFromErrorStream);
			CServerLauncherLogHyperlinkHandler.AddHyperlinkTags(ref log, ServerName);

			if (lowerCase.Contains("server listening"))
			{
				HasBuildFinished = true;
			}
			
			bool forwardToUnityConsole = logLevel.Id >= EServerLauncherLogLevelId.Warning || lowerCase.Contains("server listening") || lowerCase.Contains("build started");
			Log(logLevel, log, forwardToUnityConsole);
			
			if (lowerCase.Contains("system cannot find the path"))
			{
				if (!_suppressBuildErrorDialog)
				{
					EditorUtility.DisplayDialog(
						$"{ServerName} build failed", 
						"See errors in console.", 
						"OK");
				}
			}
		}
		
		private CServerLauncherLogLevel ExtractLogLevel(ref string log, bool isFromErrorStream)
		{
			if (CServerLauncherLogLevel.TryExtractLogLevel(ref log, out var logLevel))
				return logLevel;
			
			if (isFromErrorStream || log.Contains("error", StringComparison.OrdinalIgnoreCase) && !log.Contains("0 error", StringComparison.OrdinalIgnoreCase)
			                      || log.Contains("exception", StringComparison.OrdinalIgnoreCase))
				return CServerLauncherLogLevel.Error;
			
			if (!HasBuildFinished)
				return CServerLauncherLogLevel.BuildInformation;
			
			return CServerLauncherLogLevel.Information;
		}

		private void Log(CServerLauncherLogLevel logLevel, string message, bool forwardToUnityConsole)
		{
			var entry = new CServerLauncherLogEntry(DateTime.Now, logLevel.Id, message, AttachedProcess?.Id ?? -1);
			LogEntries.Add(entry);
			_logCountByLevel[logLevel] = GetLogCountByLevel(logLevel) + 1;
			
			if (forwardToUnityConsole)
			{
				var logType = logLevel.Id.ToLogType();
				LogToUnityConsoleOnMainThread(logType, message).Forget();
			}
			
			InvokeLogEntryReceivedEventOnMainThread(entry).Forget();
		}

		private async UniTaskVoid InvokeLogEntryReceivedEventOnMainThread(CServerLauncherLogEntry entry)
		{
			await UniTask.SwitchToMainThread(CancellationTokenSource.Token);
			EventLogEntryReceived?.Invoke(entry);
		}
		
		private async UniTask LogToUnityConsoleOnMainThread(LogType logType, string message)
		{
			await UniTask.SwitchToMainThread(CancellationTokenSource.Token);
			var previousStackTraceLogType = Application.GetStackTraceLogType(logType);
			Application.SetStackTraceLogType(logType, StackTraceLogType.None);
			Debug.unityLogger.Log(logType, $"{LogHeader} {message}");
			Application.SetStackTraceLogType(logType, previousStackTraceLogType);
		}
		
		private void RecalculateLogCountByLevel()
		{
			_logCountByLevel.Clear();
			foreach (var entry in LogEntries)
			{
				var level = entry.LogLevel.GetLogLevel();
				_logCountByLevel[level] = GetLogCountByLevel(level) + 1;
			}
		}

		private void EditorUpdate()
		{
			if (DateTime.Now >= LastStandardLogReceiveTime + _logFlushDelay)
			{
				Flush(StandardLogBuilder);
			}
			if (DateTime.Now >= LastErrorLogReceiveTime + _logFlushDelay)
			{
				Flush(ErrorLogBuilder);
			}
		}
		
		private void HandleProcessExited(object sender, EventArgs e)
		{
			EditorApplication.update -= EditorUpdate;
			AttachedProcess.OutputDataReceived -= HandleOutputDataReceived;
			AttachedProcess.ErrorDataReceived -= HandleErrorDataReceived;
			CancellationTokenSource.Cancel();
			Flush(StandardLogBuilder);
			Flush(ErrorLogBuilder);
		}
	}
}
