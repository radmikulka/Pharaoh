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
using TycoonBuilder;

namespace TycoonBuilder
{
    public class CCoreGameGameMode : CGameMode<CCoreGameGameModeData>
    {
        private readonly IRegionsController _regionsController;
        private readonly ICFtueFunnel _ftueFunnel;
        private readonly IEventBus _eventBus;

        public CCoreGameGameMode(
            IRegionsController regionsController, 
            ICFtueFunnel ftueFunnel, 
            IEventBus eventBus
            ) : base(EGameModeId.CoreGame)
        {
            _regionsController = regionsController;
            _ftueFunnel = ftueFunnel;
            _eventBus = eventBus;
        }

        public override async UniTask Load(IGameModeData taskData, CancellationToken ct)
        {
            await base.Load(taskData, ct);
            await _regionsController.LoadCurrentRegion(Data.Region, ct);
            _eventBus.Send(new CCoreGameLoadedSignal());
            
            _ftueFunnel.Send(EFtueFunnelStep.LoadScene);
        }
    }
}