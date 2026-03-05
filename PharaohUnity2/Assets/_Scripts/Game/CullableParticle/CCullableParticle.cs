// =========================================
// AUTHOR: Radek Mikulka
// DATE:   17.12.2025
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using NaughtyAttributes;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CCullableParticle : MonoBehaviour, IIHaveCullingGroup, IInitializable
	{
		[SerializeField, Required] private CParticleSystem _particleSystem;
		[SerializeField] private float _radius = 5f;

		private CCullingGroupApi _cullingGroupApi;
		private Transform _transform;

		public Vector3 Position => _transform.position;
		public float Radius => _radius;
		public bool UpdatePosition => true;

		[Inject]
		private void Inject(CCullingGroupApi cullingGroupApi)
		{
			_cullingGroupApi = cullingGroupApi;
		}

		public void Initialize()
		{
			_transform = transform;
			TryEnable();
		}

		private void OnEnable()
		{
			TryEnable();
		}

		private void OnDisable()
		{
			_cullingGroupApi.UnregisterCullingGroup(this);
		}

		private void TryEnable()
		{
			if(_cullingGroupApi == null)
				return;

			_cullingGroupApi.RegisterCullingGroup(this);
			SampleInitialState();
		}

		private void SampleInitialState()
		{
			bool isVisible = _cullingGroupApi.IsVisible(this);
			OnStateChange(isVisible);
		}

		public void OnStateChange(bool isVisible)
		{
			if (isVisible)
			{
				_particleSystem.Play();
				return;
			}
			_particleSystem.Pause();
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow.WithAlpha(0.3f);
			Gizmos.DrawSphere(transform.position, _radius);
		}
	}
}