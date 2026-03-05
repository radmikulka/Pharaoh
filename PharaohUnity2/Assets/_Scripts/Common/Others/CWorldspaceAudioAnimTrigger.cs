using UnityEngine;

public class CWorldspaceAudioAnimTrigger : MonoBehaviour
{
	[SerializeField] private AudioSource[] _audioSources;
	private float _timeOfPreviousCall = -1;

	public void anim_PlaySound()
	{
		if (_audioSources.Length < 1)
			return;

		if (Time.realtimeSinceStartup - _timeOfPreviousCall < 0.1f)
			return;
		
		_timeOfPreviousCall = Time.realtimeSinceStartup;
		
		int randomIndex = UnityEngine.Random.Range(0, _audioSources.Length);
		float volume = UnityEngine.Random.Range(0.8f, 1f);

		_audioSources[randomIndex].PlayOneShot(_audioSources[randomIndex].clip, volume);
	}
}