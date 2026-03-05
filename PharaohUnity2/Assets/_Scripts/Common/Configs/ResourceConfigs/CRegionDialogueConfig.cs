// =========================================
// AUTHOR: Marek Karaba
// DATE:   04.07.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using RoboRyanTron.SearchableEnum;
using ServerData;
using TycoonBuilder.Configs.Design;
using UnityEngine;

namespace TycoonBuilder
{
	[CreateAssetMenu(menuName = "____TycoonBuilder/Configs/Dialogues/DialogueConfig", fileName = "cfg_DialogueConfig_")]
	public class CRegionDialogueConfig : CScriptableResourceConfig<ERegion>, IIHaveBundleLinks
	{
		[SerializeField] [BundleLink(false, typeof(TextAsset))] private CBundleLink _textAsset;
		public CBundleLink TextAsset => _textAsset;
		
		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			_textAsset.SetBundleId((int)EBundleId.BaseGame);
			return new[] { _textAsset };
		}
	}
}