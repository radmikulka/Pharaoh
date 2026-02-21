// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.12.2025
// =========================================

using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Pharaoh
{
	public interface IGameMode
	{
		EGameModeId Id { get; }
		void Start();
		UniTask Load(IGameModeData taskData, CancellationToken ct);
	}
}