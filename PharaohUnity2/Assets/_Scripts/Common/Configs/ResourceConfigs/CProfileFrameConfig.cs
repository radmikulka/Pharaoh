// =========================================
// AUTHOR: Marek Karaba
// DATE:   04.08.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using RoboRyanTron.SearchableEnum;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	[CreateAssetMenu(menuName = "____TycoonBuilder/Configs/ProfileFrame")]
	public class CProfileFrameConfig : ScriptableObject, IResourceConfigBase<EProfileFrame>, IIHaveBundleLinks
	{
		[SerializeField, SearchableEnum] private EProfileFrame _id;
		[SerializeField] [BundleLink(false, typeof(Sprite))] private CBundleLink _sprite;
		
		public EProfileFrame Id => _id;
		public CBundleLink Sprite => _sprite;
		
		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			_sprite.SetBundleId((int)EBundleId.BaseGame);
			return new[] { _sprite };
		}
	}
}