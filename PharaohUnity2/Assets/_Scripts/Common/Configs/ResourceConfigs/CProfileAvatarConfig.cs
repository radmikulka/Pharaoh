// =========================================
// AUTHOR: Marek Karaba
// DATE:   05.08.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using RoboRyanTron.SearchableEnum;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	[CreateAssetMenu(menuName = "____TycoonBuilder/Configs/ProfileAvatar")]
	public class CProfileAvatarConfig : ScriptableObject, IResourceConfigBase<EProfileAvatar>, IIHaveBundleLinks
	{
		[SerializeField, SearchableEnum] private EProfileAvatar _id;
		[SerializeField] [BundleLink(false, typeof(Sprite))] private CBundleLink _sprite;
		
		public EProfileAvatar Id => _id;
		public CBundleLink Sprite => _sprite;
		
		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			_sprite.SetBundleId((int)EBundleId.BaseGame);
			return new[] { _sprite };
		}
	}
}