// =========================================
// AUTHOR: Radek Mikulka
// DATE:   24.10.2024
// =========================================

using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using Pharaoh.Ui;
using ServerData;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
	public class CScreenManager : AldaEngine.CScreenManager, IInitializable
	{
		[SerializeField] private CanvasGroup[] _rootCanvasGroups;
		
		private IEventBus _eventBus;

		[Inject]
		private void Inject(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			_eventBus.AddTaskHandler<CMenuStateRequest, CMenuStateResponse>(OnStateRequest);
			_eventBus.AddTaskHandler<CCloseTopmostScreenTask>(ProcessCloseTopmostScreenCommand);
		}
		
		private void ProcessCloseTopmostScreenCommand(CCloseTopmostScreenTask task)
		{
			TryCloseTopmostMenu();
		}

		private CMenuStateResponse OnStateRequest(CMenuStateRequest request)
		{
			CMenuStateResponse response = new(IsActive, ActiveMenus.Count);
			return response;
		}
	}
}