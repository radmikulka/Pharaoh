// =========================================
// AUTHOR: Marek Karaba
// DATE:   15.07.2025
// =========================================

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CSectionHandler : ValidatedMonoBehaviour, IScreenOpenStart, IScreenCloseStart, IScreenCloseEnd, IAldaFrameworkComponent
	{
		[SerializeField, Child] private ScrollRect _scrollRect;

		private readonly List<CSectionEntry> _sectionEntries = new();
		private CSectionEntry _currentSelectedEntry;
		private CSectionEntry _lastSelectedEntry;

		private Vector2 _lastContentPosition;

		public CSectionContentBase CurrentSection => _currentSelectedEntry.Content;
		
		public void AddTab(CSectionEntry sectionEntry)
		{
			if (_sectionEntries.Contains(sectionEntry))
				return;

			_sectionEntries.Add(sectionEntry);
		}

		public void Reset()
		{
			_sectionEntries.Clear();
			_currentSelectedEntry = null;
			_lastSelectedEntry = null;
		}

		public void SwitchSection(CSectionEntry sectionEntry)
		{
			if(_currentSelectedEntry == sectionEntry)
				return;
			
			DeselectCurrentSection();

			if (!_sectionEntries.Contains(sectionEntry))
				return;

			_currentSelectedEntry = sectionEntry;
			_lastSelectedEntry = sectionEntry;
			sectionEntry.Button.Select();
			sectionEntry.Content.Show();
		}
		
		public void SwitchSection(int index)
		{
			SwitchSection(_sectionEntries[index]);
		}

		private void DeselectCurrentSection()
		{
			if (_currentSelectedEntry == null)
				return;
			
			foreach (CSectionEntry sectionEntry in _sectionEntries.Where(sectionEntry => _lastSelectedEntry == sectionEntry))
			{
				sectionEntry.Button.Deselect();
				sectionEntry.Content.Hide();
			}
			_currentSelectedEntry = null;
		}

		public void OnScreenOpenStart()
		{
			ScrollToLastContentPosition();
			StartCoroutine(SelectLastSectionOrDefault());
		}
		
		public void OnScreenCloseStart()
		{
			SaveLastContentPosition();
		}

		private void SaveLastContentPosition()
		{
			_lastContentPosition = _scrollRect.content.anchoredPosition;
		}

		public void OnScreenCloseEnd()
		{
			DeselectCurrentSection();
		}

		private IEnumerator SelectLastSectionOrDefault()
		{
			yield return null;
			
			_lastSelectedEntry ??= _sectionEntries.First();
			SwitchSection(_lastSelectedEntry);
		}

		private void ScrollToLastContentPosition()
		{
			_scrollRect.content.anchoredPosition = _lastContentPosition;
		}
	}
}