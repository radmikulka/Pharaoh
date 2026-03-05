// =========================================
// AUTHOR: Jan Krejsa
// DATE:   21.10.2025
// =========================================


using System;
using System.Collections.Generic;
using System.Threading;
using AldaEngine;
using AldaEngine.EntrySceneLauncher;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEditor;

namespace Editor.ServerLauncher
{
	[AssetPath("_Sources/Configs", "ServerLauncherConfig")]
	[CExecuteBeforeEntrySceneLaunch("OnEntrySceneWillLaunch")]
	public class CServerLauncherConfig: CScriptableSingletonEditorOnly<CServerLauncherConfig>
	{
		private const string OnClickBehaviourEditorPrefsKey = "ServerLauncherBehaviour";
		private const string OnPlayBehaviourEditorPrefsKey = "ServerLauncherPlayBehaviour";
		private const double NotRunningUpdateIntervalSeconds = 1f;
		private const double RunningUpdateIntervalSeconds = 3f;
		
		[SerializeField] private List<CServerLauncherInstance> _entries;
		public IReadOnlyList<CServerLauncherInstance> Entries => _entries;
		
		[field: NonSerialized]
		public bool IsUpdating { get; private set; }
		
		private double _nextUpdateTime;

		public EServerLauncherClickBehaviour BehaviourOnClick
		{
			get => Enum.TryParse<EServerLauncherClickBehaviour>(EditorPrefs.GetString(OnClickBehaviourEditorPrefsKey), out var value) ? value : default;
			set => EditorPrefs.SetString(OnClickBehaviourEditorPrefsKey, value.ToString());
		}

		public EServerLauncherEnterPlayModeBehaviour BehaviourOnPlay
		{
			get => Enum.TryParse<EServerLauncherEnterPlayModeBehaviour>(EditorPrefs.GetString(OnPlayBehaviourEditorPrefsKey), out var value) ? value : default;
			set => EditorPrefs.SetString(OnPlayBehaviourEditorPrefsKey, value.ToString());
		}
		
		public void Initialize()
		{
			EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
			EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
			EditorApplication.quitting -= HandleEditorApplicationQuitting;
			EditorApplication.quitting += HandleEditorApplicationQuitting;
			if (!Application.isPlaying)
			{
				StartUpdating();
			}
		}

		public bool TryGetServerByName(string serverName, out CServerLauncherInstance result)
		{
			foreach (var server in _entries)
			{
				if (server.ServerName == serverName)
				{
					result = server;
					return true;
				}
			}

			result = null;
			return false;
		}

		public async UniTask<bool> TryLaunchAllServersAsync(CancellationTokenSource cts)
		{
			bool allSuccess = true;
			foreach (var entry in Entries)
			{
				var timeoutDuration = TimeSpan.FromSeconds(10);
				var timeoutCancellationToken = new CancellationTokenSource(timeoutDuration);
				var cancellationLink = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, timeoutCancellationToken.Token);
				
				entry.UpdateState();
				if (entry.CachedState != EServerLauncherInstanceState.Running && entry.CachedState != EServerLauncherInstanceState.Launching)
				{
					entry.BuildAndRun(true);
				}
				bool success = await entry.WaitUntilRunning(cancellationLink.Token);
				allSuccess &= success;

				if (!success && !cancellationLink.IsCancellationRequested)
				{
					string timeoutString = SAldaTimeSpan.GetTime((long) timeoutDuration.TotalSeconds, IAldaTimeSpanProvider.ProviderDefaultTwoParts);
					bool isTimeout = timeoutCancellationToken.IsCancellationRequested;
					bool userWantsToContinue = EditorUtility.DisplayDialog(
						$"{entry.ServerName} failed to start{(isTimeout ? $" within {timeoutString}" : "")}.",
						"See Console for errors.\n\nContinue to Play Mode anyway?", "Continue", "Abort");
					
					if (!userWantsToContinue)
					{
						cts.Cancel();
						break;
					}
				}
				
			}

			return allSuccess;
		}

		public void KillAllServers()
		{
			foreach (var entry in Entries)
			{
				entry.KillIfRunning();
			}
		}
		
		[UsedImplicitly]
		private static async UniTask<bool> OnEntrySceneWillLaunch(CancellationTokenSource cts)
		{
			switch (Instance.BehaviourOnPlay)
			{
				case EServerLauncherEnterPlayModeBehaviour.DoNothing:
					return true;
				case EServerLauncherEnterPlayModeBehaviour.AutoStart:
					return await Instance.TryLaunchAllServersAsync(cts);
				case EServerLauncherEnterPlayModeBehaviour.AutoStartWithReset:
					Instance.KillAllServers();
					return await Instance.TryLaunchAllServersAsync(cts);
			}
			return true;
		}

		private void HandlePlayModeStateChanged(PlayModeStateChange stateChange)
		{
			if (stateChange == PlayModeStateChange.EnteredEditMode)
			{
				StartUpdating();
				return;
			}

			if (stateChange == PlayModeStateChange.ExitingEditMode)
			{
				StopUpdating();
				return;
			}
		}
		
		private void HandleEditorApplicationQuitting() => KillAllServers();

		private void StartUpdating()
		{
			EditorApplication.update -= Update;
			EditorApplication.update += Update;
			_nextUpdateTime = 0d;
			IsUpdating = true;
		}

		private void StopUpdating()
		{
			IsUpdating = false;
			EditorApplication.update -= Update;
		}

		private void Update()
		{
			if (EditorApplication.timeSinceStartup < _nextUpdateTime)
			{
				return;
			}

			bool areAllRunning = true;
			foreach (var entry in _entries)
			{
				entry.UpdateState();
				if (entry.CachedState != EServerLauncherInstanceState.Running)
				{
					areAllRunning = false;
				}
			}

			double updateInterval = areAllRunning ? RunningUpdateIntervalSeconds : NotRunningUpdateIntervalSeconds;
			_nextUpdateTime = EditorApplication.timeSinceStartup + updateInterval;
		}
	}
}