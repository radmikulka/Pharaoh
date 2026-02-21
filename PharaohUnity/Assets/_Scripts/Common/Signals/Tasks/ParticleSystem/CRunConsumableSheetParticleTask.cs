// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System;
using ServerData;
using UnityEngine;

namespace Pharaoh
{
    public class CRunConsumableSheetParticleTask
    {
        public readonly RectTransform Start;
        public readonly CConsumableValuable Currency;

        public CRunConsumableSheetParticleTask(RectTransform start, CConsumableValuable currency)
        {
            Currency = currency;
            Start = start;
        }
    }
}