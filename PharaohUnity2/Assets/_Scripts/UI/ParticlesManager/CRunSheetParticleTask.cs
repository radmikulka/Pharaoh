// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System;
using UnityEngine;

namespace TycoonBuilder
{
    public class CRunSheetParticleTask
    {
        public readonly Action<int> OnParticleStepCompleted;
        public readonly RectTransform Start;
        public readonly RectTransform End;
        public readonly EUiParticleId ParticleId;
        public readonly int Count;

        public CRunSheetParticleTask(Action<int> onParticleStepCompleted, RectTransform start, RectTransform end, EUiParticleId particleId, int count)
        {
            OnParticleStepCompleted = onParticleStepCompleted;
            Start = start;
            End = end;
            ParticleId = particleId;
            Count = count;
        }
    }
}