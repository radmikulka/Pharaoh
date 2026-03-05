// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.11.2025
// =========================================

namespace TycoonBuilder
{
	public interface ICFtueFunnel
	{
		EFtueFunnelStep CurrentStep { get; }
		void Send(EFtueFunnelStep step);
	}
}