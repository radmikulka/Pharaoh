// =========================================
// AUTHOR: Jan Krejsa
// DATE:   24.12.2025
// =========================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Editor.ServerLauncher;
using UnityEditor.IMGUI.Controls;
using UnityEngine.UIElements;

namespace Editor.ServerLauncher
{
	public class CServerLauncherLogWindow : EditorWindow
	{
		private const string PrefWidthKey = "CServerLauncherLogWindow_Width";
		private const string PrefHeightKey = "CServerLauncherLogWindow_Height";
		private const string PrefSplitterKey = "CServerLauncherLogWindow_Splitter";
        private const float MinTopPixels = 60f;
        private const float MinBottomPixels = 60f;
        private const string CommandInputControlName = "ServerLauncher_CommandInput";
        
		public static CServerLauncherLogWindow CurrentWindowInstance { get; private set; }
		public CServerLauncherInstance LinkedServerInstance { get; private set; }
		public VisualElement ActivatorElement { get; private set; }

		private Vector2 _listScrollPos;
		private Vector2 _messageDetailScrollPos;
		
		private float SplitRatio { get; set; } = 0.66f; // normalized ratio (0..1) of how much space the top list occupies relative to the available fullRect height
		private bool _isDraggingSplitter;
		
		private GUIStyle _serverCommandButtonStyle;
		private GUIStyle _messageDetailStyle;
		
		private readonly CFilter _searchFilter = new();
		private string _inputCommand = string.Empty;
		
		private readonly Dictionary<EServerLauncherLogLevelId, bool> _levelFilters = new();
		private readonly List<CServerLauncherLogTreeViewItem> _logEntryItems = new();
		private readonly List<CServerLauncherLogTreeViewItem> _filteredLogEntries = new();
		private CServerLauncherLogTreeViewItem _selectedItem;
		private CServerLauncherLogTreeView _treeView;
		private bool _isAutoScrollEnabled = true;
		private TreeViewState<int> _treeState;

		// When true, the TreeView will not be updated (UI pause) but incoming log entries are still collected.
		private bool _isPaused;

		public static void OpenForInstance(CServerLauncherInstance instance, VisualElement activatorElement)
		{
			if (instance == null)
			{
				return;
			}

			if (CurrentWindowInstance != null && CurrentWindowInstance.LinkedServerInstance == instance)
			{
				CurrentWindowInstance.Focus();
				return;
			}

			if (CurrentWindowInstance != null)
			{
				CurrentWindowInstance.Close();
			}

			var win = CreateInstance<CServerLauncherLogWindow>();
			win.minSize = new Vector2(720f, 150f);
			win.titleContent = new GUIContent(instance.ServerName, EditorGUIUtility.FindTexture("CloudConnect"));
			win.LinkedServerInstance = instance;
			win.ActivatorElement = activatorElement;

			Rect pos = win.position;
			pos.width = Mathf.Max(win.minSize.x, EditorPrefs.GetFloat(PrefWidthKey, 800f));
			pos.height = Mathf.Max(win.minSize.y, EditorPrefs.GetFloat(PrefHeightKey, 400f));
			win.SplitRatio = EditorPrefs.GetFloat(PrefSplitterKey, win.SplitRatio);
			
			foreach (EServerLauncherLogLevelId level in Enum.GetValues(typeof(EServerLauncherLogLevelId)))
			{
				if (!win._levelFilters.ContainsKey(level)) 
					win._levelFilters[level] = level != EServerLauncherLogLevelId.BuildInformation || !win.LinkedServerInstance.Logger.HasBuildFinished; // default: all enabled except BuildInfo
			}
			
			instance.Logger.EventBuildFinishedChanged += win.OnBuildFinishedChanged;
			instance.Logger.EventLogEntryReceived += win.RegisterLogEntry;

			win.RebuildLogEntries(instance.Logger.LogEntries);
			
			win.ShowPopup();
			win.position = pos;
			CurrentWindowInstance = win;
			win.UpdatePopupPosition();
		}

		private void UpdatePopupPosition()
		{
			if (ActivatorElement == null)
				return;
			
			Rect activatorRect = ActivatorElement.worldBound;
			Vector2 anchorPos = GUIUtility.GUIToScreenPoint(new Vector2(activatorRect.x, activatorRect.y + activatorRect.height));
			
			Rect pos = position;
			pos.x = anchorPos.x;
			pos.y = anchorPos.y;
			position = pos;
		}

		private void OnEnable()
		{
			EditorApplication.update += EditorUpdate;
		}

