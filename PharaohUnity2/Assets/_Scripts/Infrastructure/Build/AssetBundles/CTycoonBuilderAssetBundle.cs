// =========================================
// AUTHOR: Radek Mikulka
// DATE:   8.3.2024
// =========================================

using System;
using AldaEngine;
using UnityEngine;

namespace TycoonBuilder.Infrastructure
{
	[CreateAssetMenu(menuName = "____TycoonBuilder/Others/AssetBundleConfig", fileName = "AssetBundleConfig")]
	public class CTycoonBuilderAssetBundle : CAssetBundleConfig
	{
		[SerializeField] private EAssetBundleFlag _flags;

		public EAssetBundleFlag Flags => _flags;
	}

	[Flags]
	public enum EAssetBundleFlag
	{
		None = 0,
		PremiumOffer = 1 << 0,
		Temp = 1 << 1
	}
}