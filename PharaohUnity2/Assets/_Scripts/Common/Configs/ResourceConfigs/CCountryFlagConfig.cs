// =========================================
// AUTHOR: Marek Karaba
// DATE:   07.08.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using RoboRyanTron.SearchableEnum;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	[CreateAssetMenu(menuName = "____TycoonBuilder/Configs/CountryFlagConfig", fileName = "cfg_countryFlag_Code")]
	public class CCountryFlagConfig : CScriptableResourceConfig<ECountryCode>, IIHaveBundleLinks
	{
		[SerializeField] [BundleLink(false, typeof(Sprite))] private CBundleLink _sprite;
		
		public CBundleLink Sprite => _sprite;
		
		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			_sprite.SetBundleId((int)EBundleId.BaseGame);
			yield return _sprite;
		}
	}
}