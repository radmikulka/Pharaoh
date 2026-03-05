// =========================================
// AUTHOR: Jan Krejsa
// DATE:   24.12.2025
// =========================================

using UnityEditor.IMGUI.Controls;

namespace Editor.ServerLauncher
{
	public class CServerLauncherLogTreeViewItem : TreeViewItem<int>
	{
		public CServerLauncherLogEntry Entry { get; }
		private SFilterResult _cachedFilterResult;

		public CServerLauncherLogTreeViewItem(int id, CServerLauncherLogEntry entry) : base(id)
		{
			Entry = entry;
			this.displayName = entry.Message;
		}

		public bool MatchesFilter(CFilter filter)
		{
			if (_cachedFilterResult.FilterHash == filter.FilterHash)
				return _cachedFilterResult.IsMatch;

			_cachedFilterResult = filter.Evaluate(Entry.Message, false);
			return _cachedFilterResult.IsMatch;
		}
	}
}

