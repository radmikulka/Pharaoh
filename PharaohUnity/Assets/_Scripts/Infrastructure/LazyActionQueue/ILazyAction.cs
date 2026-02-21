// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.10.2023
// =========================================

using System.Threading;
using Cysharp.Threading.Tasks;

namespace Pharaoh
{
	public interface ILazyAction
	{
		public int Priority { get; }
		UniTask Execute(CancellationToken ct);
	}
}