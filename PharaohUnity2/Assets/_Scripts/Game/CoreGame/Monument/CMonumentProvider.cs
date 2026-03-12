using AldaEngine.AldaFramework;
using ServerData;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
    public sealed class CMonumentProvider : MonoBehaviour, IConstructable
    {
        private IMissionController _missionController;
        private CResourceConfigs _resourceConfigs;

        private CAldaInstantiator _aldaInstantiator;
        private CMonumentInstance _monument;
        public CMonumentInstance Monument => _monument;

        [Inject]
        private void Inject(
            IMissionController missionController, 
            CResourceConfigs resourceConfigs,
            CAldaInstantiator aldaInstantiator
            )
        {
            _missionController = missionController;
            _aldaInstantiator = aldaInstantiator;
            _resourceConfigs = resourceConfigs;
        }

        public void Construct()
        {
            CMonumentResourceConfig config = _resourceConfigs.Monuments.GetConfig(_missionController.ActiveMonument);
            _monument = _aldaInstantiator.Instantiate<CMonumentInstance>(config.Prefab, transform);
        }
    }
}
