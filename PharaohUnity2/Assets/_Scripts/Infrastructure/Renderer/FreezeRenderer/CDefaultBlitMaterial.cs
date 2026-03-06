// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.11.2025
// =========================================

using UnityEngine;
using UnityEngine.Rendering;

namespace Pharaoh
{
	public class CDefaultBlitMaterial
	{
		public static readonly Material Material;

		static CDefaultBlitMaterial()
		{
			Shader shader = Shader.Find("Hidden/Blit");
			Material = CoreUtils.CreateEngineMaterial(shader);
		}
	}
}