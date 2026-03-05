// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using System;
using AldaEngine;

namespace TycoonBuilder
{
	public abstract class CData<T> : IUserData<T>
	{
		public T ServerValue { get; internal set; }

		public T LocalValue
		{
			get => _localValue;
			internal set
			{
				_localValue = value;
				OnValueChanged?.Invoke(value);
			}
		}

		private T _localValue;
		
		public event Action<T> OnValueChanged;

		protected CData(T defaultVal)
		{
			LocalValue = defaultVal;
			ServerValue = defaultVal;
		}

		public abstract bool IsConsistent();

		public override string ToString()
		{
			return $"L: {LocalValue}, R: {ServerValue}";
		}
	}
}