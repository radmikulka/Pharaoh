// =========================================
// AUTHOR: Marek Karaba
// DATE:   13.02.2026
// =========================================

using System.Collections.Generic;
using AldaEngine;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	[CreateAssetMenu(menuName = "____TycoonBuilder/Configs/DecadePassResourceConfig")]
	public class CDecadePassResourceConfig : ScriptableObject, IResourceConfigBase<EDecadeMilestone>, IIHaveBundleLinks
	{
		[SerializeField] private EDecadeMilestone _id;
		[Header("Left Side Sprite 261x675")]
		[SerializeField] [BundleLink(false, typeof(Sprite))]  private CBundleLink _leftSideSprite;
		[Header("Right Side Sprite 460x675")]
		[SerializeField] [BundleLink(false, typeof(Sprite))]  private CBundleLink _rightSideSprite;

		public EDecadeMilestone Id => _id;
		public CBundleLink LeftSideSprite => _leftSideSprite;
		public CBundleLink RightSideSprite => _rightSideSprite;

		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			_leftSideSprite.SetBundleId((int) EBundleId.BaseGame);
			yield return _leftSideSprite;
			_rightSideSprite.SetBundleId((int) EBundleId.BaseGame);
			yield return _rightSideSprite;
		}
	}
}