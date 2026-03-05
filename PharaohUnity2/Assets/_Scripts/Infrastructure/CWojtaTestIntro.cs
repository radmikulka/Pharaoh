using System.Collections;
using NaughtyAttributes;
using UnityEngine;

// We can later delete this.
// I just needed a way to test the animations in order

[RequireComponent(typeof(Animation))]
public class CWojtaTestIntro : MonoBehaviour
{
	[Header("Animations")]
	[SerializeField] private AnimationClip[] _clips;
	
	[Header("Contracts")]
	[SerializeField] private GameObject[] _contracts;
	
	private Animation _animation;
	
	private void OnEnable()
	{
		_animation = GetComponent<Animation>();

		SetContractsActive(false);
		StartCoroutine(PlayAllAnimationsInOrder());
	}

	private IEnumerator PlayAllAnimationsInOrder()
	{
		for (int i = 0; i < _clips.Length; i++)
		{
			_animation.clip = _clips[i];
			_animation.Stop();
			_animation.Rewind();
			_animation.Play();
			
			yield return new WaitForSeconds(_clips[i].length);
		}
		
		SetContractsActive(true);
		this.gameObject.SetActive(false);
	}

	private void SetContractsActive(bool active)
	{
		foreach (GameObject contract in _contracts)
		{
			contract.SetActive(active);
		}
	}

	public void SkipIntro()
	{
		StopAllCoroutines();

		SetContractsActive(true);
		this.gameObject.SetActive(false);
	}

	[Button("Calculate Cutscene Length")]
	private void CalculateCutsceneLength()
	{
		float length = 0f;

		foreach (AnimationClip clip in _clips)
		{
			length += clip.length;
		}

		Debug.Log($"Cutscene Length: {length}");
	}
}
