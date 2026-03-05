// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.01.2026
// =========================================

using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using ServerData;

namespace TycoonBuilder
{
	public interface IRegionsController
	{
		ERegion ActiveRegion { get; }
		UniTask LoadLiveEvent(ELiveEvent eventId, CancellationToken ct);
		UniTask LoadRegion(ERegion region, CancellationToken ct);
		UniTask LoadCurrentRegion(ERegion dataRegion, CancellationToken ct);
	}
}