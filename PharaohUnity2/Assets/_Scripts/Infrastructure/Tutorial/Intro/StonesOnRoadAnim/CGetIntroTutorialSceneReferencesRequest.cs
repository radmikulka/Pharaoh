// =========================================
// AUTHOR: Radek Mikulka
// DATE:   22.09.2025
// =========================================

namespace TycoonBuilder
{
	public class CGetIntroTutorialSceneReferencesRequest
	{
		
	}

	public class CGetIntroTutorialSceneReferencesResponse
	{
		public readonly CIntroTutorialSceneReferences References;

		public CGetIntroTutorialSceneReferencesResponse(CIntroTutorialSceneReferences references)
		{
			References = references;
		}
	}
}