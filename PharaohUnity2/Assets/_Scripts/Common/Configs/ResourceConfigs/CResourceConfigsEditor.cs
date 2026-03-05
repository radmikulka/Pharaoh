// =========================================
// AUTHOR: Marek Karaba
// DATE:   04.07.2025
// =========================================

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using System;
using AldaEngine;
using Random = UnityEngine.Random;

namespace TycoonBuilder
{
	[AssetPath("_Sources/Configs", "ResourceConfigs")]
	public class CResourceConfigsEditor : CScriptableSingletonEditorOnly<CResourceConfigs>
	{
		
	}
}
#endif