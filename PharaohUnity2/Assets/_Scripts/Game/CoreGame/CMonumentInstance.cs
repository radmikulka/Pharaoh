using UnityEngine;

namespace Pharaoh.CoreGame
{
    public sealed class CMonumentInstance : MonoBehaviour
    {
        [SerializeField] Transform _entryPoint;
        public Transform EntryPoint => _entryPoint;
    }
}