		private void OnDisable()
		{
			EditorApplication.update -= EditorUpdate;
			if (LinkedServerInstance?.Logger != null)
			{
				LinkedServerInstance.Logger.EventLogEntryReceived -= RegisterLogEntry;
				LinkedServerInstance.Logger.EventBuildFinishedChanged -= OnBuildFinishedChanged;
			}

			if (CurrentWindowInstance == this) 
				CurrentWindowInstance = null;

			EditorPrefs.SetFloat(PrefWidthKey, position.width);
			EditorPrefs.SetFloat(PrefHeightKey, position.height);
			EditorPrefs.SetFloat(PrefSplitterKey, SplitRatio);
		}

		private void EditorUpdate() => Repaint();

		private void RegisterLogEntry(CServerLauncherLogEntry entry)
		{
			int id = _logEntryItems.Count + 1;
			_logEntryItems.Add(new CServerLauncherLogTreeViewItem(id, entry));
		}
		
		private void RebuildLogEntries(IEnumerable<CServerLauncherLogEntry> entries)
		{
			_logEntryItems.Clear();
			foreach (var entry in entries)
			{
				RegisterLogEntry(entry);
			}
		}
		
		private void OnGUI()
		{
			if (ActivatorElement == null)
			{
				Close();
				return;
			}
			
			_serverCommandButtonStyle ??= new GUIStyle("wordwrapminibutton");
			_messageDetailStyle ??= new GUIStyle(EditorStyles.wordWrappedLabel)
			{
				richText = true,
				wordWrap = true
			};
			
			Rect frameRect = new(0, 0, position.width, position.height);
			GUI.Box(frameRect, GUIContent.none, EditorStyles.helpBox);
			
			bool isReadyToSendCommands = LinkedServerInstance.CachedState == EServerLauncherInstanceState.Running
			                  && LinkedServerInstance.BuildProcess != null
			                  && LinkedServerInstance.Logger.AttachedProcess != null
			                  && !LinkedServerInstance.Logger.AttachedProcess.HasExited
			                  && LinkedServerInstance.Logger.HasBuildFinished;

			DrawTopToolbar();

			float toolbarHeight = EditorStyles.toolbar.fixedHeight;
			float inputHeight = 32f;
			float spacing = 2f;
			float bottomPadding = 4f;
			Rect fullRect = new Rect(
				0,
				toolbarHeight,
				position.width,
				position.height - toolbarHeight - (inputHeight + 2 * spacing) - bottomPadding);

			// compute split pixels from normalized ratio and clamp to sensible min/max based on available fullRect height
			float availableHeight = fullRect.height;
			float minRatio = Mathf.Clamp01(MinTopPixels / Mathf.Max(1f, availableHeight));
			float maxRatio = Mathf.Clamp01(1f - (MinBottomPixels / Mathf.Max(1f, availableHeight)));
			SplitRatio = Mathf.Clamp(SplitRatio, minRatio, maxRatio);
			float splitPixels = Mathf.Clamp(SplitRatio * availableHeight, MinTopPixels, availableHeight - MinBottomPixels);
			
			// list area (top) - use TreeView
			Rect listRect = new Rect(0, fullRect.y, fullRect.width, splitPixels);
			Rect splitter = new Rect(0, listRect.yMax, position.width, 4);

			// message area (bottom) - extend down to the input area
			float inputTop = position.height - 36f; // top Y of the input area (local window coordinates)
			float messageDetailTop = splitter.yMax + 2f;
			Rect messageDetailRect = new Rect(
				4, 
				messageDetailTop, 
				fullRect.width - 8, 
				Mathf.Max(0f, inputTop - messageDetailTop - 4f));
			Rect inputRect = new Rect(0, position.height - spacing - inputHeight, position.width, inputHeight);

			if (LinkedServerInstance == null)
			{
				DisplayNotificationBar(new Color(1f, 0f, 0f, 1f), CServerLauncherLogLevel.Error.IconActive, "No instance linked.", "Close", Close);
			}

			if (LinkedServerInstance != null && (LinkedServerInstance.Logger.AttachedProcess == null || LinkedServerInstance.BuildProcess == null))
			{
				DisplayNotificationBar(new Color(1f, 0f, 0f, 1f), CServerLauncherLogLevel.Error.IconActive, "Process not attached.", "Re-Run Server", () => LinkedServerInstance.BuildAndRun(false));
			}

			if (!string.IsNullOrEmpty(_searchFilter.FilterText))
			{
				DisplayNotificationBar(new Color(1f, 1f, 0f, 1f), EditorGUIUtility.FindTexture("d__Help@2x"), "Filter active.");
			}
			
			if (_isPaused)
			{
				DisplayNotificationBar(new Color(1f, 1f, 0f, 1f), EditorGUIUtility.FindTexture("d__Help@2x"), "Paused - logs not refreshing.", "Unpause", () => _isPaused = false);
			}
			
			DrawTreeListArea(listRect);
			HandleSplitter(splitter, fullRect);
			
			if (LinkedServerInstance?.SupportedServerCommands != null && LinkedServerInstance.SupportedServerCommands.Length > 0)
			{
				const float commandsBarHeight = 16f;
				Rect commandsBarRect = new(0, inputRect.y - commandsBarHeight, position.width, commandsBarHeight);
				messageDetailRect.yMax -= commandsBarHeight;
				
				GUI.Box(commandsBarRect, GUIContent.none, EditorStyles.toolbar);
				EditorGUI.BeginDisabledGroup(!isReadyToSendCommands);
				foreach (var cmd in LinkedServerInstance.SupportedServerCommands)
				{
					Rect buttonRect = new(commandsBarRect.xMin + 4f, commandsBarRect.yMin, 90f, commandsBarRect.height);
					GUIContent label = new(cmd.Label, cmd.Tooltip);
					if (GUI.Button(buttonRect, label, _serverCommandButtonStyle))
					{
						var commandInstance = cmd.CreateInstance();
						var commandPopupContent = new CServerLauncherCommandPopUp(LinkedServerInstance, commandInstance);
						UnityEditor.PopupWindow.Show(buttonRect, commandPopupContent);
					}
					commandsBarRect.xMin += buttonRect.width + 4f;
				}
				EditorGUI.EndDisabledGroup();
			}
			
			DrawMessageDetailArea(messageDetailRect);

			DrawInputCommandArea(inputRect, isReadyToSendCommands);
			
			void DisplayNotificationBar(Color backgroundColor, Texture2D icon, string message, string buttonLabel = null, Action buttonAction = null)
			{
				Rect notifRect = new Rect(0, listRect.y, listRect.width, 24f);
				listRect.yMin += notifRect.height;
				
				var prevColor = GUI.color;
				GUI.color = backgroundColor;
				GUI.Box(notifRect, GUIContent.none);
				GUI.color = prevColor;
				
				var contentRect = new Rect(4f, notifRect.y + 2f, notifRect.width - 8f, notifRect.height - 4f);
				GUILayout.BeginArea(contentRect);
				GUILayout.BeginVertical();
				GUILayout.BeginHorizontal();

				var guiContent = new GUIContent(message, icon);
				GUILayout.Label(guiContent, GUILayout.Height(20f));
				GUILayout.FlexibleSpace();
				
				if (buttonLabel != null && GUILayout.Button(buttonLabel, GUILayout.Width(120)))
				{
					buttonAction?.Invoke();
				}
				
				GUILayout.EndHorizontal();
				GUILayout.EndVertical();
				GUILayout.EndArea();
			}
		}

