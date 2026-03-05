// =========================================
// AUTHOR: Marek Karaba
// DATE:   19.08.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	[CreateAssetMenu(menuName = "____TycoonBuilder/Configs/MovementTypeConfig", fileName = "cfg_movementType_")]
	public class CMovementTypeConfig : CScriptableResourceConfig<EMovementType>, IIHaveBundleLinks
	{
		[SerializeField] [BundleLink(false, typeof(Sprite))] private CBundleLink _sprite;
		
		public CBundleLink Sprite => _sprite;
		
		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			_sprite.SetBundleId((int)EBundleId.BaseGame);
			return new[] { _sprite };
		}
	}
}