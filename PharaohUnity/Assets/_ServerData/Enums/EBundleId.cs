// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using System.Collections.Generic;
using System;
using AldaEngine;

namespace ServerData
{
	#if UNITY_CLIENT
	[BundleEnum]
	#endif
	public enum EBundleId
	{
        None = 0,
        BaseGame = 1,
        BaseGameScene = 2,
        CoreGame = 3,
        CoreGameScenes = 4,

        Mission1_1_Environment = 1_000,
        Mission1_1_Scene = 1_001
    }
}