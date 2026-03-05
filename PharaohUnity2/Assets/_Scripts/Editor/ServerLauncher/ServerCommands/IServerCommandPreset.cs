// =========================================
// AUTHOR: Jan Krejsa
// DATE:   31.12.2025
// =========================================

namespace Editor.ServerLauncher
{
	public interface IServerCommandPreset
	{
		string Label { get; }
		string Tooltip { get; }
		CServerCommandInstance CreateInstance();
	}
}
