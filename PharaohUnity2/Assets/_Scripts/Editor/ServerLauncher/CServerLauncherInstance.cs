// =========================================
// AUTHOR: Jan Krejsa
// DATE:   22.10.2025
// =========================================

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;
using System.Reflection;
using UnityEditor;
using UnityEngine.Assertions;
using Assert = NUnit.Framework.Assert;

namespace Editor.ServerLauncher
{
	[Serializable]
	public class CServerLauncherInstance
	{
		[SerializeField] private string _serverName;
		[Label("Build & Run Command Path (relative to Project folder)")]
		[SerializeField] private string _relativeCommandPath;
		[Label("Solution Path (relative to Project folder)")]
		[SerializeField] private string _relativeSolutionPath;
		[SerializeField] private string _relativeCsprojPath;
		[SerializeField] private string _processFileName;
		[SerializeField] private string[] _additionalProcessFilePathTokens;
		
		[SerializeReference] [SerializeReferenceDropdown] private IServerCommandPreset[] _supportedServerCommands = Array.Empty<IServerCommandPreset>();

		[NonSerialized] private EServerLauncherInstanceState _cachedState;
		
		public string ServerName => _serverName;
		public string BuildAndRunCommandPathRelative => _relativeCommandPath;
		public string BuildAndRunCommandPathFull => Path.GetFullPath(Path.Combine(Application.dataPath, "../", BuildAndRunCommandPathRelative)).Replace("\\", "/");
		public string RelativeSolutionPath => _relativeSolutionPath;
		public string FullSolutionPath => Path.GetFullPath(Path.Combine(Application.dataPath, "../", RelativeSolutionPath)).Replace("\\", "/");
		public string FullCsprojPath => Path.GetFullPath(Path.Combine(Application.dataPath, "../", _relativeCsprojPath)).Replace("\\", "/");
		public string ProcessFileName => _processFileName;
		public string[] AdditionalProcessFilePathTokens => _additionalProcessFilePathTokens;
		public IServerCommandPreset[] SupportedServerCommands => _supportedServerCommands;
		
		public bool HasCommandPath => !string.IsNullOrWhiteSpace(_relativeCommandPath);
		public bool HasSolutionPath => !string.IsNullOrWhiteSpace(_relativeSolutionPath);
		
		[field: NonSerialized]
		public Process CachedRunningProcess { get; set; }
		[field: NonSerialized]
		public Process BuildProcess { get; set; }
		public CServerLauncherLogger Logger { get; } = new CServerLauncherLogger();

		private int BuildProcessId
		{
			get => EditorPrefs.GetInt($"{ServerName}_{PlayerSettings.productGUID}_BuildProcessId", -1);
			set => EditorPrefs.SetInt($"{ServerName}_{PlayerSettings.productGUID}_BuildProcessId", value);
		}
		
		public event Action StateChanged;
		
		public EServerLauncherInstanceState CachedState
		{
			get => _cachedState;
			private set
			{
				if (_cachedState == value)
				{
					return;
				}

				_cachedState = value;
				StateChanged?.Invoke();
			}
		}
		

		public void BuildAndRun(bool suppressBuildErrorDialog = false)
		{
			if (string.IsNullOrEmpty(BuildAndRunCommandPathRelative))
			{
				EditorUtility.DisplayDialog("Error", $"Command path for '{ServerName}' is not configured.", "OK");
				return;
			}
		
			KillIfRunning();
			string workingDirectory = Path.GetDirectoryName(BuildAndRunCommandPathFull);
			
			BuildProcess = new Process
			{
				StartInfo =
				{
					FileName = BuildAndRunCommandPathFull,
					WorkingDirectory = workingDirectory,
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					RedirectStandardInput = true,
					StandardInputEncoding = Encoding.ASCII
				}
			};

			Logger.Initialize(ServerName, BuildProcess, suppressBuildErrorDialog);
			BuildProcess.Start();
			BuildProcessId = BuildProcess.Id;
			Logger.BeginLogging();
		}
		
		public void StartHotReload(bool suppressBuildErrorDialog = false)
		{
			Assert.IsNotEmpty(_relativeCsprojPath);
			
			string csprojDir = Path.GetDirectoryName(FullCsprojPath);
			
			KillIfRunning();

			BuildProcess = new Process
			{
				StartInfo =
				{
					FileName = "dotnet",
					Arguments = "watch run --non-interactive", 
					WorkingDirectory = csprojDir,
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					RedirectStandardInput = true,
					EnvironmentVariables = 
					{
						{ "DOTNET_USE_POLLING_FILE_WATCHER", "1" },
						{ "DOTNET_WATCH_POLLING_INTERVAL", "500" },
						{ "DOTNET_WATCH_RELOAD_ON_FILE_CHANGE", "1" }
					}
				}
			};

			Logger.Initialize(ServerName, BuildProcess, suppressBuildErrorDialog);
			BuildProcess.Start();
			BuildProcessId = BuildProcess.Id;
			Logger.BeginLogging();
		}
		
