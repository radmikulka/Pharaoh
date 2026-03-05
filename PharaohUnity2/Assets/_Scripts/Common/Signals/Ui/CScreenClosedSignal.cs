// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using AldaEngine;

namespace TycoonBuilder
{
    public class CScreenClosedSignal : IEventBusSignal
    {
        public readonly IUiScreen Screen;

        public CScreenClosedSignal(IUiScreen screen)
        {
            Screen = screen;
        }
    }
}