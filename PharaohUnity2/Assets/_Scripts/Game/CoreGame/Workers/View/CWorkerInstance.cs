using UnityEngine;

namespace Pharaoh
{
    public sealed class CWorkerInstance : MonoBehaviour
    {
        // Later: hold Animator reference for walk/idle clips.

        public void Sync(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
        }
    }
}
