// // =========================================
// // AUTHOR: Radek Mikulka
// // DATE:   06.09.2023
// // =========================================

using KBCore.Refs;
using UnityEngine;

namespace TycoonBuilder
{
    public class CMainCameraProvider : IMainCameraProvider
    {
        public IMainCamera Camera { get; private set; }
        
        public void SetMainCamera(IMainCamera camera)
        {
            Camera = camera;
        }
    }
}