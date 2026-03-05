// =========================================
// AUTHOR: Radek Mikulka
// DATE:   15.08.2025
// =========================================

using System;
using AldaEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace TycoonBuilder
{
	public class CDebugModeObject : MonoBehaviour
	{
		private void Awake()
		{
			Refresh();
		}

		#if UNITY_EDITOR

		private void Start()
		{
			CDebugConfig.Instance.DebugModeChanged += DebugModeChanged;
		}

		private void OnDestroy()
		{
			CDebugConfig.Instance.DebugModeChanged -= DebugModeChanged;
		}

		private void DebugModeChanged(bool state)
		{
			Refresh();
		}

		#endif
		
		private void Refresh()
		{
			bool shouldBeActive = CDebugConfig.Instance.DebugMode;
			gameObject.SetActiveObject(shouldBeActive);
		}

	}
}