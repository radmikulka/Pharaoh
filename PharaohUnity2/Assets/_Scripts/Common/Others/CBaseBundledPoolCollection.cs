// // =========================================
// // AUTHOR: Radek Mikulka
// // DATE:   06.09.2023
// // =========================================

using System;
using System.Collections.Generic;
using AldaEngine;
using ServerData;
using UnityEngine;

namespace Pharaoh
{
    public class CBaseBundledPoolCollection<TEnum, TType> : CBundledPoolsCollection<TEnum, TType> 
        where TEnum : Enum 
        where TType : Component
    {
        public override IEnumerable<IBundleLink> GetBundleLinks()
        {
            AssignBaseBundle();
            return base.GetBundleLinks();
        }
        
        private void AssignBaseBundle()
        {
            foreach (var bundleLink in _assetsDb)
            {
                bundleLink.BundleLink.SetBundleId((int) EBundleId.BaseGame);
            }
        }
    }
}