// =========================================
// AUTHOR: Jan Krejsa
// DATE:   16.01.2026
// =========================================

using System;
using UnityEngine;

namespace Editor.ServerLauncher
{
	[Serializable]
	public class CFilter
	{
		[SerializeField] private string _filterText;
		[SerializeField] private int _filterHash;

		public int FilterHash => _filterHash;
		public string FilterText
		{
			get => _filterText;
			set
			{
				_filterText = value ?? "";
				RecalculateHash();
			}
		}
		
		private void RecalculateHash() => _filterHash = _filterText.GetHashCode();

		public SFilterResult Evaluate(string text, bool caseSensitive)
		{
			bool isMatch = string.IsNullOrWhiteSpace(_filterText) || text.IndexOf(_filterText, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase) >= 0;
			return new SFilterResult(_filterHash, isMatch);
		}
	}
}
