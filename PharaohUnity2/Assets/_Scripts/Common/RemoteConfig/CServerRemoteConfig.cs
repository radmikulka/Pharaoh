using System.Collections.Generic;
using System.Globalization;
using ServerData.Dto;

namespace Pharaoh
{
	public class CServerRemoteConfig : IServerRemoteConfig
	{
		private Dictionary<string, string> _values = new();

		public void Initialize(CRemoteConfigDto dto)
		{
			_values = dto?.Values ?? new Dictionary<string, string>();
		}

		public string GetString(string key, string defaultValue = "")
		{
			return _values.TryGetValue(key, out string value) ? value : defaultValue;
		}

		public int GetInt(string key, int defaultValue = 0)
		{
			if (_values.TryGetValue(key, out string value) && int.TryParse(value, out int result))
			{
				return result;
			}

			return defaultValue;
		}

		public bool GetBool(string key, bool defaultValue = false)
		{
			if (_values.TryGetValue(key, out string value) && bool.TryParse(value, out bool result))
			{
				return result;
			}

			return defaultValue;
		}

		public float GetFloat(string key, float defaultValue = 0f)
		{
			if (_values.TryGetValue(key, out string value) && float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
			{
				return result;
			}

			return defaultValue;
		}

		public bool HasKey(string key)
		{
			return _values.ContainsKey(key);
		}
	}
}