		private void DrawTopToolbar()
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			GUILayout.Space(4f);
			if (GUILayout.Button("Clear", EditorStyles.toolbarDropDown, GUILayout.Width(50)))
			{
				var menu = new GenericMenu();
				
				AddMenuItem("Clear All", _logEntryItems.Count > 0, ClearLogEntries);
				AddMenuItem("Clear Old", _logEntryItems.Count(item => item.Entry.LinkedBuildProcessId != GetCurrentBuildProcessId()) > 0, ClearOldLogEntries);

				var dropdownRect = GUILayoutUtility.GetLastRect();
				dropdownRect.y += EditorStyles.toolbarDropDown.fixedHeight;
				menu.DropDown(dropdownRect);

				void AddMenuItem(string label, bool enabled, GenericMenu.MenuFunction function)
				{
					var labelContent = new GUIContent(label);
					if (enabled)
					{
						menu.AddItem(labelContent, false, function);
					}
					else
					{
						menu.AddDisabledItem(labelContent);
					}
				}
			}
			
			bool newPausedState = GUILayout.Toggle(_isPaused, new GUIContent("Pause", "Pause log refreshing"), EditorStyles.toolbarButton, GUILayout.Width(45));
			if (newPausedState != _isPaused)
			{
				_isPaused = newPausedState;
				if (!_isPaused)
				{
					if (_treeView != null)
					{
						_treeView.UpdateEntries(_filteredLogEntries);
					}
					_isAutoScrollEnabled = true;
				}
			}
			
