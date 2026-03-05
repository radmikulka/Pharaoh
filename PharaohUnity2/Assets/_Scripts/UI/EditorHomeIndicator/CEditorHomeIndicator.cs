// =========================================
// AUTHOR: Marek Karaba
// DATE:   30.07.2025
// =========================================

using AldaEngine.AldaFramework;
using UnityEngine;
using UnityEngine.UI;

namespace TycoonBuilder.EditorHomeIndicator
{
	public class CEditorHomeIndicator : MonoBehaviour
	#if UNITY_EDITOR
	, IConstructable
	#endif
	{
		#if UNITY_EDITOR
		[SerializeField] private Image _indicatorImage;
		
		public void Construct()
		{
			if(!_indicatorImage)
				return;
			_indicatorImage.enabled = CDebugConfig.Instance.ShowDebugIosHomeButton;
		}
		#endif
	}
}