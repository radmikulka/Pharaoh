// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System;
using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using ServerData;
using UnityEngine;
using Pharaoh;
using Zenject;

namespace Pharaoh
{
    public class CCameraMover : ValidatedMonoBehaviour
    {
        public void Warp(Vector3 position)
        {
            transform.position = position;
        }
    }
}