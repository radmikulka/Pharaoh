// =========================================
// AUTHOR: Jan Krejsa
// DATE:   22.10.2025
// =========================================

using Editor.ServerLauncher;
using JetBrains.Annotations;
using Paps.UnityToolbarExtenderUIToolkit;
using UnityEngine.UIElements;

namespace Editor.EditorToolbarExtensions
{
	[MainToolbarElement("ServerLauncher", ToolbarAlign.Right), UsedImplicitly]
	public class CServerLauncherToolbarExtension: VisualElement
	{
		[UsedImplicitly]
		public void InitializeElement()
		{
			style.flexDirection = FlexDirection.Row;

			var entries = CServerLauncherConfig.Instance.Entries;
			if (entries == null)
			{
				return;
			}
			
			foreach (var entry in entries)
			{
				var entryDisplay = new CServerLauncherEntryDisplay(entry);
				Add(entryDisplay);
			}
			
			CServerLauncherConfig.Instance.Initialize();
		}
	}
}
