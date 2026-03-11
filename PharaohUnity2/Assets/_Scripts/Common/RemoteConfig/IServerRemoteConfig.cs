namespace Pharaoh
{
	public interface IServerRemoteConfig
	{
		string GetString(string key, string defaultValue = "");
		int GetInt(string key, int defaultValue = 0);
		bool GetBool(string key, bool defaultValue = false);
		float GetFloat(string key, float defaultValue = 0f);
		bool HasKey(string key);
	}
}