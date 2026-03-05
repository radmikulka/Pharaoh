// =========================================
// AUTHOR: Juraj Joscak
// DATE:   31.07.2025
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using TycoonBuilder.Ui;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CStaticSections : MonoBehaviour, IInitializable
	{
		[SerializeField] private CStaticSectionData[] _sections;
		[SerializeField] private CUiPooledBlock _pooledBlock;
		[SerializeField] private CSectionHandler _sectionHandler;
		[SerializeField] private InterfaceReference<ISectionContentProvider> _sectionContentProvider;

		public void Initialize()
		{
			CSectionContentBase[] content = _sectionContentProvider.Value.GetContent();
			for (int i = 0; i < _sections.Length; i++)
			{
				CreateTab(_sections[i]._titleLangKey, content[i], _sections[i]._icon);
			}
		}
		
		private void CreateTab(string tabName, CSectionContentBase sectionContentBase, Sprite icon)
		{
			CSectionUiData data = new(tabName, sectionContentBase, _sectionHandler, icon);
			_pooledBlock.Add(data);
		}

		public RectTransform GetItemRectAtIndex(int index)
		{
			return _pooledBlock.GetItemAtIndex<CSectionUiData>(index).ActiveItem.PooledObject.GetComponent<RectTransform>();
		}

		[Serializable]
		private class CStaticSectionData
		{
			public string _titleLangKey;
			public Sprite _icon;
		}
	}
}