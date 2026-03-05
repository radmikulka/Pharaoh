// =========================================
// AUTHOR: Juraj Joscak
// DATE:   29.07.2025
// =========================================

using System;
using AldaEngine;
using UnityEngine;

namespace TycoonBuilder
{
	public class CField<TType>
	{
		private TType _value;

		public TType Value
		{
			get => _value;
			set
			{
				_value = value;
				OnValueChanged?.Invoke(value);
			}
		}
		public event Action<TType> OnValueChanged;
		private CSaveHandlerBase<TType> _saver;

		public CField(CSaveHandlerBase<TType> saver)
		{
			_saver = saver;
			Value = saver.Load();
			OnValueChanged += Save;
		}
		
		public void ResetToDefault()
		{
			Value = _saver.DefaultValue;
		}

		public void LateSetDefault(TType defaultValue)
		{
			if (!_saver.DefaultValue.Equals(default(TType)))
				return;
			
			if(Value.Equals(default(TType)))
			{
				Value = defaultValue;
			}
				
			_saver = new CSaveHandlerBase<TType>(_saver.SaveId, defaultValue);
		}

		private void Save(TType value)
		{
			_saver.Save(value);
		}
	}
	
	public class CSaveHandlerBase<TType>
	{
		public readonly string SaveId;
		public TType DefaultValue { get; }

		public CSaveHandlerBase(string saveId, TType defaultValue)
		{
			DefaultValue = defaultValue;
			SaveId = saveId;
		}

		public void Save(TType value)
		{
			CPlayerPrefs.Set(SaveId, value);
			CPlayerPrefs.Save();
		}

		public TType Load()
		{
			return CPlayerPrefs.Get(SaveId, DefaultValue);
		}
	}
}