// // =========================================
// // AUTHOR: Radek Mikulka
// // DATE:   06.09.2023
// // =========================================

using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;
using UnityEngine.SceneManagement;
using Pharaoh;

namespace Pharaoh
{
    public class CCoreGameGameMode : CGameMode<CCoreGameGameModeData>
    {
        private readonly IMissionController _missionController;
        private readonly IEventBus _eventBus;

        public CCoreGameGameMode(IMissionController missionController, IEventBus eventBus) : base(EGameModeId.CoreGame)
        {
            _missionController = missionController;
            _eventBus = eventBus;
        }

        public override async UniTask Load(IGameModeData taskData, CancellationToken ct)
        {
            await base.Load(taskData, ct);
            await _missionController.LoadMission(Data.Mission, ct);
            _eventBus.Send(new CCoreGameLoadedSignal());
        }
    }
}