// =========================================
// AUTHOR: Marek Karaba
// DATE:   12.01.2026
// =========================================

using System.Collections.Generic;
using AldaEngine;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	[CreateAssetMenu(menuName = "____TycoonBuilder/Configs/DispatcherConfig", fileName = "cfg_dispatcher_name")]
	public class CDispatcherConfig : CScriptableResourceConfig<EDispatcher>, IIHaveBundleLinks
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