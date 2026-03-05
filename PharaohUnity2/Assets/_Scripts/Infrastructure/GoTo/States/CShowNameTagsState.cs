// =========================================
// AUTHOR: Marek Karaba
// DATE:   09.10.2025
// =========================================

using AldaEngine;

namespace TycoonBuilder.GoToStates
{
	public class CShowNameTagsState : CGoToFsmStateWithData<CLockObject>
	{
		private CLockObject _lockObject;
		
		public CShowNameTagsState(IEventBus eventBus) : base(eventBus)
		{
		}

		public override void SetData(CLockObject lockObject)
		{
			_lockObject = lockObject;
		}

		public override void Start()
		{
			ShowNameTags();
			IsCompleted = true;
		}

		private void ShowNameTags()
		{
			EventBus.ProcessTask(new CRemoveFloatingWindowsBlockerTask(_lockObject));
		}
	}
}