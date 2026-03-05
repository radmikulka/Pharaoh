// =========================================
// AUTHOR: Radek Mikulka
// DATE:   20.02.2026
// =========================================

namespace TycoonBuilder
{
	public static class CUnitConverter
	{
		private const double MilesToKmFactor = 1.609344;
		private const double FeetToMetersFactor = 0.3048;
		private const double InchesToMetersFactor = 0.0254;

		public static double MphToKmh(double mph)
		{
			return mph * MilesToKmFactor;
		}

		public static double MiToKm(double miles)
		{
			return miles * MilesToKmFactor;
		}

		public static double FtToM(double feet)
		{
			return feet * FeetToMetersFactor;
		}
		
		public static double InToM(double inches)
		{
			return inches * InchesToMetersFactor;
		}
	}
}