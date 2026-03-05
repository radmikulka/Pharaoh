// =========================================
// AUTHOR: Radek Mikulka
// DATE:   07.11.2025
// =========================================

namespace TycoonBuilder
{
	public interface IGraphicsQualityProvider
	{
		EGraphicsQuality Quality { get; }
		void DeferChanges();
		void ApplyDeferredChanges();
	}
}