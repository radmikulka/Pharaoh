// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
    public class CCameraBorders
    {
        public Bounds Bounds { get; private set; }

        public void SetBounds(Bounds bounds)
        {
            Bounds = bounds;
        }
    }
}