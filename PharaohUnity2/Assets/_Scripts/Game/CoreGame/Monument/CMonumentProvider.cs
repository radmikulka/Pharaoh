using UnityEngine;

namespace Pharaoh
{
    public sealed class CMonumentProvider : MonoBehaviour
    {
        [SerializeField] private CMonumentInstance _monument;
        public CMonumentInstance Monument => _monument;
    }
}
