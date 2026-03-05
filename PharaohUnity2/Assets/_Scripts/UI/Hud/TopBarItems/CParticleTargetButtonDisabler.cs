// =========================================
// AUTHOR: Juraj Joscak
// DATE:   07.10.2025
// =========================================

using AldaEngine;
using KBCore.Refs;
using ServerData;
using UnityEngine;

namespace TycoonBuilder.Ui
{
	public class CParticleTargetButtonDisabler : ValidatedMonoBehaviour, IParticleTargetPart
	{
		[SerializeField, Self] private CUiButton _button;
		
		public void DisableSync()
		{
			_button.SetInteractable(false);
		}

		public void EnableSync()
		{
			_button.SetInteractable(true);
		}

		public void ParticleStepFinished(IValuable diff)
		{
			
		}
	}
}