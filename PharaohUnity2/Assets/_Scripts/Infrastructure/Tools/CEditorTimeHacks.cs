// =========================================
// AUTHOR: Radek Mikulka
// DATE:   2023-09-12
// =========================================

using System.Collections.Generic;
using UnityEngine;
using System;
using AldaEngine;
using Random = UnityEngine.Random;

namespace TycoonBuilder
{
	public class CEditorTimeHacks : MonoBehaviour
	{
		#if UNITY_EDITOR

		private void Start()
		{
			Time.timeScale = CDebugConfig.Instance.DefaultGameSpeed;
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.KeypadEnter))
			{
				UnityEditor.EditorApplication.isPaused = !UnityEditor.EditorApplication.isPaused;
			}
			
			if (Input.GetKeyDown(KeyCode.Keypad0))
			{
				Time.timeScale = 0f;
			}
			
			if (Input.GetKeyDown(KeyCode.Keypad1))
			{
				Time.timeScale = 0.1f;
			}
			
			if (Input.GetKeyDown(KeyCode.Keypad2))
			{
				Time.timeScale = 1f;
			}
			
			if (Input.GetKeyDown(KeyCode.Keypad3))
			{
				Time.timeScale = 3f;
			}

			if (Input.GetKeyDown(KeyCode.Keypad5))
			{
				Time.timeScale = 5f;
			}

			if (Input.GetKeyDown(KeyCode.Keypad6))
			{
				Time.timeScale = 10f;
			}

			if (Input.GetKeyDown(KeyCode.Keypad9))
			{
				Time.timeScale = 100f;
			}

			if (Input.GetKeyDown(KeyCode.KeypadPlus))
			{
				Time.timeScale += 1f;
			}
			
			if (Input.GetKeyDown(KeyCode.KeypadMinus))
			{
				Time.timeScale = CMath.Max(0f, Time.timeScale - 1f);
			}
		}
		#endif
	}
}