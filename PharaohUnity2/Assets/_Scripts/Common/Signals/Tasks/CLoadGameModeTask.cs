// // =========================================
// // AUTHOR: Radek Mikulka
// // DATE:   06.09.2023
// // =========================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TycoonBuilder;

namespace TycoonBuilder
{
    public class CLoadGameModeTask
    {
        public readonly Func<CancellationToken, Task> OnLoadCompleted;
        public readonly IGameModeData Data;

        public CLoadGameModeTask(IGameModeData data, Func<CancellationToken, Task> onLoadCompleted = null)
        {
            OnLoadCompleted = onLoadCompleted;
            Data = data;
        }
    }
}