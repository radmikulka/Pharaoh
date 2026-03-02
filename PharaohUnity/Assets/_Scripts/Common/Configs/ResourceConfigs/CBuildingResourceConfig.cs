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

		public EBuildingId Id => _id;
		public CBundleLink Prefab => _prefab;
		public CBundleLink Icon => _icon;

		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			_prefab.SetBundleId((int) EBundleId.BaseGame);
			_icon.SetBundleId((int) EBundleId.BaseGame);

			yield return _prefab;
			yield return _icon;
		}
	}
}
