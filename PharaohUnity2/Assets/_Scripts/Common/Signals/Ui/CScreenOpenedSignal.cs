// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
    public class CScreenOpenedSignal : IEventBusSignal
    {
        public readonly IUiScreen Screen;

        public CScreenOpenedSignal(IUiScreen screen)
        {
            Screen = screen;
        }
    }
}