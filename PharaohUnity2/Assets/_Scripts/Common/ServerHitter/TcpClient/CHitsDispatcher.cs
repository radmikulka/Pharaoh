// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.12.2023
// =========================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using AldaEngine.Tcp;
using Newtonsoft.Json;
using ServerData;
using ServerData.Hits;
using ServiceEngine;
using TycoonBuilder.Signal;
using UnityEngine;
using Zenject;
using ILogger = AldaEngine.ILogger;

namespace TycoonBuilder
{
	public class CHitsDispatcher : MonoBehaviour, IConstructable
	{
		private const int DurationBetweenHitsInSecs = 6;

		private readonly Dictionary<string, object> _tempAnalyticsData = new();

		private CCommunicationTokenProvider _communicationTokenProvider;
		private CancellationTokenSource _cancellationTokenSource;
		private CTcpClientFactory _tcpClientFactory;
		private CErrorHandler _errorHandler;
		private ICrashlytics _crashlytics;
		private CHitInfoHeader _hitHeader;
		private ICtsProvider _ctsProvider;
		private IActiveAuth _activeAuth;
		private ITcpClient _tcpClient;
		private CHitsQueue _hitsQueue;
		private IAnalytics _analytics;
		private IEventBus _eventBus;
		private ILogger _logger;

		private DateTime _lastHitEnqueueTime;
		private DateTime _lastHitSendTime;
		private DateTime _nextHitSendTime;
		private int _processingPacketsCount;
		private int _sessionId;
		
		private readonly ConcurrentQueue<Action> _mainThreadActions = new();

		[Inject]
		private void Inject(
			CCommunicationTokenProvider communicationTokenProvider, 
			CTcpClientFactory tcpClientFactory, 
			CErrorHandler errorHandler,
			ICtsProvider ctsProvider, 
			ICrashlytics crashlytics, 
			IActiveAuth activeAuth, 
			IServerTime serverTime, 
			IAnalytics analytics, 
			IEventBus eventBus, 
			ILogger logger
			)
		{
			_communicationTokenProvider = communicationTokenProvider;
			_tcpClientFactory = tcpClientFactory;
			_hitsQueue = new CHitsQueue();
			_errorHandler = errorHandler;
			_crashlytics = crashlytics;
			_ctsProvider = ctsProvider;
			_activeAuth = activeAuth;
			_analytics = analytics;
			_eventBus = eventBus;
			_logger = logger;
		}

		public void Construct()
		{
			Reset();
			RunUpdateLoop();
			_hitHeader = GetHeader();
		}

		public void CreateNewTcpClient(CServerTcpEndPoint endPoint)
		{
			_tcpClient = _tcpClientFactory.CreateTcpClient(endPoint);
		}

		public void Enqueue(CHitRecord hit)
		{
			LogOutputHits(hit);
			
			// suppress hit must be send alone, so we force dispatch previous hits
			if (hit.SuppressCommunicationErrorThrow)
			{
				ForceDispatch();
			}
			
			_hitsQueue.Enqueue(hit);
			_lastHitEnqueueTime = DateTime.UtcNow;
			
			if (hit.ExecuteImmediately)
			{
				ForceDispatch();
			}
		}

		private void LogOutputHits(CHitRecord hitRecord)
		{
			_tempAnalyticsData.Clear();
			_tempAnalyticsData.Add("HitName", hitRecord.Hit.GetType().Name);
			_analytics.SendData("OutputHit", _tempAnalyticsData);
			_crashlytics.Log($"OutputHit {(EHit)hitRecord.Hit.HitId}");
		}
		
		private void LogServerOutput(CHitRecordsGroup recordsGroup)
		{
			if (!CPlatform.IsDebug)
				return;

			for (int i = 0; i < recordsGroup.Records.Length; i++)
			{
				string json = JsonConvert.SerializeObject(recordsGroup.Records[i].Hit, Formatting.Indented);
				Debug.Log($"SERVER OUT {(EHit)recordsGroup.Records[i].Hit.HitId} {Environment.NewLine}{json}");
			}
		}

