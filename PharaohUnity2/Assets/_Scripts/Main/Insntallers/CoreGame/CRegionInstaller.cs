// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.07.2025
// =========================================

using System;
using System.Diagnostics.CodeAnalysis;
using AldaEngine;
using KBCore.Refs;
using UnityEngine;

namespace TycoonBuilder
{
	[SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
	public class CRegionInstaller : CSceneDiInstaller
	{
		[SerializeField, Child] private CRegionController _region;

		private void OnValidate()
		{
			this.ValidateRefs();
		}

		public override void InstallBindings()
		{
			base.InstallBindings();
			
			Container.AddSingletonFromInstance(_region);
		}
	}
}