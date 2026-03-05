// =========================================
// AUTHOR: Radek Mikulka
// DATE:   15.08.2025
// =========================================

using Unity.Cinemachine;
using UnityEngine;

namespace TycoonBuilder
{
	public interface IMainCamera
	{
		Camera Camera { get; }
		
		void FreezeRender(bool state);
		void SetCameraBlendMode(bool allowBlend);
		bool IsLiveCamera(CinemachineCamera cam);
		Vector3 WorldToScreenPoint(Vector3 point);
		Ray ScreenPointToRay(Vector2 screenCoords);
		Vector3 InitialForwardDirection { get; }
	}
}