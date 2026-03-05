// =========================================
// AUTHOR: Jan Krejsa
// DATE:   24.12.2025
// =========================================

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Editor.ServerLauncher
{
	public class CServerLauncherLogTreeView : TreeView<int>
	{
		private readonly CServerLauncherInstance _currentServerInstance;
		private List<CServerLauncherLogTreeViewItem> _entries;
		private GUIStyle _consoleStyleBackingField;

		public Vector2 ScrollPos
		{
			get => state.scrollPos;
			set => state.scrollPos = value;
		}

		public CServerLauncherLogTreeView(TreeViewState<int> state, List<CServerLauncherLogTreeViewItem> entries, CServerLauncherInstance serverInstance) : base(state)
		{
			rowHeight = 38f;
			_entries = entries ?? new List<CServerLauncherLogTreeViewItem>();
			_currentServerInstance = serverInstance;
			showAlternatingRowBackgrounds = true;
			showBorder = true;
			Reload();
		}

		public void UpdateEntries(List<CServerLauncherLogTreeViewItem> entries)
		{
			_entries = entries ?? new List<CServerLauncherLogTreeViewItem>();
			Reload();
		}

		public int GetFirstSelectedEntryIndex()
		{
			foreach (int id in GetSelection())
			{
				return id - 1;
			}

			return -1;
		}

		protected override TreeViewItem<int> BuildRoot()
		{
			var root = new TreeViewItem<int> { id = 0, depth = -1 };
			var items = new List<TreeViewItem<int>>(_entries.Count);
			for (int i = 0; i < _entries.Count; i++)
			{
				items.Add(_entries[i]);
			}
			SetupParentsAndChildrenFromDepths(root, items);
			return root;
		}

		protected override void RowGUI(RowGUIArgs args)
		{
			var item = (CServerLauncherLogTreeViewItem)args.item;
			DrawEntryRow(args.rowRect, item);
		}

		private void DrawEntryRow(Rect r, CServerLauncherLogTreeViewItem item)
		{
			CServerLauncherLogEntry entry = item.Entry;
			GUI.enabled = entry.LinkedBuildProcessId == _currentServerInstance.BuildProcess?.Id;
			
			const float leftPadding = 8f;
			const float verticalPadding = 4f;
			float iconSize = r.height - 2 * verticalPadding;
			var iconRect = new Rect(r.x + leftPadding, r.y + verticalPadding, iconSize, iconSize);

			var icon = entry.LogLevel.GetIcon(true);
			var iconTooltip = new GUIContent(string.Empty, entry.LogLevel.GetLogLevel().Name);
			GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
			GUI.Label(iconRect, iconTooltip);
			
			Rect messageRect = r;
			messageRect.xMin += leftPadding + iconSize;
			
			string trimmedMessage = entry.Message;
			int firstNl = trimmedMessage.IndexOf('\n');
			if (firstNl != -1)
			{
				int secondNl = trimmedMessage.IndexOf('\n', firstNl + 1);
				if (secondNl != -1)
				{
					trimmedMessage = trimmedMessage.Substring(0, secondNl);
				}
			}
			
			string label = $"[{entry.LogTime:HH:mm:ss}] {trimmedMessage}";
			
			GUIStyle consoleStyle = _consoleStyleBackingField ??= new(EditorStyles.wordWrappedLabel)
			{
				richText = true,
				wordWrap = true
			};

			GUI.Label(messageRect, label, consoleStyle);
			GUI.enabled = true;
		}
	}
}
