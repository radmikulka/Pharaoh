// =========================================
// NAME: Marek Karaba
// DATE: 24.02.2026
// =========================================

using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using NaughtyAttributes;
using TycoonBuilder.Configs;
using TycoonBuilder.Offers;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CLeaderboardChangeShower : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField] private CUiComponentText _positionText;
		[SerializeField] private CUiComponentText _infoText;
		[SerializeField] private GameObject _climbingIcon;
		[SerializeField] private GameObject _fallingIcon;
		[SerializeField] private float _textChangeDelay = 0.6f;
		[SerializeField] private ParticleSystem _particleSystemClimbing;
		[SerializeField] private ParticleSystem _particleSystemFalling;
		
		[Header("Animation")]
		[SerializeField] private Animation _animation;
		[SerializeField] private AnimationClip _clipClimbing;
		[SerializeField] private AnimationClip _clipFalling;

		private CLazyActionQueue _lazyActionQueue;
		private CGameModeManager _gameModeManager;
		private CUiGlobalsConfig _globalsConfig;
		private IScreenManager _screenManager;
		private ICtsProvider _ctsProvider;
		private ITranslation _translation;
		private IServerTime _serverTime;
		private IEventBus _eventBus;
		private CUser _user;
		
		private long Delay => CTimeConst.Minute.InMilliseconds * 2;
		private CancellationTokenSource _cts;
		private long _lastShownTimestamp;
		private bool _waitingForShow;
		private int _newUserRank;
		private CStandardEventContent _standardEventContent;
		
		[Inject]
		private void Inject(
			CLazyActionQueue lazyActionQueue,
			CGameModeManager gameModeManager,
			CUiGlobalsConfig globalsConfig,
			IScreenManager screenManager,
			ICtsProvider ctsProvider,
			ITranslation translation,
			IServerTime serverTime,
			IEventBus eventBus,
			CUser user)
		{
			_lazyActionQueue = lazyActionQueue;
			_gameModeManager = gameModeManager;
			_globalsConfig = globalsConfig;
			_screenManager = screenManager;
			_ctsProvider = ctsProvider;
			_translation = translation;
			_serverTime = serverTime;
			_eventBus = eventBus;
			_user = user;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CLeaderboardComplementSyncedSignal>(OnLeaderboardSynced);
			_eventBus.Subscribe<CGameModeStartedSignal>(OnGameModeStarted);
		}

		public void SetContent(CStandardEventContent standardEventContent)
		{
			_standardEventContent = standardEventContent;
			
			_cts?.Cancel();
			_cts = CancellationTokenSource.CreateLinkedTokenSource(_ctsProvider.Token);
			TryToShowLastSeenRank(_cts.Token).Forget();
		}

		private async UniTask TryToShowLastSeenRank(CancellationToken ct)
		{
			int lastSeenRank = GetLastSeenRank();
			int currentRank = _standardEventContent.Leaderboard?.GetRankForUid(_user.Account.EncryptedUid) ?? 0;

			bool hasRankChanged = HasRankChanged(lastSeenRank, currentRank);
			if (!hasRankChanged)
				return;

			const float gameStartDelay = 1f;
			await UniTask.WaitForSeconds(gameStartDelay, cancellationToken: ct);
			await UniTask.WaitUntil(() => !_lazyActionQueue.IsProcessing && !_screenManager.IsActive, cancellationToken: ct);
			TryDeferOrPlayAnimation(lastSeenRank, currentRank);
		}

		private void OnGameModeStarted(CGameModeStartedSignal signal)
		{
			if (!_waitingForShow)
				return;

			int lastSeenRank = GetLastSeenRank();
			if (lastSeenRank == 0)
			{
				_standardEventContent?.SetLastSeenRank();
				return;
			}

			bool hasRankChanged = HasRankChanged(lastSeenRank, _newUserRank);
			if (!hasRankChanged)
				return;

			PlayAnimation(lastSeenRank, _newUserRank);
		}
		
		private int GetLastSeenRank()
		{
			return _standardEventContent?.LastSeenRank ?? 0;
		}

		private bool IsRankValid(int rank)
		{
			bool isRankValid = rank > 0;
			return isRankValid;
		}

		private bool HasRankChanged(int lastSeenRank, int newRank)
		{
			bool isLastSeenRankValid = IsRankValid(lastSeenRank);
			bool isNewRankValid = IsRankValid(newRank);
			bool rankChanged = lastSeenRank != newRank;
			return isLastSeenRankValid && isNewRankValid && rankChanged;
		}

		private void TryDeferOrPlayAnimation(int lastSeenRank, int newRank)
		{
			if (_gameModeManager.ActiveGameMode?.Id != EGameModeId.CoreGame)
			{
				_waitingForShow = true;
				_newUserRank = newRank;
				return;
			}

			PlayAnimation(lastSeenRank, newRank);
		}

		private void OnLeaderboardSynced(CLeaderboardComplementSyncedSignal signal)
		{
			if (_standardEventContent?.LeaderboardComplement?.LeaderboardUid != signal.LeaderboardUid)
				return;

			if (signal.LastSeenRank == 0)
			{
				_standardEventContent?.SetLastSeenRank();
				return;
			}

			bool hasRankChanged = HasRankChanged(signal.LastSeenRank, signal.NewUserRank);
			if (!hasRankChanged)
				return;

			long timeSinceLastShown = _serverTime.GetTimestampInMs() - _lastShownTimestamp;
			if (timeSinceLastShown < Delay)
				return;

			TryDeferOrPlayAnimation(signal.LastSeenRank, signal.NewUserRank);
		}

		private void PlayAnimation(int oldRank, int newRank)
		{
			_waitingForShow = false;
			_lastShownTimestamp = _serverTime.GetTimestampInMs();
			_standardEventContent?.SetLastSeenRank();
			Animate(oldRank, newRank, climbing: newRank < oldRank, _ctsProvider.Token).Forget();
		}

		private async UniTask Animate(int oldPosition, int newPosition, bool climbing, CancellationToken ct)
		{
			SetInfoText(oldPosition, newPosition);
			_positionText.SetValue(oldPosition);
			_animation.Stop();
			_animation.clip = climbing ? _clipClimbing : _clipFalling;
			_animation.Rewind();
			_animation.Play();
			
			await UniTask.WaitForSeconds(_textChangeDelay, cancellationToken: ct);
			
			_positionText.SetValue(newPosition);
			
			ParticleSystem particles = climbing ? _particleSystemClimbing : _particleSystemFalling;
			particles.Stop();
			particles.Play();
		}

		private void SetInfoText(int oldPosition, int newPosition)
		{
			bool isClimbing = newPosition < oldPosition;
			string text;
			Color color;
			if (isClimbing)
			{
				text = _translation.GetText("Generic.Climbing");
				color = _globalsConfig.GainCurrencyColor;
			}
			else
			{
				text = _translation.GetText("Generic.Falling");
				color = _globalsConfig.NotEnoughCurrencyColor;
			}
			
			_infoText.SetValue(text);
			_infoText.SetColor(color, true);
			_climbingIcon.SetActive(isClimbing);
			_fallingIcon.SetActive(!isClimbing);
		}
		
		[Button]
		// ReSharper disable once UnusedMember.Global
		public void TestAnimateClimbing()
		{
			int oldPosition = 5;
			int newPosition = 3;
			
			_cts?.Cancel();
			_cts = CancellationTokenSource.CreateLinkedTokenSource(_ctsProvider.Token);
			
			Animate(oldPosition, newPosition, climbing: true, _cts.Token).Forget();
		}

		[Button]
		// ReSharper disable once UnusedMember.Global
		public void TestAnimateFalling()
		{
			int oldPosition = 3;
			int newPosition = 5;
			
			_cts?.Cancel();
			_cts = CancellationTokenSource.CreateLinkedTokenSource(_ctsProvider.Token);
			
			Animate(oldPosition, newPosition, climbing: false, _cts.Token).Forget();
		}
	}
}