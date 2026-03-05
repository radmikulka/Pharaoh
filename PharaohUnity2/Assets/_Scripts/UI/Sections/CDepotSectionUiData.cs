// =========================================
// AUTHOR: Marek Karaba
// DATE:   12.08.2025
// =========================================

using AldaEngine;
using ServerData;
using UnityEngine;

namespace TycoonBuilder.Ui
{
	public class CDepotSectionUiData : CSectionUiData
	{
		public readonly EDecadeMilestone DecadeMilestone;
		public readonly bool Locked;

		public CDepotSectionUiData(
			EDecadeMilestone decadeMilestone,
			string title,
			CSectionContentBase sectionContentBase,
			CSectionHandler sectionHandler, bool locked, Sprite icon = null)
			: base(title, sectionContentBase, sectionHandler, icon)
		{
			DecadeMilestone = decadeMilestone;
			Locked = locked;
		}
	}
}