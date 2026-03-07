// =========================================
// AUTHOR: Radek Mikulka
// DATE:   8.3.2024
// =========================================

using System;
using AldaEngine;
using UnityEngine;

namespace Pharaoh.Infrastructure
{
	[CreateAssetMenu(menuName = "____Pharaoh/Others/AssetBundleConfig", fileName = "AssetBundleConfig")]
	public class CPharaohAssetBundle : CAssetBundleConfig
	{
		[SerializeField] private EAssetBundleFlag _flags;

		public EAssetBundleFlag Flags => _flags;
	}

	[Flags]
	public enum EAssetBundleFlag
	{
		None = 0,
		Temp = 1 << 1
	}
}