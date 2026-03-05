// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

namespace TycoonBuilder
{
	public static class CScreen
	{
		public const float ReferenceWidth = 1890f;
		public const float ReferenceHeight = 900f;

		public static float ReferenceWidthMod => UnityEngine.Screen.width / ReferenceWidth;
		public static float ReferenceHeightMod => UnityEngine.Screen.height / ReferenceHeight;
		public static float ReferenceScreenRaito => ReferenceWidth / ReferenceHeight;
		public static float ScreenRaito => UnityEngine.Screen.width / (float)UnityEngine.Screen.height;
	}
}