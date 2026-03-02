using UnityEngine;

namespace GiantGrey.TileWorldCreator.Samples
{
    public class CameraRotation : MonoBehaviour
    {
        public float rotationSpeed = 10f;

        void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}