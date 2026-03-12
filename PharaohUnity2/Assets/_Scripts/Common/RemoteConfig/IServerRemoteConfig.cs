using ServerData.Dto;

namespace Pharaoh
{
	public interface IServerRemoteConfig
	{
		void Initialize(CRemoteConfigDto dto);
		string GetString(string key, string defaultValue = "");
		int GetInt(string key, int defaultValue = 0);
		bool GetBool(string key, bool defaultValue = false);
		float GetFloat(string key, float defaultValue = 0f);
		bool HasKey(string key);
	}
}