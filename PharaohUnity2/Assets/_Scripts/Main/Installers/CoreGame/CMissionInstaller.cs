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
	public class CMissionInstaller : CSceneDiInstaller
	{
		[SerializeField, Child] private CMissionController  _mission;
		[SerializeField, Child] private CMonumentProvider   _monumentProvider;
		[SerializeField, Child] private CWorkerPath         _workerPath;
		[SerializeField, Child] private CWorkerManager      _workerManager;

		private void OnValidate()
		{
			this.ValidateRefs();
		}

		public override void InstallBindings()
		{
			base.InstallBindings();

			Container.AddSingletonFromInstance(_mission);
			Container.AddSingletonFromInstance(_monumentProvider);
			Container.AddSingletonFromInstance(_workerPath);
			Container.AddSingletonFromInstance(_workerManager);
			Container.AddSingleton<IWorkerConfig, CDummyWorkerConfig>();
			Container.AddSingleton<IMissionStatLimitsProvider, CDummyMissionStatLimitsProvider>();
			Container.AddSingleton<IRewardQueue, CRewardQueue>();
		}
	}
}