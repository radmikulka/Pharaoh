using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
    public class CUiMissionProgressBar : MonoBehaviour
    {
        [SerializeField] private CUiComponentSlider _fillBar;
        [SerializeField] private CUiMilestoneItem _milestoneItemPrefab;
        [SerializeField] private Transform _milestonesContainer;

        private IEventBus _eventBus;
        private CDesignMissionConfigs _missionConfigs;

        private readonly List<CUiMilestoneItem> _milestoneItems = new();
        private IReadOnlyList<SMilestoneConfig> _milestones;
        private HashSet<float> _claimedMilestones;
        private float _currentProgress;

        [Inject]
        private void Inject(IEventBus eventBus, CDesignMissionConfigs missionConfigs)
        {
            _eventBus       = eventBus;
            _missionConfigs = missionConfigs;

            _eventBus.Subscribe<CMonumentProgressChangedSignal>(OnProgressChanged);
            _eventBus.Subscribe<CMilestoneRewardClaimedSignal>(OnMilestoneRewardClaimed);
            _eventBus.Subscribe<CMissionActivatedSignal>(OnMissionActivated);
        }

        private void OnMissionActivated(CMissionActivatedSignal signal)
        {
            SetupMilestones(signal.Mission);
            RefreshFromServer();
        }

        private void SetupMilestones(EMissionId missionId)
        {
            foreach (CUiMilestoneItem item in _milestoneItems)
            {
                Destroy(item.gameObject);
            }
            _milestoneItems.Clear();

            CMissionConfig config = _missionConfigs.GetMission(missionId);
            if (config == null)
                return;

            _milestones = config.Milestones;

            foreach (SMilestoneConfig milestone in _milestones)
            {
                CUiMilestoneItem item = Instantiate(_milestoneItemPrefab, _milestonesContainer);
                item.Init(milestone.Threshold, null, OnMilestoneClaimClicked);
                _milestoneItems.Add(item);
            }
        }

        private void RefreshFromServer()
        {
            CMissionDataResponse response = _eventBus.ProcessTask<CMissionDataRequest, CMissionDataResponse>();
            if (response == null)
                return;

            _currentProgress    = response.MonumentProgress;
            _claimedMilestones  = response.ClaimedMilestones;

            UpdateProgressDisplay();
        }

        private void OnProgressChanged(CMonumentProgressChangedSignal signal)
        {
            _currentProgress = signal.Progress;
            UpdateProgressDisplay();
        }

        private void UpdateProgressDisplay()
        {
            _fillBar.SetValue(_currentProgress);

            for (int i = 0; i < _milestoneItems.Count; i++)
            {
                float threshold = _milestoneItems[i].Threshold;
                bool reached = _currentProgress >= threshold;
                bool claimed = _claimedMilestones != null && _claimedMilestones.Contains(threshold);
                _milestoneItems[i].SetReachable(reached, claimed);
            }
        }

        private void OnMilestoneRewardClaimed(CMilestoneRewardClaimedSignal signal)
        {
            _claimedMilestones?.Add(signal.MilestoneThreshold);

            foreach (CUiMilestoneItem item in _milestoneItems)
            {
                if (Mathf.Approximately(item.Threshold, signal.MilestoneThreshold))
                {
                    item.SetReachable(true, true);
                    break;
                }
            }
        }

        private void OnMilestoneClaimClicked(float threshold)
        {
            _eventBus.ProcessTask(new CClaimMilestoneRewardTask(threshold));
        }
    }
}
