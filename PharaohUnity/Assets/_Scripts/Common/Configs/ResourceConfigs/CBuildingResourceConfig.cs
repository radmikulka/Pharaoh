using System.Collections.Generic;
using AldaEngine;
using AldaEngine.UnityObjectPool;
using RoboRyanTron.SearchableEnum;
using ServerData;
using UnityEngine;

namespace Pharaoh
{
	[CreateAssetMenu(menuName = "____Pharaoh/Configs/Building")]
	public class CBuildingResourceConfig : ScriptableObject, IResourceConfigBase<EBuildingId>, IIHaveBundleLinks
	{
		[SerializeField, SearchableEnum] private EBuildingId _id;
		[SerializeField] [BundleLink(false, typeof(GameObject))] private CBundleLink _prefab;
		[SerializeField] [BundleLink(false, typeof(Sprite))] private CBundleLink _icon;

		[Header("Road Variants (only used for Road building)")]
		[SerializeField] [BundleLink(false, typeof(GameObject))] private CBundleLink _roadVariantDeadEnd;
		[SerializeField] [BundleLink(false, typeof(GameObject))] private CBundleLink _roadVariantCorner;
		[SerializeField] [BundleLink(false, typeof(GameObject))] private CBundleLink _roadVariantStraight;
		[SerializeField] [BundleLink(false, typeof(GameObject))] private CBundleLink _roadVariantTJunction;
		[SerializeField] [BundleLink(false, typeof(GameObject))] private CBundleLink _roadVariantCross;

		public EBuildingId Id => _id;
		public CBundleLink Prefab => _prefab;
		public CBundleLink Icon => _icon;
		public CBundleLink RoadVariantDeadEnd   => _roadVariantDeadEnd;
		public CBundleLink RoadVariantCorner    => _roadVariantCorner;
		public CBundleLink RoadVariantStraight  => _roadVariantStraight;
		public CBundleLink RoadVariantTJunction => _roadVariantTJunction;
		public CBundleLink RoadVariantCross     => _roadVariantCross;

		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			_prefab.SetBundleId((int) EBundleId.BaseGame);
			_icon.SetBundleId((int) EBundleId.BaseGame);

			yield return _prefab;
			yield return _icon;

			_roadVariantDeadEnd.SetBundleId((int) EBundleId.BaseGame);
			yield return _roadVariantDeadEnd;
			
			_roadVariantCorner.SetBundleId((int) EBundleId.BaseGame);
			yield return _roadVariantCorner;
			
			_roadVariantStraight.SetBundleId((int) EBundleId.BaseGame);
			yield return _roadVariantStraight;
			
			_roadVariantTJunction.SetBundleId((int) EBundleId.BaseGame);
			yield return _roadVariantTJunction;
			
			_roadVariantCross.SetBundleId((int) EBundleId.BaseGame);
			yield return _roadVariantCross;
		}
	}
}
