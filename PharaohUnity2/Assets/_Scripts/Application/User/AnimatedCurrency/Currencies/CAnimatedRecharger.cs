// =========================================
// AUTHOR: Radek Mikulka
// DATE:   11.09.2025
// =========================================

using System;
using ServerData;

namespace TycoonBuilder
{
	public class CAnimatedRecharger : IAnimatedCurrency
	{
		private readonly CObservableRecharger _recharger;

		public event Action<SValueChangeArgs> ValueChanged;

		public int Value => _recharger.CurrentAmount;
		public void Tick(long timestamp)
		{
			_recharger.Update(timestamp);
		}

		public CAnimatedRecharger(CObservableRecharger recharger)
		{
			_recharger = recharger;
			_recharger.ValueChanged += OnRechargerChanged;
		}

		private void OnRechargerChanged(SValueChangeArgs args)
		{
			ValueChanged?.Invoke(args);
		}
	}
}