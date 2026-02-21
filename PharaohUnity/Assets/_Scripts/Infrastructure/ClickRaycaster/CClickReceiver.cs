// =========================================
// AUTHOR: Radek Mikulka
// DATE:   07.10.2025
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;

namespace Pharaoh
{
	public class CClickReceiver : MonoBehaviour, IConstructable, IClickableItem
	{
		private CEvent _onClickEvent;

		public void Construct()
		{
			_onClickEvent = new CEvent("OnClickEvent", gameObject);
		}

		public void AddListener(Action onClick)
		{
			_onClickEvent.Subscribe(onClick);
		}

		public void OnClicked()
		{
			_onClickEvent.Invoke();
		}

		private void OnDrawGizmos()
		{
			TryDrawCollider();
		}

		private void TryDrawCollider()
		{
			BoxCollider boxCollider = GetComponent<BoxCollider>();
			if (!boxCollider)
				return;

			ValidateCollider(boxCollider);
			
			Matrix4x4 originalMatrix = Gizmos.matrix;
			
			Color color = Color.green.WithAlpha(0.5f);
			Gizmos.matrix = Matrix4x4.TRS(transform.position + boxCollider.transform.rotation * boxCollider.center, transform.rotation, Vector3.one);
			Gizmos.color = color.WithAlpha(color.a * 0.5f);
			Gizmos.DrawWireCube(Vector3.zero, boxCollider.size);
			Gizmos.color = color;
			Gizmos.DrawCube(Vector3.zero, boxCollider.size);
			Gizmos.matrix = originalMatrix;
		}

		private void ValidateCollider(BoxCollider boxCollider)
		{
			if(boxCollider.transform.localScale != CVector3.One)
			{
				Debug.LogWarning($"BoxCollider on {name} has localScale != (1,1,1). This may cause issues with collider size.", boxCollider);
			}
		}
	}
}