        public void SendInputToBuildProcess(string input)
        {
            try
            {
                if (BuildProcess != null && !BuildProcess.HasExited && BuildProcess.StartInfo.RedirectStandardInput)
                {
	                BuildProcess.StandardInput.WriteLine(input);
	                BuildProcess.StandardInput.Flush();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to send input to build process for '{ServerName}': {ex.Message}");
            }
        }

		public void OpenSolutionInIde(string fileName = null, int lineNumber = -1)
		{
			string idePath = GetExternalScriptEditorPathViaReflection();

			if (string.IsNullOrEmpty(RelativeSolutionPath))
			{
				EditorUtility.DisplayDialog("Error", $"Solution path for '{ServerName}' is not configured.", "OK");
				return;
			}
			
			StringBuilder argsBuilder = new StringBuilder();
			argsBuilder.Append($"\"{RelativeSolutionPath}\"");
			if (!string.IsNullOrEmpty(fileName))
			{
				argsBuilder.Append($" \"{fileName}\"");
				if (lineNumber > 0)
				{
					argsBuilder.Append($" --line {lineNumber} ");
				}
			}
			
			try
			{
				var process = new Process
				{
					StartInfo =
					{
						FileName = idePath,
						Arguments = argsBuilder.ToString(),
						UseShellExecute = false,
						CreateNoWindow = true
					}
				};
				
				process.Start();
			}
			catch (Exception ex)
			{
				Debug.LogError("Failed to open solution: " + ex.Message);
			}
		}

		public void KillIfRunning()
		{
			if (BuildProcess != null && !BuildProcess.HasExited)
			{
				BuildProcess.Kill();
				BuildProcess = null;
			}
			
			if (TryFindBuildProcess(out var buildProcess))
			{
				buildProcess.Kill();
				BuildProcess = null;
			} 
			
			if (CachedRunningProcess != null && !CachedRunningProcess.HasExited)
			{
				CachedRunningProcess.Kill();
				CachedRunningProcess = null;
			}
		}

		private bool TryFindBuildProcess(out Process buildProcess)
		{
			buildProcess = null;
			int buildProcessId = BuildProcessId;
			if (buildProcessId >= 0)
			{
				try
				{
					buildProcess = Process.GetProcessById(buildProcessId);
					return buildProcess != null && !buildProcess.HasExited;
				}
				catch (Exception)
				{
					return false;
				}
			}
			return false;
		}

		public void UpdateState()
		{
			bool isRunning = CheckIfRunning();
			if (isRunning)
			{
				CachedState = EServerLauncherInstanceState.Running;
				return;
			}
			
			if (BuildProcess != null && !BuildProcess.HasExited)
			{
				CachedState = EServerLauncherInstanceState.Launching;
				return;
			}
			
			CachedState = EServerLauncherInstanceState.NotRunning;
		}

		private bool CheckIfRunning()
		{
			if (CachedRunningProcess != null && !CachedRunningProcess.HasExited)
			{
				return true;
			}
			
			if (TryFindProcess(out var foundProcess))
			{
				CachedRunningProcess = foundProcess;
				return true;
			}
			
			return false;
		}

		

		public async UniTask<bool> WaitUntilRunning(CancellationToken ct)
		{
			UpdateState();
			while ((CachedState == EServerLauncherInstanceState.Launching || CachedState == EServerLauncherInstanceState.UnknownState) && !ct.IsCancellationRequested)
			{
				await UniTask.Yield();
			}

			return CachedState == EServerLauncherInstanceState.Running;
		}
		
		private bool TryFindProcess(out Process result)
		{
			var processes = Process.GetProcessesByName(ProcessFileName);
			foreach (var process in processes)
			{
				bool isMatchingProcess = !process.HasExited && IsMatchingPath(process.MainModule?.FileName, AdditionalProcessFilePathTokens);
				if (isMatchingProcess)
				{
					result = process;
					return true;
				}
			}

			result = null;
			return false;
		}
		
		private static bool IsMatchingPath(string path, string[] requiredPathTokens) =>
			!string.IsNullOrEmpty(path) && (requiredPathTokens.Length < 1 || requiredPathTokens.All(path.Contains));
		
		private static string GetExternalScriptEditorPathViaReflection()
		{
			try
			{
				Type preferencesType = typeof(EditorPrefs).Assembly.GetType("UnityEditor.Preferences");
				if (preferencesType != null)
				{
					MethodInfo getMethod = preferencesType.GetMethod(
						"GetExternalScriptEditor",
						BindingFlags.Static | BindingFlags.NonPublic
					);

					if (getMethod != null)
					{
						string path = getMethod.Invoke(null, null) as string;
						return path;
					}
				}

				// Fallback: use EditorPrefs (if Unity stored it there)
				return EditorPrefs.GetString("kScriptsDefaultApp");
			}
			catch (Exception ex)
			{
				Debug.LogError("Failed to get external script editor path: " + ex.Message);
				return null;
			}
		}
	}
}