// =========================================
// AUTHOR: Juraj Joscak
// DATE:   16.02.2026
// =========================================

using System.Collections.Generic;
using AldaEngine;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	[CreateAssetMenu(menuName = "____TycoonBuilder/Configs/Task")]
	public class CTaskResourceConfig : ScriptableObject, IResourceConfigBase<ETaskId>, IIHaveBundleLinks
	{
		[SerializeField] private ETaskId _id;
		[SerializeField, BundleLink(false, typeof(Sprite))]  private CBundleLink _icon;

		public ETaskId Id => _id;
		public CBundleLink Icon => _icon;
		
		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			_icon.SetBundleId((int)EBundleId.BaseGame);
			yield return _icon;
		}
	}
}