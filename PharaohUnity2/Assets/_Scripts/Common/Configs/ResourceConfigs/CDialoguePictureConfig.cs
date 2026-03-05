// =========================================
// AUTHOR: Marek Karaba
// DATE:   04.07.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using RoboRyanTron.SearchableEnum;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	[CreateAssetMenu(menuName = "____TycoonBuilder/Configs/Dialogues/DialoguePictureConfig", fileName = "cfg_DialoguePictureConfig_Picture00")]
	public class CDialoguePictureConfig : CScriptableResourceConfig<EDialoguePictureId>, IIHaveBundleLinks
	{
		[SerializeField] [BundleLink(false, typeof(Sprite))] private CBundleLink _sprite;
		
		public CBundleLink Sprite => _sprite;
		
		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			_sprite.SetBundleId((int)EBundleId.None);
			return new[] { _sprite };
		}
	}
}