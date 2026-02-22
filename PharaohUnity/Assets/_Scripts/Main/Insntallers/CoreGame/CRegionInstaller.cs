// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.07.2025
// =========================================

using System;
using System.Diagnostics.CodeAnalysis;
using AldaEngine;
using KBCore.Refs;
using Pharaoh.Map;
using UnityEngine;

namespace Pharaoh
{
	[SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
	public class CRegionInstaller : CSceneDiInstaller
	{
		[SerializeField, Child] private CMission _mission;
		[SerializeField, Child] private CMapInstance _mapInstance;

		private void OnValidate()
		{
			this.ValidateRefs();
		}

		public override void InstallBindings()
		{
			base.InstallBindings();

			Container.AddSingletonFromInstance(_mission);
			Container.AddSingletonFromInstance(_mapInstance);
		}
	}
}