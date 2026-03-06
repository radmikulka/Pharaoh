// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Numerics;
using AldaEngine;
using ServerData;
using Pharaoh;

namespace Pharaoh
{
    public class CAnimatedCurrency
    {
        private readonly HashSet<CLockObject> _animationLocks = new();
        private readonly IAnimatedCurrency _currency;
        private bool BidingLocked => _animationLocks.Count > 0;
        public int Value { get; private set; }
        public event Action<int> ValueChanged;

        public CAnimatedCurrency(IAnimatedCurrency currency)
        {
            _currency = currency;
            _currency.ValueChanged += OnValueChanged;
            Value = _currency.Value;
        }

        public void AddBidingLock(CLockObject lockObject)
        {
            _animationLocks.Add(lockObject);
        }

        public void RemoveBidingLock(CLockObject lockObject)
        {
            _animationLocks.Remove(lockObject);
            TryRecalculate();
        }

        public void Tick(long timestamp)
        {
            _currency.Tick(timestamp);
        }

        private void OnValueChanged(SValueChangeArgs args)
        {
            if (!BidingLocked)
            {
                Recalculate();
                return;
            }

            bool isNonAnimatedChange = args.ModifyParams?.GetParamOrDefault<CAnimatedChangeParam>() == null;
            if (isNonAnimatedChange)
            {
                AddValue(args.Difference);
            }
        }

        public void AddValue(int value)
        {
            Value += value;
            ValueChanged?.Invoke(Value);
        }

        public void Dispose()
        {
            ValueChanged = null;
        }
        
        private void TryRecalculate()
        {
            if(BidingLocked)
                return;
            Recalculate();
        }
        
        private void Recalculate()
        {
            int previousValue = Value;
            Value = _currency.Value;
            
            if (previousValue != Value)
            {
                ValueChanged?.Invoke(Value);
            }
        }
    }
}