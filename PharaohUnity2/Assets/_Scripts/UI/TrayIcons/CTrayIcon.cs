// =========================================
// AUTHOR: Juraj Joscak
// DATE:   24.09.2025
// =========================================

using System;
using System.Linq;
using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CTrayIcon : ValidatedMonoBehaviour, IConstructable, IInitializable
	{
		[SerializeField, Self] private CUiButton _button;
		[SerializeField, Child] private CRemoteImage _image;
		[SerializeField] private RectTransform _markerHolder;
		
		private IEventBus _eventBus;
		private ICtsProvider _ctsProvider;
		private CUiMarkerProvider _markerProvider;
		private CUser _user;
		private IServerTime _serverTime;
		
		public RectTransform RectTransform { get; private set; }
		
		private ITrayIconData _data;
		private CShopOfferParamHandler[] _paramHandlers;
		private CancellationTokenSource _markerRepaintCts;
		private long _currentReminderStartTime;
		
		[Inject]
		private void Inject(IEventBus eventBus, ICtsProvider ctsProvider, CUiMarkerProvider markerProvider, CUser user, IServerTime serverTime)
		{
			_eventBus = eventBus;
			_ctsProvider = ctsProvider;
			_markerProvider = markerProvider;
			_user = user;
			_serverTime = serverTime;
		}
		
		public void Construct()
		{
			RectTransform = (RectTransform)transform;
			_paramHandlers = GetComponentsInChildren<CShopOfferParamHandler>(true);
		}

		public void Initialize()
		{
			_button.AddClickListener(OnClick);
		}
		
		public void Set(ITrayIconData data)
		{
			gameObject.SetActiveObject(true);
			_data = data;

			SetImage();
			
			foreach (CShopOfferParamHandler handler in _paramHandlers)
			{
				handler.Set(_data.Params);
			}
			
			SetMarker();
		}
		
		public bool HasGuid(string guid)
		{
			if(_data == null)
				return false;
			
			return _data switch
			{
				CStandaloneTrayIconData standalone => standalone.Offer.Guid == guid,
				CGroupTrayIconData groupIconData => _user.Offers.GetOffersByParam(EOfferParam.GroupId, groupIconData.Group.GroupId).Any(offer => offer.Guid == guid),
				_ => throw new Exception("Unknown tray icon data type.")
			};
		}

		private void SetMarker()
		{
			_markerRepaintCts?.Cancel();
			bool seen;
			bool reminderActive;
			switch (_data)
			{
				case CStandaloneTrayIconData standalone:
					seen = standalone.Offer.IsSeen;
					reminderActive = ReminderActive(standalone.Offer);
					break;
				case CGroupTrayIconData group:
					COffer[] offers = _user.Offers.GetOffersByParam(EOfferParam.GroupId, group.Group.GroupId); 
					seen = offers.All(offer => offer.IsSeen);
					reminderActive = offers.Any(ReminderActive);
					break;
				default:
					seen = true;
					reminderActive = false;
					break;
			}

			if (seen && !reminderActive)
			{
				_markerProvider.DisableMarker(_markerHolder);
			}
			else
			{
				_markerProvider.SetMarker(1, _markerHolder, EMarkerType.ExclamationMark);
			}

			bool ReminderActive(COffer offer)
			{
				long reminderTime = offer.GetParamValueOrDefault<long>(EOfferParam.ExpirationReminder);
				long expirationTime = offer.GetParamValueOrDefault<long>(EOfferParam.ExpirationTime);
				long reminderStartTime = expirationTime - reminderTime;
				
				if (reminderTime <= 0)
					return false;

				if (_currentReminderStartTime > reminderStartTime || _currentReminderStartTime == 0)
				{
					_currentReminderStartTime = reminderStartTime;
				}
				
				_markerRepaintCts?.Cancel();
				if (_currentReminderStartTime <= _serverTime.GetTimestampInMs())
					return true;
				
				_markerRepaintCts = _ctsProvider.GetNewLinkedCts();
				ScheduleMarkerRepaint(_currentReminderStartTime, _markerRepaintCts.Token).Forget();
				return false;
			}
		}
		
		private async UniTaskVoid ScheduleMarkerRepaint(long repaintTimeInMs, CancellationToken ct)
		{
			await UniTask.WaitUntil(() => _serverTime.GetTimestampInMs() >= repaintTimeInMs, cancellationToken: ct);
			SetMarker();
		}
		
		public void Hide()
		{
			gameObject.SetActiveObject(false);
			_data = null;
			_markerRepaintCts?.Cancel();
			
			foreach (CShopOfferParamHandler handler in _paramHandlers)
			{
				handler.Disable();
			}
		}
		
		public void SetPosition(Vector3 position)
		{
			RectTransform.localPosition = position;
		}

		private void SetImage()
		{
			string url = _data.GetParamValueOrDefault<string>(EOfferParam.TrayIcon);
			_image.SetUrl(url);
		}
		
		private void OnClick()
		{
			if (_data == null)
				return;

			switch (_data)
			{
				case CGroupTrayIconData group:
					_eventBus.ProcessTaskAsync(new CShowPopUpOfferTask(group.Group.GroupId, true), _ctsProvider.Token);
					break;
				case CStandaloneTrayIconData standalone:
					_eventBus.ProcessTaskAsync(new CShowPopUpOfferTask(standalone.Offer.Guid, false), _ctsProvider.Token);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(_data));
			}
		}
	}
}