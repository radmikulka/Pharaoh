using UnityEngine;

namespace GiantGrey.TileWorldCreator.Samples
{
	public class SmoothCameraFollow : MonoBehaviour
	{
			public Vector3 offset;
			public float smoothTime = 0.3F;
			public Transform target;
			
			private Vector3 velocity = Vector3.zero;
			
			
			
			void Update()
			{
				if (target == null)
					return;
					
					
				Vector3 targetPosition = target.position + offset;
				transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
			}
	}
}