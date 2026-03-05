// =========================================
// AUTHOR: Marek Karaba
// DATE:   09.10.2025
// =========================================

using AldaEngine;
using Cysharp.Threading.Tasks;

namespace TycoonBuilder.GoToStates
{
	public class CHideNameTagsState : CGoToFsmStateWithData<CLockObject>
	{
		private CLockObject _lockObject;
		
		public CHideNameTagsState(IEventBus eventBus) : base(eventBus)
		{
		}

		public override void SetData(CLockObject lockObject)
		{
			_lockObject = lockObject;
		}

		public override void Start()
		{
			HideNameTags();
			IsCompleted = true;
		}
		
		private void HideNameTags()
		{
			EventBus.ProcessTask(new CAddFloatingWindowsBlockerTask(_lockObject));
		}
	}
}