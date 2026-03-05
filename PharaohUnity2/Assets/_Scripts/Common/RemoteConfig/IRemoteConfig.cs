// =========================================
// AUTHOR: Radek Mikulka
// DATE:   21.12.2023
// =========================================

using AldaEngine;
using Cysharp.Threading.Tasks;
using ServiceEngine;

namespace TycoonBuilder
{
	public interface IRemoteConfig
	{
		UniTask TryFetchAsync(float timeoutInSeconds);
		UniTask TryActivateFetchedDataAsync();
		SRemoteConfigValue GetValue(ERemoteConfigKey key);
	}
}