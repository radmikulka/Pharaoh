// =========================================
// AUTHOR: Radek Mikulka
// DATE:   25.09.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CMainCanvasDisabler : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField] private Canvas[] _canvases;

		private IEventBus _eventBus;

		[Inject]
		private void Inject(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}
		
		public void Initialize()
		{
			_eventBus.AddTaskHandler<CSetActiveMainCanvasTask>(HandleSetActiveMainCanvasTask);
		}

		private void HandleSetActiveMainCanvasTask(CSetActiveMainCanvasTask task)
		{
			foreach (Canvas canvas in _canvases)
			{
				canvas.enabled = task.Active;
			}
		}
	}
}