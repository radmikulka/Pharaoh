// =========================================
// AUTHOR: Marek Karaba
// DATE:   06.11.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;

namespace TycoonBuilder
{
	public class CDialogueAnalytics : IInitializable
	{
		private readonly IAnalytics _analytics;
		private readonly IEventBus _eventBus;
		
		private readonly Dictionary<string, object> _cachedParams = new();

		public CDialogueAnalytics(IAnalytics analytics, IEventBus eventBus)
		{
			_analytics = analytics;
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CDialogueStartedSignal>(OnDialogueStarted);
			_eventBus.Subscribe<CDialogueFinishedSignal>(OnDialogueFinished);
			_eventBus.Subscribe<CDialogueSkippedSignal>(OnDialogueSkipped);
		}

		private void OnDialogueStarted(CDialogueStartedSignal signal)
		{
			_cachedParams.Clear();
			string dialogueId = signal.DialogueId.ToString();
			int index = (int)signal.DialogueId;
			int totalNodes = signal.CurrentDialogueCommandsCount;
			
			_cachedParams.Add("Name", dialogueId);
			_cachedParams.Add("Index", index);
			_cachedParams.Add("TotalNodes", totalNodes);
			_analytics.SendData("DialogueStart", _cachedParams);
		}

		private void OnDialogueFinished(CDialogueFinishedSignal signal)
		{
			_cachedParams.Clear();
			string dialogueId = signal.DialogueId.ToString();
			int index = (int)signal.DialogueId;
			int totalNodes = signal.CurrentDialogueCommandsCount;
			
			_cachedParams.Add("Name", dialogueId);
			_cachedParams.Add("Index", index);
			_cachedParams.Add("TotalNodes", totalNodes);
			_analytics.SendData("DialogueFinish", _cachedParams);
		}

		private void OnDialogueSkipped(CDialogueSkippedSignal signal)
		{
			_cachedParams.Clear();
			string dialogueId = signal.DialogueId.ToString();
			int index = (int)signal.DialogueId;
			int skipPointNode = signal.CurrentDialogueCommandsIndex;
			int totalNodes = signal.CurrentDialogueCommandsCount;
			
			_cachedParams.Add("Name", dialogueId);
			_cachedParams.Add("Index", index);
			_cachedParams.Add("TotalNodes", totalNodes);
			_cachedParams.Add("SkipPointNode", skipPointNode);
			_analytics.SendData("DialogueSkip", _cachedParams);
		}
	}
}