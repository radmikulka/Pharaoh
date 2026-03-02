// =========================================
// AUTHOR: Radek Mikulka
// DATE:   01.03.2026
// =========================================

using System.Threading;
using Cysharp.Threading.Tasks;
using ServerData;

namespace Pharaoh
{
    public class CSaveManager : ISaveManager
    {
        private CSaveData _data = new();

        public CSaveData Data => _data;
        public bool HasSave => false; // TODO: implement when server I/O is added

        public async UniTask SaveAsync(CancellationToken ct = default)
        {
            // TODO: serialize _data to binary (MessagePack) and send to server
            await UniTask.CompletedTask;
        }

        public async UniTask LoadAsync(CancellationToken ct = default)
        {
            // TODO: fetch from server and deserialize into _data
            await UniTask.CompletedTask;
        }
    }
}
