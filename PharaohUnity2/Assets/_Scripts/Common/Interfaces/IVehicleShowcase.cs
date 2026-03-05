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
	public interface IVehicleShowcase
	{
		UniTask Show(EVehicle vehicleVehicle, CancellationToken ct);
	}
}