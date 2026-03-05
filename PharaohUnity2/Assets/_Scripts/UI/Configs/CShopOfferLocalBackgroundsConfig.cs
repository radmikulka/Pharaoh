// =========================================
// AUTHOR: Juraj Joscak
// DATE:   18.12.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using ServerData;
using UnityEngine;

namespace TycoonBuilder.Configs
{
	[CreateAssetMenu(menuName = "AldaGames/TycoonBuilder/Configs/ShopOfferLocalBackgroundsConfig", fileName = "ShopOfferLocalBackgroundsConfig")]
	public class CShopOfferLocalBackgroundsConfig : ScriptableObject, IIHaveBundleLinks
	{
		[SerializeField] private CLocalBackground[] _sprites;
		
		public CBundleLink GetSprite(EOfferLocalBackground id)
		{
			CLocalBackground bg = _sprites.FirstOrDefault(s => s._id == id);
			if (bg != null)
				return bg._sprite;
			
			throw new Exception($"CShopOfferLocalBackgroundsConfig: Sprite for id {id} not found!");
		}
		
		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			return _sprites.Select(bg => bg._sprite);
		}
	}
	
	[Serializable]
	public class CLocalBackground
	{
		[SerializeField] public EOfferLocalBackground _id;
		[SerializeField] [BundleLink(true, typeof(Sprite))] public CBundleLink _sprite;
	}
}