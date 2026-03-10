using AldaEngine;
using AldaEngine.AldaFramework;
using Zenject;

namespace Pharaoh
{
	public class CDialogTaskHandler : IInitializable
	{
		private IEventBus _eventBus;
		private IScreenManager _screenManager;

		[Inject]
		private void Inject(IEventBus eventBus, IScreenManager screenManager)
		{
			_eventBus = eventBus;
			_screenManager = screenManager;
		}

		public void Initialize()
		{
			_eventBus.AddTaskHandler<CShowDialogTask>(OnShowDialog);
		}

		private void OnShowDialog(CShowDialogTask task)
		{
			EScreenId screenId = task.IsOverlay ? EScreenId.DialogOverlay : EScreenId.Dialog;
			_screenManager.OpenMenu<CDialogScreen>((int)screenId, screen => screen.Configure(task));
		}
	}
}
