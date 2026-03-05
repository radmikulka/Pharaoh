// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.12.2025
// =========================================

using System.Diagnostics.CodeAnalysis;
using AldaEngine;
using KBCore.Refs;
using UnityEngine;

namespace TycoonBuilder
{
	[SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
	public class CNormalLiveEventInstaller : CBaseDiInstaller
	{
		[SerializeField, Child] private CEventRegionController _region;

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