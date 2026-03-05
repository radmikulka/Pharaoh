// =========================================
// AUTHOR: Marek Karaba
// DATE:   04.07.2025
// =========================================

using AldaEngine;
using UnityEngine;

namespace TycoonBuilder
{
	public class CWaitForInput : IWaitType
	{
		public static readonly IWaitType Instance = new CWaitForInput();
		
		private CWaitForInput()
		{
			
		}

		public bool IsCompleted()
		{
			if (CPlatform.IsEditor)
			{
				return Input.GetKeyUp(KeyCode.Mouse0);
			}

			if (Input.touchCount == 0)
				return false;
			
			return Input.GetTouch(0).phase == TouchPhase.Ended;
		}
	}
}