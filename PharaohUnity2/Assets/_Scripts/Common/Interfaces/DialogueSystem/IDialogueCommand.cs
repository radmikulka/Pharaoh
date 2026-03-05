// =========================================
// AUTHOR: Marek Karaba
// DATE:   04.07.2025
// =========================================

using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;

namespace TycoonBuilder
{
	public interface IDialogueCommand
	{
		UniTask Execute(IDialogueHandler dialogueHandler, CancellationTokenSource cancellationTokenSource);
	}
}