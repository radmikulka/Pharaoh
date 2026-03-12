using UnityEngine;

namespace Pharaoh
{
    public sealed class CMonumentInstance : MonoBehaviour
    {
        [SerializeField] private Transform _entryPoint;
        [SerializeField] private CMonumentPart[] _parts;

        public Transform EntryPoint => _entryPoint;
        public CMonumentPart[] Parts => _parts;
    }
}
