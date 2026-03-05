// =========================================
// AUTHOR: Juraj Joscak
// DATE:   07.07.2025
// =========================================

using System;
using AldaEngine.AldaFramework;
using UnityEngine;

namespace TycoonBuilder.Ui
{
	public class CTabs : MonoBehaviour, IInitializable
	{
		[SerializeField] private STabWithButton[] _tabs;

		public int SelectedTabIndex { get; private set; } = -1;

		public void Initialize()
		{
			for(int i = 0; i < _tabs.Length; i++)
			{
				_tabs[i].Button.SetIndex(i);
			}
			
			SwitchTab(-1);
		}

		public void Enable()
		{
			SwitchTab(SelectedTabIndex);
		}

		public void Disable()
		{
			int lastSelectedTabIndex = SelectedTabIndex;
			SwitchTab(-1);
			SelectedTabIndex = lastSelectedTabIndex;
		}
		
		public void SwitchTab(int index)
		{
			for (int i = 0; i < _tabs.Length; i++)
			{
				if (i == index)
				{
					_tabs[i].Button.Select();
					_tabs[i].Content.Show();
				}
				else
				{
					_tabs[i].Button.Deselect();
					_tabs[i].Content.Hide();
				}
			}
			SelectedTabIndex = index;
		}
		
		[Serializable]
		private struct STabWithButton
		{
			[SerializeField] private CTabButton _button;
			[SerializeField] private CTabContentBase _content;
			
			public CTabButton Button => _button;
			public CTabContentBase Content => _content;
		}
	}
}