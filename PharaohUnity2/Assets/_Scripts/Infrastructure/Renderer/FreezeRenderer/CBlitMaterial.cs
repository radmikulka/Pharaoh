// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.11.2025
// =========================================

using UnityEngine;
using UnityEngine.Rendering;

namespace Pharaoh
{
	public class CBlitMaterial
	{
		public static readonly Material Material;

		static CBlitMaterial()
		{
			Shader shader = Shader.Find("Hidden/Blit");
			Material = CoreUtils.CreateEngineMaterial(shader);
		}
	}
}