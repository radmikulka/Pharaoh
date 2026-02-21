// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.09.2025
// =========================================

using System.Collections.Generic;

namespace Pharaoh
{
	public class CGoToContext
	{
		private class CGoToContextEntry<T> : IGoToEntry
		{
			public readonly T Value;

			public CGoToContextEntry(T value)
			{
				Value = value;
			}
		}

		private readonly Dictionary<EGoToContextKey, IGoToEntry> _entries = new();

		public T GetEntryOrDefault<T>(EGoToContextKey key)
		{
			if (!_entries.TryGetValue(key, out IGoToEntry entry))
				return default;

			if (entry is CGoToContextEntry<T> typedEntry)
			{
				return typedEntry.Value;
			}

			return default;
		}

		public T GetEntry<T>(EGoToContextKey key)
		{
			return ((CGoToContextEntry<T>)_entries[key]).Value;
		}

		public void SetEntry<T>(EGoToContextKey key, T value)
		{
			_entries[key] = new CGoToContextEntry<T>(value);
		}

		public bool HasEntry(EGoToContextKey key)
		{
			return _entries.ContainsKey(key);
		}
	}
}