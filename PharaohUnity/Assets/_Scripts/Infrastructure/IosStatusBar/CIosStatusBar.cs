/*
//=========================================//
AUTHOR:		Radek Mikulka
DATE:		10.10.2022
FUNCTION:	
//=========================================//
*/

using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace Pharaoh
{
	public class CIosStatusBar : MonoBehaviour
	{
		#if UNITY_IOS

		private float lastTouchTime;

		private void Update()
		{
			float t = Time.realtimeSinceStartup;

			int touchCount = Input.touchCount;
			for (int i = 0; i < touchCount; i++)
			{
				Touch touch = Input.touches[i];
				if (touch.phase == TouchPhase.Ended && Vector3.SqrMagnitude(touch.rawPosition - touch.position) < 10f)
				{
					lastTouchTime = t;
				}
			}

			SetHideButton(!(t < lastTouchTime + 0.5f));
		}

		private void SetHideButton(bool state)
		{
			if(Device.hideHomeButton == state)
				return;
			Device.hideHomeButton = state;
		}
		
		#endif
		 
	}
}