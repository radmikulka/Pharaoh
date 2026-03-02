using RoboRyanTron.SearchableEnum;
using ServerData;
using UnityEngine;

namespace Pharaoh
{
    [CreateAssetMenu(menuName = "____Pharaoh/Configs/Scene")]
    public class CMissionConfig : ScriptableObject, IResourceConfigBase<EMissionId>
    {
        [SerializeField] [SearchableEnum] private EMissionId _id;

        public EMissionId Id => _id;
    }
}