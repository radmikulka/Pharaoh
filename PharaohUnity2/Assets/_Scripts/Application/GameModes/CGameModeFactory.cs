// // =========================================
// // AUTHOR: Radek Mikulka
// // DATE:   06.09.2023
// // =========================================

using System;
using TycoonBuilder;
using Zenject;

namespace TycoonBuilder
{
    public class CGameModeFactory
    {
        private readonly DiContainer _diContainer;

        public CGameModeFactory(DiContainer diContainer)
        {
            _diContainer = diContainer;
        }

        public IGameMode CreateGameMode(EGameModeId id)
        {
            switch (id)
            {
                case EGameModeId.CoreGame:
                    return _diContainer.Resolve<CCoreGameGameMode>();
                case EGameModeId.RegionLiveEvent:
                    return _diContainer.Resolve<CRegionLiveEventGameGameMode>();
                default:
                    throw new ArgumentOutOfRangeException(nameof(id), id, null);
            }
        }
    }
}