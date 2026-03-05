// =========================================
// AUTHOR: Marek Karaba
// DATE:   25.07.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using ServerData.Design;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CDecadePassHudButton : MonoBehaviour, IInitializable, IParticleTargetPart
	{
		[SerializeField] private Slider _xpSlider;
		[SerializeField] private CYearVisualSetter _yearVisualSetter;
		[SerializeField] private CDecadePassHudButtonClaimVisual _claimVisual;
		
		private IEventBus _eventBus;
		private CUser _user;
		
		private bool _syncEnabled = true;
		private int _currentXp;
		private EYearMilestone _currentYear;
		
		[Inject]
		private void Inject(IEventBus eventBus, CUser user)
		{
			_eventBus = eventBus;
			_user = user;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CYearIncreasedSignal>(OnYearIncreased);
			_eventBus.Subscribe<CXpIncreasedSignal>(OnXpIncreased);
			_eventBus.Subscribe<CDecadePassRewardClaimAnimatedSignal>(OnRewardClaimAnimated);
			_eventBus.Subscribe<CDecadePassActivatedSignal>(OnDecadePassActivated);
			
			RepaintCurrentYear();
		}

		private void RepaintCurrentYear()
		{
			_currentYear = _user.Progress.Year;
			int currentXp = _user.DecadePass.GetCurrentXpInCurrentTier();
			
			_yearVisualSetter.SetYear(_currentYear);
			SetProgress(currentXp);
			UpdateState();
		}

		private void OnYearIncreased(CYearIncreasedSignal signal)
		{
			if(!_syncEnabled)
				return;
			
			_yearVisualSetter.SetYear(signal.YearMilestone);
		}

		private void OnXpIncreased(CXpIncreasedSignal signal)
		{
			if(!_syncEnabled)
				return;
			
			int currentXp = _user.DecadePass.GetCurrentXpInCurrentTier();
			SetProgress(currentXp);
			UpdateState();
		}

		private void OnRewardClaimAnimated(CDecadePassRewardClaimAnimatedSignal signal)
		{
			if(!_syncEnabled)
				return;
			
			UpdateState();
		}

		private void OnDecadePassActivated(CDecadePassActivatedSignal signal)
		{
			if(!_syncEnabled)
				return;
			
			UpdateState();
		}

		private void SetProgress(int currentXpInTier)
		{
			_currentXp = currentXpInTier;
			
			EYearMilestone currentYear = _user.Progress.Year;
			int ownedXp = _user.Progress.XpInCurrentYear;
			int maxClaimableIndex = _user.DecadePass.GetMaxClaimableIndex(currentYear, ownedXp);
			int rewardsCount = _user.DecadePass.GetMaxRewardIndex();
			int maxXp = _user.DecadePass.GetMaxXpInCurrentTier(currentYear);
			
			if (maxClaimableIndex == rewardsCount)
			{
				currentXpInTier = maxXp;
			}
			
			float progress = (float)currentXpInTier / maxXp;
			_xpSlider.value = progress;

			if (progress >= 1f)
			{
				UpdateState();
			}
		}

		private void UpdateState()
		{
			EYearMilestone currentYear = _user.Progress.Year;
			int ownedXp = _user.Progress.XpInCurrentYear;
			bool allClaimed  = _user.DecadePass.AreAllRewardsClaimed(currentYear, ownedXp);
			ToggleClaimedVisual(allClaimed);
		}
		
		private void ToggleClaimedVisual(bool allClaimed)
		{
			_claimVisual.SetActive(!allClaimed);
		}

		public void DisableSync()
		{
			_syncEnabled = false;
		}

		public void EnableSync()
		{
			_syncEnabled = true;
			RepaintCurrentYear();
		}

		public void ParticleStepFinished(IValuable diff)
		{
			CXpValuable xpDiff = (CXpValuable)diff;
			SetProgress(_currentXp + xpDiff.Amount);
		}
	}
}