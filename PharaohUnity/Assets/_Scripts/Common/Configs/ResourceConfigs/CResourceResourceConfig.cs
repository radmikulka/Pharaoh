// =========================================
// AUTHOR: Marek Karaba
// DATE:   23.07.2025
// =========================================

using System;
using System.Collections.Generic;
using AldaEngine;
using AldaEngine.UnityObjectPool;
using RoboRyanTron.SearchableEnum;
using ServerData;
using UnityEngine;

namespace Pharaoh
{
	[CreateAssetMenu(menuName = "____Pharaoh/Configs/Resource")]
	public class CResourceResourceConfig : ScriptableObject, IResourceConfigBase<EResource>, IIHaveBundleLinks
	{
		[SerializeField, SearchableEnum] private EResource _id;
		[SerializeField] [BundleLink(false, typeof(Sprite))] private CBundleLink _sprite;

		public EResource Id => _id;
		public CBundleLink Sprite => _sprite;

		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			_sprite.SetBundleId((int) EBundleId.BaseGame);

			yield return _sprite;
		}
	}
}