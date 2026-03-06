// // =========================================
// // AUTHOR: Radek Mikulka
// // DATE:   06.09.2023
// // =========================================

using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Pharaoh;

namespace Pharaoh
{
    public abstract class CGameMode<T> : IGameMode where T : IGameModeData
    {
        public EGameModeId Id { get; }
        public T Data { get; private set; }

        protected CGameMode(EGameModeId id)
        {
            Id = id;
        }

        public virtual UniTask Load(IGameModeData taskData, CancellationToken ct)
        {
            Data = (T)taskData;
            return UniTask.CompletedTask;
        }
        
        public virtual UniTask Unload(CancellationToken ct) { return UniTask.CompletedTask; }
        
        public UniTask Preload() { return UniTask.CompletedTask; }
        
        public virtual void Start()
        {
            
        }
    }
}