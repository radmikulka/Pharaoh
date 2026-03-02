// =========================================
// AUTHOR: Radek Mikulka
// DATE:   01.03.2026
// =========================================

using System.Threading;
using Cysharp.Threading.Tasks;
using ServerData;

namespace Pharaoh
{
    public interface ISaveManager
    {
        CSaveData Data { get; }
        bool HasSave { get; }
        UniTask SaveAsync(CancellationToken ct = default);
        UniTask LoadAsync(CancellationToken ct = default);
    }
}