		private void LogInputHits(CResponsePacket response)
		{
			if(response == null)
				return;

			foreach (IHit responseHit in response.Hits)
			{
				_tempAnalyticsData.Clear();
				_tempAnalyticsData.Add("HitName", responseHit.GetType().Name);
				_analytics.SendData("InputHit", _tempAnalyticsData);
				_crashlytics.Log($"InputHit {(EHit)responseHit.HitId}");
			}
			
			if (!CPlatform.IsDebug)
				return;

			for (int i = 0; i < response.Hits.Length; i++)
			{
				string json = JsonConvert.SerializeObject(response.Hits[i], Formatting.Indented);
				Debug.Log($"SERVER OUT {(EHit)response.Hits[i].HitId} {Environment.NewLine}{json}");
			}
		}

		public bool IsProcessingAnyData()
		{
			if(_processingPacketsCount > 0)
				return true;
			
			return !_hitsQueue.IsEmpty;
		}

		private void OnApplicationPause(bool pauseStatus)
		{
			if(CPlatform.IsEditor)
				return;
			
			if (!pauseStatus)
				return;
			
			ForceDispatch();
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			if(hasFocus)
				return;
			
			ForceDispatch();
		}

		private void OnApplicationQuit()
		{
			if (!CPlatform.IsEditor)
				return;
			
			ForceDispatch();
		}

		private void RunUpdateLoop()
		{
			Task.Factory.StartNew(async () =>
			{
				while (true)
				{
					try
					{
						await ThreadedUpdate().ConfigureAwait(false);	
					}
					catch (Exception e)
					{
						_logger.LogError(e);
					}
					await Task.Delay(TimeSpan.FromSeconds(0.1f)).ConfigureAwait(false);
				}
				// ReSharper disable once FunctionNeverReturns
			}, TaskCreationOptions.LongRunning);
		}

		private void Update()
		{
			ExecuteMainThreadActions();
		}
		
		private void ExecuteMainThreadActions()
		{
			while (_mainThreadActions.TryDequeue(out Action action))
			{
				try
				{
					action.Invoke();
				}
				catch (Exception e)
				{
					CUnityReadableException unityError = new CUnityReadableException(e);
					_logger.LogError(unityError);
				}
			}
		}

		private async Task ThreadedUpdate()
		{
			await SendNextHitIfIsTooOld().ConfigureAwait(false);
			
			DateTime now = DateTime.UtcNow;
			if (_nextHitSendTime > now)
				return;
			
			if(_lastHitEnqueueTime.AddSeconds(2) > now)
				return;
				
			await TrySendNextHits().ConfigureAwait(false);
		}
		
		private async Task SendNextHitIfIsTooOld()
		{
			DateTime now = DateTime.UtcNow;
			DateTime? oldestRecordTime = _hitsQueue.GetOldestRecordCreationTime();
			if (!oldestRecordTime.HasValue)
				return;
			
			TimeSpan age = now - oldestRecordTime.Value;
			if (age.TotalSeconds > 10)
			{
				await TrySendNextHits().ConfigureAwait(false);
			}
		}

		private void ForceDispatch()
		{
			CHitRecord[] hits = _hitsQueue.GetNextRecordsGroupOrDefault();
			if(hits == null)
				return;

			Task.Run(async () =>
			{
				await SendNext(hits).ConfigureAwait(false);
			});
		}

		private async Task TrySendNextHits()
		{
			CHitRecord[] hits = _hitsQueue.GetNextRecordsGroupOrDefault();
			if(hits == null)
				return;

			await SendNext(hits).ConfigureAwait(false);
		}

		private async Task SendNext(CHitRecord[] hits)
		{
			bool suppressErrorThrow = ContainsSuppressCommunicationErrorThrow(hits);
			CHitRecordsGroup recordsGroup = new(_hitHeader, hits, suppressErrorThrow);
			int sessionId = _sessionId;
			try
			{
				Interlocked.Increment(ref _processingPacketsCount);
				CancellationToken ct = _ctsProvider.Token;
				
				recordsGroup = TryInsertRefreshTokenHit(recordsGroup);

				_lastHitSendTime = DateTime.UtcNow;
				_nextHitSendTime = _lastHitSendTime.AddSeconds(DurationBetweenHitsInSecs);
				
				_mainThreadActions.Enqueue(() => LogServerOutput(recordsGroup));

				CResponsePacket response = (CResponsePacket) await _tcpClient.SendAsync(recordsGroup.GetPackedData()).ConfigureAwait(false);
				ct.ThrowIfCancellationRequested();
				
				_mainThreadActions.Enqueue(() => LogInputHits(response));

				bool containsError = ContainsAnyError(response);
				if (!containsError || suppressErrorThrow)
				{
					ProcessSuccessResponse(recordsGroup, response);
					await TrySendNextHits().ConfigureAwait(false);
				}
				else
				{
					_mainThreadActions.Enqueue(() => recordsGroup.FailAll(EErrorCode.Internal));
					ProcessErrorResponse(response);
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e);
				
				_mainThreadActions.Enqueue(() =>
				{
					recordsGroup.FailAll(EErrorCode.Internal);
				});

				if (!suppressErrorThrow)
				{
					_mainThreadActions.Enqueue(() =>
					{
						_errorHandler.HandleInternalError(EErrorCode.Internal);
					});
					
					Reset();
				}
			}
			finally
			{
				if (sessionId == _sessionId)
				{
					Interlocked.Decrement(ref _processingPacketsCount);
				}
			}
			
			bool isProcessing = IsProcessingAnyData();
			if (!isProcessing)
			{
				_eventBus.Send(new CAllServerHitsProcessedSignal());
			}
			
			return;
			
			bool ContainsAnyError(CResponsePacket response)
			{
				if (response == null)
					return true;
				return response.Hits.Any(hit => hit.HitId < (int)EHit.ConnectRequest);
			}
		}
		
