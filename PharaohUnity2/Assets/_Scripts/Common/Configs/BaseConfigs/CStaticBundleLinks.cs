// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.3.2024
// =========================================

using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

namespace AldaEngine
{
	[CreateAssetMenu(fileName = "StaticBundleLinks", menuName = "____Pharaoh/EditorTasks/StaticBundleLinks")]
	public class CStaticBundleLinks : CScriptableSingletonEditorOnly<CStaticBundleLinks>, IIHaveBundleLinks
	{
		[SerializeField] [BundleLink(true, typeof(Object))] private CBundleLink[] _staticBundleLinks;

		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			foreach (CBundleLink bundleLink in _staticBundleLinks)
			{
				yield return bundleLink;
			}
		}
	}
}