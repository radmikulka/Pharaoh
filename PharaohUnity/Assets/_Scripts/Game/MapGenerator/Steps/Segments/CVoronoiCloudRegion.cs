using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Pharaoh.MapGenerator
{
    public class CVoronoiCloudRegion : MonoBehaviour, IClickableItem
    {
        public int RegionId;

        private ISaveManager _saveManager;
        private CUser _user;

        [Inject]
        private void Inject(ISaveManager saveManager, CUser user)
        {
            _saveManager = saveManager;
            _user = user;
        }

        private void Start()
        {
            if (_saveManager == null || _user == null) return;

            int missionKey = (int)_user.ActiveMission.Mission;
            if (_saveManager.Data.RevealedCloudRegions.TryGetValue(missionKey, out var revealed)
                && revealed.Contains(RegionId))
            {
                gameObject.SetActive(false);
            }
        }

        public void OnClicked(RaycastHit hit)
        {
            gameObject.SetActive(false);

            if (_saveManager == null || _user == null) return;

            int missionKey = (int)_user.ActiveMission.Mission;
            if (!_saveManager.Data.RevealedCloudRegions.TryGetValue(missionKey, out var revealed))
                _saveManager.Data.RevealedCloudRegions[missionKey] = revealed = new System.Collections.Generic.HashSet<int>();

            if (revealed.Add(RegionId))
                _saveManager.SaveAsync().Forget();
        }
    }
}