			GUILayout.FlexibleSpace();

			GUI.SetNextControlName("ServerLauncher_FilterInput");
			_searchFilter.FilterText = GUILayout.TextField(_searchFilter.FilterText, EditorStyles.toolbarSearchField, GUILayout.Width(200));

			EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(_searchFilter.FilterText));
			var clearButtonLabel = new GUIContent("✕", "Clear filter");
			if (GUILayout.Button(clearButtonLabel, EditorStyles.toolbarButton, GUILayout.Width(16), GUILayout.Height(16)))
			{
				_searchFilter.FilterText = string.Empty;
				GUI.FocusControl(null);
			}
			EditorGUI.EndDisabledGroup();

			GUILayout.Space(4);

			// filter buttons with counts
			foreach (CServerLauncherLogLevel logLevel in CServerLauncherLogLevel.AllLogLevels)
			{
				int count = LinkedServerInstance.Logger.GetLogCountByLevel(logLevel);
				Texture2D icon = logLevel.GetIcon(count > 0);
				GUIContent label = new($"{(count > 999 ? "999+" : count.ToString())}", icon,logLevel.Name);
				bool current = _levelFilters.TryGetValue(logLevel.Id, out bool isEnabled) && isEnabled;
				bool toggled = GUILayout.Toggle(current, label, EditorStyles.toolbarButton, GUILayout.Width(45));
				_levelFilters[logLevel.Id] = toggled;
			}

			GUILayout.Space(4);

			if (GUILayout.Button("Close", EditorStyles.toolbarButton, GUILayout.Width(60)))
			{
				Close();
			}

