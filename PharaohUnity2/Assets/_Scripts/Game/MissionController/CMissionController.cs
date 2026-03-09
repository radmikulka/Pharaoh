// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.07.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using ServerData.Dto;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
    public class CMissionController : MonoBehaviour, IConstructable
    {
        [SerializeField] private EMissionId _missionId;

        private readonly Plane        _cameraPlane = new(CVector3.Up, new Vector3(0f, 27f, 0f));
        private readonly CMissionData _data        = new();

        private GameObject                _sceneRoot;
        private IEventBus                 _eventBus;
        private IMissionStatLimitsProvider _limits;

        public EMissionId MissionId        => _missionId;
        public bool       IsActive         { get; private set; }
        public int        WorkerCountLevel => _data.WorkerCountLevel;
        public int        WorkerSpeedLevel => _data.WorkerSpeedLevel;
        public int        ProfitLevel      => _data.ProfitLevel;
        public int        SoftCurrency     => _data.SoftCurrency;
        public float      MonumentProgress => _data.MonumentProgress;

        [Inject]
        private void Inject(IEventBus eventBus, IMissionStatLimitsProvider limits)
        {
            _eventBus = eventBus;
            _limits   = limits;
        }

        public void Construct()
        {
            _sceneRoot = gameObject.scene.GetRootGameObjects()[0];
            _eventBus.Subscribe<CMissionStatUpgradedSignal>(OnStatUpgraded);
            _eventBus.AddTaskHandler<CClaimMilestoneRewardTask>(OnClaimMilestoneReward);
            _eventBus.AddTaskHandler<CMissionDataRequest, CMissionDataResponse>(OnMissionDataRequest);
            _eventBus.AddTaskHandler<CAddSoftCurrencyTask>(OnAddSoftCurrency);
        }

        public Plane GetCameraPlane() => _cameraPlane;

        public int GetLevel(EMissionStatId stat) => _data.GetLevel(stat);

        public void SetActive(bool state)
        {
            IsActive = state;
            _sceneRoot.SetActive(state);
        }

        public void AddSoftCurrency(int amount)
        {
            _data.SoftCurrency += amount;
            _eventBus.Send(new CSoftCurrencyChangedSignal(_data.SoftCurrency, amount));
        }

        public void AddMonumentProgress(float amount)
        {
            _data.MonumentProgress = Mathf.Clamp01(_data.MonumentProgress + amount);
            _eventBus.Send(new CMonumentProgressChangedSignal(_data.MonumentProgress));
        }

        public CMissionDataDto ToDto() => _data.ToDto();

        private CMissionDataResponse OnMissionDataRequest(CMissionDataRequest request)
        {
            return new CMissionDataResponse(_data.SoftCurrency, _data.MonumentProgress, _data.ClaimedMilestones);
        }

        private void OnClaimMilestoneReward(CClaimMilestoneRewardTask task)
        {
            if (_data.MonumentProgress < task.MilestoneThreshold)
                return;

            if (!_data.ClaimedMilestones.Add(task.MilestoneThreshold))
                return;

            _eventBus.Send(new CMilestoneRewardClaimedSignal(task.MilestoneThreshold));
        }

        private void OnAddSoftCurrency(CAddSoftCurrencyTask task)
        {
            AddSoftCurrency(task.Amount);
        }

        private void OnStatUpgraded(CMissionStatUpgradedSignal signal)
        {
            int maxLevel = _limits.GetMaxLevel(signal.Stat);
            int current  = GetLevel(signal.Stat);
            if (current >= maxLevel)
                return;

            switch (signal.Stat)
            {
                case EMissionStatId.WorkerCount: _data.WorkerCountLevel++; break;
                case EMissionStatId.WorkerSpeed: _data.WorkerSpeedLevel++; break;
                case EMissionStatId.Profit:      _data.ProfitLevel++;      break;
            }
            _eventBus.Send(new CMissionStatLevelChangedSignal(signal.Stat, GetLevel(signal.Stat)));
        }
    }
}
