// =========================================
// AUTHOR: Radek Mikulka
// DATE:   22.09.2025
// =========================================

namespace TycoonBuilder
{
	public class CPlayCameraShakeTask
	{
		public readonly float Duration;
		public readonly float Amplitude;
		public readonly float Frequency;

		public CPlayCameraShakeTask(float duration, float amplitude, float frequency)
		{
			Duration = duration;
			Amplitude = amplitude;
			Frequency = frequency;
		}
	}
}