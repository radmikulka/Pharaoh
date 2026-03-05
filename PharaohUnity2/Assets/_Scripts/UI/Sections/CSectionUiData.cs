// =========================================
// AUTHOR: Marek Karaba
// DATE:   15.07.2025
// =========================================

using AldaEngine;
using UnityEngine;

namespace TycoonBuilder.Ui
{
	public class CSectionUiData : CUiPoolData
	{
		public readonly string Title;
		public readonly Sprite Icon;
		public readonly CSectionContentBase SectionContentBase;
		public readonly CSectionHandler SectionHandler;

		public CSectionUiData(string title, CSectionContentBase sectionContentBase, CSectionHandler sectionHandler, Sprite icon = null)
		{
			Title = title;
			SectionContentBase = sectionContentBase;
			SectionHandler = sectionHandler;
			Icon = icon;
		}
	}
}