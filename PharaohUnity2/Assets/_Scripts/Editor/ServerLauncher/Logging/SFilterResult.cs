// =========================================
// AUTHOR: Jan Krejsa
// DATE:   16.01.2026
// =========================================

namespace Editor.ServerLauncher
{
	public struct SFilterResult
	{
		public readonly int FilterHash;
		public readonly bool IsMatch;

		public SFilterResult(int filterHash, bool isMatch)
		{
			FilterHash = filterHash;
			IsMatch = isMatch;
		}
	}
}
