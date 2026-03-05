// =========================================
// AUTHOR: Radek Mikulka
// DATE:   25.05.2025
// =========================================

using System;
using System.ComponentModel;
using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using NaughtyAttributes;
using ServerData;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CRadekTest : MonoBehaviour, IAldaFrameworkComponent
	{
		private IEventBus _eventBus;

		[Inject]
		private void Inject(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}

		public void button_TestR2()
		{
			_eventBus.ProcessTaskAsync(new CLoadGameModeTask(new CCoreGameGameModeData(ERegion.Region2)), CancellationToken.None);
		}
	}
}