// =========================================
// AUTHOR: Juraj Joscak
// DATE:   24.09.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using Server;
using ServerData;
using TycoonBuilder.Infrastructure;
using TycoonBuilder.Offers;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CTrayIconBar : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField] private float _spacing = 10f;
		[SerializeField] private CTrayIcon _iconTemplate;
		
		private CUser _user;
		private IServerTime _serverTime;
		private CAldaInstantiator _aldaInstantiator;
		private DiContainer _diContainer;
		private IEventBus _eventBus;
		
		private readonly List<CTrayIcon> _icons = new();
		private Vector3 _stride;
		private int _maxIconsToShow;
		
		[Inject]
		private void Inject(CUser user, IServerTime serverTime, CAldaInstantiator aldaInstantiator, DiContainer container, IEventBus eventBus)
		{
			_user = user;
			_serverTime = serverTime;
			_aldaInstantiator = aldaInstantiator;
			_diContainer = container;
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			_iconTemplate.gameObject.SetActiveObject(false);
			_stride = new Vector3(0, -(_spacing + _iconTemplate.RectTransform.rect.height), 0);
			_maxIconsToShow = Mathf.FloorToInt(((RectTransform)transform).rect.height / -_stride.y);
			
			_eventBus.Subscribe<COfferClaimedSignal>(Repaint);
			_eventBus.AddTaskHandler<CGetTrayIconRequest, CGetTrayIconResponse>(HandleGetTrayIcon);
			_eventBus.Subscribe<COfferExpiredSignal>(Repaint);
			_eventBus.Subscribe<COfferSeenSignal>(Repaint);
			_eventBus.Subscribe<CVehicleAddedSignal>(Repaint);
			_eventBus.Subscribe<CNewOffersSyncedSignal>(Repaint);
			
			Repaint();
		}
		
		private void Repaint()
		{
			ITrayIconData[] iconData = GetTrayIconData();
			for(int i = 0; i < iconData.Length; i++)
			{
				if (_icons.Count <= i)
				{
					SpawnNew();
				}
				
				_icons[i].Set(iconData[i]);
				_icons[i].SetPosition(i*_stride + _stride/2);
			}
			
			for(int i = iconData.Length; i < _icons.Count; i++)
			{
				_icons[i].Hide();
			}
		}

		private ITrayIconData[] GetTrayIconData()
		{
			ITrayIconData[] groups = _user.Offers.GetGroupsWithParam(EOfferParam.TrayIcon)
					.Where(IsGroupValid)
					.Select(group => new CGroupTrayIconData(group) as ITrayIconData)
					.ToArray()
				;
			
			IEnumerable<ITrayIconData> offers = _user.Offers.GetOffersWithParam(EOfferParam.TrayIcon)
					.Where(IsOfferValid)
					.Where(OfferNotInGroup)
					.Select(offer => new CStandaloneTrayIconData(offer))
				;
			
			ITrayIconData[] iconData = groups.Concat(offers)
				.OrderByDescending(IconPriority)
				.Take(_maxIconsToShow)
				.ToArray();

			return iconData;
			
			bool OfferNotInGroup(COffer offer)
			{
				string groupId = offer.GetParamValueOrDefault<string>(EOfferParam.GroupId);
				if (string.IsNullOrEmpty(groupId))
					return true;

				return groups.All(group => ((CGroupTrayIconData)group).Group.GroupId != groupId);
			}
		}
		
		private void SpawnNew()
		{
			CTrayIcon icon = _aldaInstantiator.Instantiate(_iconTemplate, transform, _diContainer);
			_icons.Add(icon);
		}
		
		private bool IsOfferValid(COffer offer)
		{
			if (!_user.Offers.IsOfferValid(offer))
				return false;

			if (offer.MaxPurchasesReached())
				return false;
			
			return true;
		}

		private bool IsGroupValid(COfferGroup group)
		{
			return _user.Offers.IsGroupValid(group.GroupId);
		}
		
		private int IconPriority(ITrayIconData offer)
		{
			return offer.GetParamValueOrDefault<int>(EOfferParam.TrayIconPriority);
		}

		private void Repaint(IEventBusSignal _)
		{
			Repaint();
		}
		
		private CGetTrayIconResponse HandleGetTrayIcon(CGetTrayIconRequest request)
		{
			CTrayIcon icon = _icons.FirstOrDefault(icon => icon.HasGuid(request.OfferId));
			return new CGetTrayIconResponse(icon);
		}
	}

	public interface ITrayIconData
	{
		T GetParamValueOrDefault<T>(EOfferParam trayIconPriority);
		IOfferParam[] Params { get; }
	}

	public class CStandaloneTrayIconData : ITrayIconData
	{
		public COffer Offer { get; }
		public IOfferParam[] Params => Offer.Params;

		public CStandaloneTrayIconData(COffer offer)
		{
			Offer = offer;
		}

		public T GetParamValueOrDefault<T>(EOfferParam paramId) => Offer.GetParamValueOrDefault<T>(paramId);
	}
	
	public class CGroupTrayIconData : ITrayIconData
	{
		public COfferGroup Group { get; }
		public IOfferParam[] Params => Group.Params;
		public CGroupTrayIconData(COfferGroup group)
		{
			Group = group;
		}
		
		public T GetParamValueOrDefault<T>(EOfferParam paramId) => Group.GetParamValueOrDefault<T>(paramId);
	}
}