// =========================================
// AUTHOR: Radek Mikulka
// DATE:   21.10.2025
// =========================================

using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public class CSmartArrowOverPoint : CBaseSmartArrow
	{
		private CancellationTokenSource _cts;
		
		public CSmartArrowOverPoint(
			ITutorialCommentator tutorialCommentator, 
			ISmartArrowLocker smartArrowLocker, 
			IDialogueHandler dialogueHandler, 
			IEventBus eventBus, 
			CUser user) 
			: base(tutorialCommentator, smartArrowLocker, dialogueHandler, eventBus, user)
		{
			
		}

		public async UniTask Show(Vector3 point, CancellationToken ct)
		{
			_cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
			
			ShowAt(point, new Vector2(0f, 140f));
			
			while (true)
			{
				bool shouldBeVisible = ShouldBeVisible();
				SetVisible(shouldBeVisible, 0.2f);

				await UniTask.Yield(_cts.Token);
			}
		}

		public override void Destroy()
		{
			base.Destroy();
			_cts.Cancel();
		}
	}
}