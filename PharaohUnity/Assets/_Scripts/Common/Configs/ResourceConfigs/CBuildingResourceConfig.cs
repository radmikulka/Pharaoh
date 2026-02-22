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
		[SerializeField] private EBuildingType _buildingType;
		[SerializeField] [BundleLink(false, typeof(GameObject))] private CBundleLink _prefab;
		[SerializeField] [BundleLink(false, typeof(Sprite))] private CBundleLink _icon;
		[SerializeField] private ECellTag _requiredTags;
		[SerializeField] private string _displayName;

		public EBuildingId Id => _id;
		public EBuildingType BuildingType => _buildingType;
		public CBundleLink Prefab => _prefab;
		public CBundleLink Icon => _icon;
		public ECellTag RequiredTags => _requiredTags;
		public string DisplayName => _displayName;

		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			_prefab.SetBundleId((int) EBundleId.BaseGame);
			_icon.SetBundleId((int) EBundleId.BaseGame);

			yield return _prefab;
			yield return _icon;
		}
	}
}