// =========================================
// AUTHOR: Marek Karaba
// DATE:   04.07.2025
// =========================================

using AldaEngine;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	[System.Serializable]
	public class CFacialExpressionSpritePair
	{
		[SerializeField] private ECharacterFacialExpression _characterFacialExpression;
		[SerializeField] [BundleLink(false, typeof(Sprite))] private CBundleLink _visual2d;
		
		public CBundleLink Visual2d => _visual2d;
		
		private EBundleId _bundleId;

		public ECharacterFacialExpression CharacterFacialExpression => _characterFacialExpression;

		public void SetBundleId(int bundleId)
		{
			_bundleId = (EBundleId)bundleId;
			_visual2d.SetBundleId((int)_bundleId);
		}
	}
}