			EditorGUILayout.EndHorizontal();
		}

		private void ClearLogEntries()
		{
			_logEntryItems.Clear();
			_filteredLogEntries.Clear();
			LinkedServerInstance.Logger.ClearLogs();
			_selectedItem = null;
		}
		
		private void ClearOldLogEntries()
		{
			if (LinkedServerInstance == null) 
				return;
			
			int keepId = GetCurrentBuildProcessId();
			LinkedServerInstance.Logger.ClearOldLogs(keepId);
			RebuildLogEntries(LinkedServerInstance.Logger.LogEntries);
			_filteredLogEntries.Clear();
			_selectedItem = null;
		}

		private int GetCurrentBuildProcessId() => LinkedServerInstance.BuildProcess?.Id ?? -1;

		private void DrawTreeListArea(Rect rect)
		{
			if (!_isPaused)
			{
				_filteredLogEntries.Clear();
				foreach (var item in _logEntryItems)
				{
					if (_levelFilters[item.Entry.LogLevel] && item.MatchesFilter(_searchFilter))
					{
						_filteredLogEntries.Add(item);
					}
				}
			}

			if (_filteredLogEntries.Count < 1)
			{
				if (LinkedServerInstance.Logger.LogEntries.Count < 1)
				{
					EditorGUI.LabelField(rect, "No log entries.", EditorStyles.centeredGreyMiniLabel);
					return;
				}

				if (!string.IsNullOrEmpty(_searchFilter.FilterText))
				{
					EditorGUI.LabelField(rect, $"No log entries for filter '{_searchFilter.FilterText}'.", EditorStyles.centeredGreyMiniLabel);
					return;
				}
				
				EditorGUI.LabelField(rect, "No log entries to display with the current filters.", EditorStyles.centeredGreyMiniLabel);
				return;
			}

			if (_treeState == null) _treeState = new TreeViewState<int>();

			if (_treeView == null)
			{
				_treeView = new CServerLauncherLogTreeView(_treeState, _filteredLogEntries, LinkedServerInstance);
			}
			else
			{
				if (!_isPaused)
				{
					_treeView.UpdateEntries(_filteredLogEntries);
				}
			}
			
			float bottomScrollPos = Mathf.Max(_treeView.totalHeight - rect.height, 0f);

			if (_isAutoScrollEnabled)
			{
				_treeView.ScrollPos = new Vector2(0f, bottomScrollPos);
			}

			// draw tree in the provided rect
			_treeView.OnGUI(new Rect(rect.x, rect.y, rect.width, rect.height));

			// selection -> selected entry
			int selectedIndex = _treeView.GetFirstSelectedEntryIndex();
			if (selectedIndex >= 0 && selectedIndex < _logEntryItems.Count)
			{
				_selectedItem = _logEntryItems[selectedIndex];
			}
			else
			{
				_selectedItem = null;
				_messageDetailScrollPos = Vector2.zero;
			}

			bool isScrolledToBottom = _treeView.ScrollPos.y >= bottomScrollPos;
			_isAutoScrollEnabled = isScrolledToBottom;
		}

		private void HandleSplitter(Rect splitter, Rect fullRect)
		{
			EditorGUIUtility.AddCursorRect(splitter, MouseCursor.ResizeVertical);
			if (Event.current.type == EventType.MouseDown && splitter.Contains(Event.current.mousePosition))
			{
				_isDraggingSplitter = true;
				Event.current.Use();
			}
			if (_isDraggingSplitter)
			{
				if (Event.current.type == EventType.MouseDrag)
				{
					// compute new ratio relative to the fullRect area
					float yRel = Event.current.mousePosition.y - fullRect.y;
					float newRatio = yRel / Mathf.Max(1f, fullRect.height);
					// clamp ratio to min/max based on pixel constraints
					float minRatioLocal = Mathf.Clamp01(MinTopPixels / Mathf.Max(1f, fullRect.height));
					float maxRatioLocal = Mathf.Clamp01(1f - (MinBottomPixels / Mathf.Max(1f, fullRect.height)));
					SplitRatio = Mathf.Clamp(newRatio, minRatioLocal, maxRatioLocal);
					Repaint();
				}
				if (Event.current.type == EventType.MouseUp)
				{
					_isDraggingSplitter = false;
					// persist normalized splitter ratio immediately when user finishes dragging
					EditorPrefs.SetFloat(PrefSplitterKey, SplitRatio);
				}
			}
			// draw visible separator
			GUI.Box(splitter, GUIContent.none);
		}
		
		private void DrawMessageDetailArea(Rect rect)
		{
			GUI.BeginGroup(rect);
			GUILayout.BeginArea(new Rect(0, 0, rect.width, rect.height));

			if (_selectedItem != null)
			{
				_messageDetailScrollPos = EditorGUILayout.BeginScrollView(_messageDetailScrollPos);

				float requiredHeight = _messageDetailStyle.CalcHeight(new GUIContent(_selectedItem.Entry.Message), rect.width - 20); // subtract for padding/scrollbar
				EditorGUILayout.SelectableLabel(_selectedItem.Entry.Message, _messageDetailStyle, GUILayout.Height(requiredHeight));
				EditorGUILayout.EndScrollView();
			}
			else
			{
				EditorGUILayout.LabelField("Select a log entry to view details.", EditorStyles.centeredGreyMiniLabel);
			}

			GUILayout.EndArea();
			GUI.EndGroup();
		}
		
		private void DrawInputCommandArea(Rect rect, bool isReadyToSendCommands)
		{
			bool submitRequested = false;
			if (isReadyToSendCommands && HasPressedEnter() && GUI.GetNameOfFocusedControl() == CommandInputControlName)
			{
				submitRequested = true;
				Event.current.Use();
			}
            
            if (!isReadyToSendCommands)
			{
				EditorGUI.BeginDisabledGroup(true);
			}
			
			GUILayout.BeginArea(rect);
			GUILayout.BeginHorizontal(EditorStyles.helpBox);
			GUI.SetNextControlName(CommandInputControlName);
			_inputCommand = EditorGUILayout.TextField(_inputCommand, GUILayout.Height(rect.height - 4f));
			if (GUILayout.Button("Send", GUILayout.Width(80), GUILayout.Height(rect.height - 4f)))
			{
				submitRequested = true;
			}
			GUILayout.EndHorizontal();
			GUILayout.EndArea();

			if (submitRequested)
			{
				SendInput();
				_isAutoScrollEnabled = true; // jump to bottom
			}
            
            if (!isReadyToSendCommands)
			{
				EditorGUI.EndDisabledGroup();
			}

			bool HasPressedEnter()
			{
				Event evt = Event.current;
				return evt.type == EventType.KeyDown &&
				       (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter);
			}
		}

		private void SendInput()
		{
			if (string.IsNullOrWhiteSpace(_inputCommand) || LinkedServerInstance == null) return;
			try
			{
				LinkedServerInstance.SendInputToBuildProcess(_inputCommand);
				_inputCommand = string.Empty;
				_isAutoScrollEnabled = true; // jump to bottom
			}
			catch (Exception ex)
			{
				Debug.LogError($"Failed to send input: {ex.Message}");
			}
		}
		

		private void OnBuildFinishedChanged() => _levelFilters[EServerLauncherLogLevelId.BuildInformation] = !LinkedServerInstance.Logger.HasBuildFinished;
	}
}
