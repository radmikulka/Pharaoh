// =========================================
// AUTHOR: Marek Karaba
// DATE:   25.07.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	[CreateAssetMenu(menuName = "____TycoonBuilder/Configs/YearMilestone")]
	public class CDecadeMilestoneResourceConfig : ScriptableObject, IResourceConfigBase<EDecadeMilestone>, IIHaveBundleLinks
	{
		[SerializeField] private EDecadeMilestone _id;
		[Header("Curved Sprite")]
		[SerializeField] [BundleLink(false, typeof(Sprite))]  private CBundleLink _curvedSprite;
		[Header("Straight Sprite")]
		[SerializeField] [BundleLink(false, typeof(Sprite))]  private CBundleLink _straightSprite;

		public EDecadeMilestone Id => _id;
		public CBundleLink CurvedSprite => _curvedSprite;
		public CBundleLink StraightSprite => _straightSprite;

		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			_curvedSprite.SetBundleId((int) EBundleId.BaseGame);
			yield return _curvedSprite;
			_straightSprite.SetBundleId((int) EBundleId.BaseGame);
			yield return _straightSprite;
		}
	}
}