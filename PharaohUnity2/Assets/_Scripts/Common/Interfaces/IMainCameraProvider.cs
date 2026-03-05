// // =========================================
// // AUTHOR: Radek Mikulka
// // DATE:   06.09.2023
// // =========================================

using UnityEngine;

namespace TycoonBuilder
{
    public interface IMainCameraProvider
    {
        IMainCamera Camera { get; }
        
        void SetMainCamera(IMainCamera camera);
    }
}