		private void ProcessErrorResponse(CResponsePacket response)
		{
			Reset();

			_eventBus.ProcessTask<CStopUserValidatorTask>();
			
			_mainThreadActions.Enqueue(() =>
			{
				foreach (var responseHit in response.Hits)
				{
					_errorHandler.HandleErrorResponse((CResponseHit)responseHit);
				}
			});
		}
		
		private void ProcessSuccessResponse(CHitRecordsGroup recordsGroup, CResponsePacket response)
		{
			_mainThreadActions.Enqueue(() =>
			{
				for (int i = 0; i < response.Hits.Length; i++)
				{
					IHit responseHit = response.Hits[i];
					recordsGroup.Records[i].OnSuccess?.Invoke((CResponseHit) responseHit);
				}
				
				_eventBus.Send(new CServerHitsProcessedSignal(response.Hits));
			});
		}

		private void Reset()
		{
			++_sessionId;
			
			_crashlytics.Log("Hit dispatcher reset");
			_hitsQueue.Clear();
			_mainThreadActions.Clear();
			_processingPacketsCount = 0;
			_cancellationTokenSource?.Cancel();
			_cancellationTokenSource = new CancellationTokenSource();
		}

		private bool ContainsSuppressCommunicationErrorThrow(CHitRecord[] hits)
		{
			bool result = hits.Any(hit => hit.SuppressCommunicationErrorThrow);
			if (result && hits.Length > 1)
			{
				throw new Exception("Cannot send multiple hits with SuppressCommunicationErrorThrow flag");
			}

			return result;
		}

		private CHitRecordsGroup TryInsertRefreshTokenHit(CHitRecordsGroup recordsGroup)
		{
			bool insertRefreshTokenHit = ShouldInsertRefreshTokenHit(recordsGroup.Records);
			if(!insertRefreshTokenHit)
				return recordsGroup;
			
			CHitRecord refreshTokenRecord = GetRefreshTokenRecord();
			
			List<CHitRecord> newRecords = new List<CHitRecord>
			{
				refreshTokenRecord
			};
			newRecords.AddRange(recordsGroup.Records);
			
			return new CHitRecordsGroup(
				recordsGroup.Header, 
				newRecords.ToArray(),
				recordsGroup.SuppressAutomaticErrorHandling);
		}

		private CHitInfoHeader GetHeader()
		{
			CHitInfoHeader result = new()
			{
				GameVersion = CConfigVersion.Instance.Version,
				Platform = CPlatform.Platform
			};
			return result;
		}
		
		private bool ShouldInsertRefreshTokenHit(CHitRecord[] records)
		{
			int timeoutInMinutes = CMath.Max(0, CUserCacheTimoutConfig.TimeoutTimeInMinutes - 3);
			bool result = _lastHitSendTime.AddMinutes(timeoutInMinutes) < DateTime.UtcNow;

			if (!result)
			{
				return false;
			}

			foreach (CHitRecord record in records)
			{
				result &= record.Hit is CCommTokenBasedRequest;
			}

			return result;
		}

		private CHitRecord GetRefreshTokenRecord()
		{
			return new CHitRecord(
				false, 
				null, 
				false, 
				false, 
				new CRefreshTokenRequest(
					_activeAuth.AuthUid,
					_communicationTokenProvider.CommunicationToken), 
				null);
		}
	}
}