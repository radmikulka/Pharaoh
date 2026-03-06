// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.07.2025
// =========================================

using System;
using System.Diagnostics.CodeAnalysis;
using AldaEngine;
using KBCore.Refs;
using UnityEngine;

namespace Pharaoh
{
	[SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
	public class CRegionInstaller : CSceneDiInstaller
	{
		[SerializeField, Child] private CMissionController mission;

		private void OnValidate()
		{
			this.ValidateRefs();
		}

		public override void InstallBindings()
		{
			base.InstallBindings();
			
			Container.AddSingletonFromInstance(mission);
		}
	}
}