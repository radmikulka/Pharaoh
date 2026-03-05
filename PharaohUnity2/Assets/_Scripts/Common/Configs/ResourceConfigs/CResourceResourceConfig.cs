// =========================================
// AUTHOR: Marek Karaba
// DATE:   23.07.2025
// =========================================

using System;
using System.Collections.Generic;
using AldaEngine;
using AldaEngine.UnityObjectPool;
using RoboRyanTron.SearchableEnum;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	[CreateAssetMenu(menuName = "____TycoonBuilder/Configs/Resource")]
	public class CResourceResourceConfig : ScriptableObject, IResourceConfigBase<EResource>, IIHaveBundleLinks
	{
		[Serializable]
		private class CResourceModel
		{
			public EMovementType MovementType;
			[BundleLink(false, typeof(CPooledObject))] public CBundleLink Model;
		}
		
		[SerializeField, SearchableEnum] private EResource _id;
		[SerializeField] [BundleLink(false, typeof(Sprite))] private CBundleLink _sprite;
		[SerializeField] private CResourceModel[] _inGameModels;
		[SerializeField, BundleLink(false, typeof(CPooledObject))] private CBundleLink _depoModel;

		public EResource Id => _id;
		public CBundleLink Sprite => _sprite;
		public CBundleLink DepoModel => _depoModel;

		public IEnumerable<IBundleLink> GetBundleLinks()
		{
			_sprite.SetBundleId((int) EBundleId.BaseGame);
			_depoModel.SetBundleId((int) EBundleId.BaseGame);

			foreach (CResourceModel inGameModel in _inGameModels)
			{
				inGameModel.Model.SetBundleId((int) EBundleId.BaseGame);
				yield return inGameModel.Model;
			}
			
			yield return _sprite;
			yield return _depoModel;
		}
		
		public CBundleLink GetInGameModel(EMovementType movementType)
		{
			foreach (CResourceModel inGameModel in _inGameModels)
			{
				if (inGameModel.MovementType == movementType)
				{
					return inGameModel.Model;
				}
			}

			throw new Exception($"No in-game model found for resource {_id} with movement type {movementType}");
		}
	}
}