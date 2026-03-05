// =========================================
// AUTHOR: Jan Krejsa
// DATE:   31.12.2025
// =========================================

using UnityEngine;

namespace Editor.ServerLauncher
{
	public abstract class CServerCommandInstance
	{
		protected CServerCommandInstance(string label)
		{
			Label = label;
		}

		public string Label { get; }
		public virtual void OnGui(Rect rect) { }
		public abstract float GetGuiHeight();
		public abstract string GetCommand();
	}
}