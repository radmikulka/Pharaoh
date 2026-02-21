// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.01.2026
// =========================================

using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using ServerData;

namespace Pharaoh
{
	public interface IMissionController
	{
		EMissionId ActiveMissionId { get; }
		UniTask LoadMission(EMissionId mission, CancellationToken ct);
	}
}