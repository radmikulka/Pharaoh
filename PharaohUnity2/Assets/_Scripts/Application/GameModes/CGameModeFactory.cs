// // =========================================
// // AUTHOR: Radek Mikulka
// // DATE:   06.09.2023
// // =========================================

using System;
using Pharaoh;
using Zenject;

namespace Pharaoh
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
                    return _diContainer.Resolve<CCoreGameMode>();
                default:
                    throw new ArgumentOutOfRangeException(nameof(id), id, null);
            }
        }
    }
}