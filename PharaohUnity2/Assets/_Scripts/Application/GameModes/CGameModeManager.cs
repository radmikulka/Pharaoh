// // =========================================
// // AUTHOR: Radek Mikulka
// // DATE:   06.09.2023
// // =========================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TycoonBuilder;

namespace TycoonBuilder
{
    public class CGameModeManager : IInitializable
    {
        private readonly Dictionary<EGameModeId, IGameMode> _activeGameModes = new();
        private readonly CGameModeFactory _gameModeFactory;
        private readonly ILoadingScreen _loadingScreen;
        private readonly IEventBus _eventBus;

        public IGameMode ActiveGameMode { get; private set; }

        public CGameModeManager(
            CGameModeFactory gameModeFactory, 
            ILoadingScreen loadingScreen, 
            IEventBus eventBus
            )
        {
            _gameModeFactory = gameModeFactory;
            _loadingScreen = loadingScreen;
            _eventBus = eventBus;
        }

        public void Initialize()
        {
            _eventBus.AddAsyncTaskHandler<CLoadGameModeTask>(LoadGameMode);
        }

        private async Task LoadGameMode(CLoadGameModeTask task, CancellationToken ct)
        {
            await _loadingScreen.Show(ct);
            _loadingScreen.SetInfoText("Loading.LoadingGame", true);
            try
            {
                ActiveGameMode = GetOrCreateGameMode(task.Data.GameModeId);
                await ActiveGameMode.Load(task.Data, ct);
                ActiveGameMode.Start();

                _eventBus.Send(new CGameModeStartedSignal(task.Data));

                if (task.OnLoadCompleted != null)
                {
                    await task.OnLoadCompleted(ct);
                }
            }
            catch (Exception e)
            {
                CUnityReadableException readableException = new(e);
                Debug.LogError(readableException);
            }
            await _loadingScreen.Hide(ct);
        }

        private IGameMode GetOrCreateGameMode(EGameModeId gameModeId)
        {
            if (_activeGameModes.TryGetValue(gameModeId, out var gameMode))
                return gameMode;

            gameMode = _gameModeFactory.CreateGameMode(gameModeId);
            _activeGameModes.Add(gameMode.Id, gameMode);
            return gameMode;
        }
    }
}