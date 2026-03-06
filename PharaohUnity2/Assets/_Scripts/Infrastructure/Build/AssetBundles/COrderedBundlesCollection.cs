// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.3.2024
// =========================================

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Pharaoh;
using Pharaoh.Infrastructure;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

namespace AldaEngine
{
	[CreateAssetMenu(fileName = "OrderedBundlesCollection", menuName = "____TycoonBuilder/EditorTasks/TycoonOrderedBundlesCollection")]
	public class COrderedBundlesCollection : CBaseOrderedBundlesCollection
	{
		[SerializeField] private CAssetBundleToBuild[] _orderedLocalBundles;

		public override IReadOnlyList<CAssetBundleToBuild> OrderedAssetBundles
		{
			get
			{
				List<CAssetBundleToBuild> result = new();
				result.AddRange(_orderedLocalBundles);
				return result;
			}
		}
	}
}
#endif