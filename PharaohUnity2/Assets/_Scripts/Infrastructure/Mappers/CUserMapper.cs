// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System;
using System.Linq;
using AldaEngine;
using Server;
using ServerData;
using ServerData.Design;
using ServerData.Dto;

namespace TycoonBuilder
{
	public class CUserMapper
	{
		private readonly CVehiclesFactory _vehiclesFactory;
		private readonly IMapper _mapper;

		public CUserMapper(
			CVehiclesFactory vehiclesFactory,
			IMapper mapper
			)
		{
			_vehiclesFactory = vehiclesFactory;
			_mapper = mapper;

			AddOffers();
			AddCity();
			AddContracts();
			AddTripPrices();
			AddGlobalVariables();
			AddLevelData();
			AddFactories();
			AddBattlePass();
			AddEventLeaderboard();
			AddTasks();
			AddProjects();
		}

		private void AddProjects()
		{
			_mapper.AddMap<IRetroactiveTaskDto, CRetroactiveProjectTask>(o => o switch
			{
				CUpgradeFactoryToLevelRetroactiveTaskDto dto
					=> new CRetroactiveProjectTask(dto.TaskId, dto.TargetCount, dto.Factory),
				CCountableRetroactiveTaskDto dto
					=> new CRetroactiveProjectTask(dto.TaskId, dto.TargetCount),
				_ => throw new NotImplementedException()
			});

			_mapper.AddMap<CActiveProjectDto, CActiveProject>(o =>
			{
				CActiveTask[] activeTasks = _mapper.Map<CActiveTaskDto, CActiveTask>(o.ActiveTasks);
				CRetroactiveProjectTask[] retroactiveTasks = _mapper.Map<IRetroactiveTaskDto, CRetroactiveProjectTask>(o.RetroactiveTasks);
				return new CActiveProject(o.ProjectId, activeTasks, retroactiveTasks);
			});
		}

		private void AddTasks()
		{
			_mapper.AddMap<CActiveTaskDto, CActiveTask>(o =>
			{
				CCountableTaskRequirement[] taskRequirements = _mapper.Map<CCountableTaskRequirementDto, CCountableTaskRequirement>(o.Requirements);
				IValuable[] rewards = _mapper.FromJson<IValuable>(o.Rewards);
				return new CActiveTask(o.Uid, o.TaskId, taskRequirements, rewards, o.IsClaimed, o.FrontendOrder);
			});

			_mapper.AddMap<CCountableTaskRequirementDto, CCountableTaskRequirement>(o => new CCountableTaskRequirement(o.TaskRequirementType, o.Id, o.TargetCount, o.CurrentCount));

			_mapper.AddMap<CWeeklyTaskDto, CWeeklyTask>(o =>
			{
				IValuable[] rewards = _mapper.FromJson<IValuable>(o.Rewards);
				return new CWeeklyTask(rewards, o.RequiredPoints, o.IsClaimed, o.Uid);
			});
		}

		private void AddEventLeaderboard()
		{
			_mapper.AddMap<ILeaderboardDto, CLeaderboard>(o =>
			{
				switch (o)
				{
					case CLiveEventLeaderboardDto leaderboard:
						CLeaderboardUser[] competitors = _mapper.Map<CLeaderboardUserDto, CLeaderboardUser>(leaderboard.Competitors);
						CLeaderboardReward[] rewards = _mapper.Map<CLeaderboardRewardDto, CLeaderboardReward>(leaderboard.Rewards);
						return new CLeaderboard(competitors, rewards, leaderboard.EndTime, leaderboard.Uid, leaderboard.PhaseIndex);
				}

				throw new NotImplementedException();
			});

			_mapper.AddMap<CLeaderboardUserDto, CLeaderboardUser>(o =>
			{
				CUserSocial userSocial = _mapper.Map<CUserSocialDto, CUserSocial>(o.UserSocial);
				return new CLeaderboardUser(userSocial, o.Points, o.PointsChangeTime);
			});

			_mapper.AddMap<CLeaderboardRewardDto, CLeaderboardReward>(o =>
			{
				IValuable[] rewards = _mapper.FromJson<IValuable>(o.Rewards);
				return new CLeaderboardReward(o.MinRank, rewards);
			});

			_mapper.AddMap<CUserSocialDto, CUserSocial>(o =>
				new CUserSocial(o.EncryptedUid, o.NickName, o.Frame, o.Avatar, o.IsOnline));

			_mapper.AddMap<CLeaderboardComplementDto, CLeaderboardComplement>(o =>
			{
				CLeaderboardUser[] modifications = _mapper.Map<CLeaderboardUserDto, CLeaderboardUser>(o.ValuableModifications);
				return new CLeaderboardComplement(o.LeaderboardUid, modifications);
			});
		}

		private void AddBattlePass()
		{
			_mapper.AddMap<CBattlePassContentDto, CBattlePassContent>(o =>
			{
				CBattlePassReward[] freeRewards = _mapper.Map<CBattlePassRewardTemplateDto, CBattlePassReward>(o.FreeRewards);
				CBattlePassReward[] premiumRewards = _mapper.Map<CBattlePassRewardTemplateDto, CBattlePassReward>(o.PremiumRewards);
				COffer premiumOffer = _mapper.Map<COfferDto, COffer>(o.PremiumOffer);
				COffer extraPremiumOffer = _mapper.Map<COfferDto, COffer>(o.ExtraPremiumOffer);
				IValuable bonusBankReward = _mapper.FromJson<IValuable>(o.BonusBankReward);
				return new CBattlePassContent(freeRewards, premiumRewards, o.PointsRequiredForTier, extraPremiumOffer, premiumOffer,
					o.ClaimedBonusBanksCount, bonusBankReward, o.PointsNeededForBonusBankTier, o.BonusBankRewardsCount);
			});

			_mapper.AddMap<CBattlePassRewardTemplateDto, CBattlePassReward>(o =>
			{
				IValuable reward = _mapper.FromJson<IValuable>(o.Reward);
				return new CBattlePassReward(reward, o.CanBeDoubled, o.IsPropagated);
			});

			_mapper.AddMap<CBattlePassDataDto, CBaseBattlePassData>(o
				=> new CBaseBattlePassData(o.PremiumStatus, o.ClaimedFreeIndexes, o.ClaimedPremiumIndexes));
		}

		private void AddLevelData()
		{
			_mapper.AddMap<CLevelDataDto, CLevelData>(o => new CLevelData(o.Level, o.UpgradeStartTime));
		}

		private void AddFactories()
		{
			_mapper.AddMap<CFactorySlotDto, CFactorySlot>(o
				=> new CFactorySlot(o.Index, o.CraftingProduct, o.CompletionTime, o.IsUnlocked));
		}

		private void AddCity()
		{
			_mapper.AddMap<CBuildingPlotDto, CBuildingPlot>(o => new CBuildingPlot(o.Index, o.IsUnlocked, o.Building));
		}

		private void AddContracts()
		{
			_mapper.AddMap<CContractDto, CContract>(o =>
			{
				CTripPrice tripPrice = _mapper.Map<CTripPriceDto, CTripPrice>(o.TripPrice);
				IValuable[] rewards = _mapper.FromJson<IValuable>(o.Rewards);

				switch (o.Type)
				{
					case EContractType.Story:
					{
						CStaticContractData metaData = _mapper.Map<CStaticContractMetaDataDto, CStaticContractData>(o.MetaData);
						CFleetContractData fleetData = o.FleetTotalPower > 0
							? new CFleetContractData(o.FleetTotalPower,
								o.FleetSlots?.Select(s => new CTransportFleetSlotConfig(s.MovementType, s.TransportType)).ToArray()
								?? Array.Empty<CTransportFleetSlotConfig>())
							: null;
						return new CContract(EContractType.Story, o.Requirement, o.DeliveredAmount, tripPrice, rewards, o.Customer, o.Uid, o.IsSeen, o.Regions, o.MovementType, staticData: metaData, fleetData: fleetData);
					}
					case EContractType.Passenger:
					{
						return new CContract(EContractType.Passenger, o.Requirement, o.DeliveredAmount, tripPrice, rewards, o.Customer, o.Uid, o.IsSeen, o.Regions, o.MovementType, passengerData: new CPassengerContractData(o.CityId));
					}
					case EContractType.Event:
					{
						CStaticContractData metaData = _mapper.Map<CStaticContractMetaDataDto, CStaticContractData>(o.MetaData);
						return new CContract(EContractType.Event, o.Requirement, o.DeliveredAmount, tripPrice, rewards, o.Customer, o.Uid,
							o.IsSeen || o.MetaData.Task > 1, o.Regions, o.MovementType, staticData: metaData, eventData: new CEventContractData(o.EventId, o.IsInfinity));
					}
					default:
						throw new ArgumentOutOfRangeException(nameof(o.Type), o.Type, "Unknown contract type");
				}
			});

			_mapper.AddMap<CStaticContractMetaDataDto, CStaticContractData>(o => new CStaticContractData(
				o.Id, o.IsActivated, o.Task, o.TotalTasksCount, o.AllowGoTo, o.StoryDialogue));
		}

		private void AddTripPrices()
		{
			_mapper.AddMap<CTripPriceDto, CTripPrice>(o => new CTripPrice(o.FuelPrice, o.DurabilityPrice));
		}

		private void AddOffers()
		{
			_mapper.AddMap<COfferDto, COffer>(o =>
			{
				IValuable price = _mapper.FromJson<IValuable>(o.Price);
				IValuable[] rewards = _mapper.FromJson<IValuable>(o.Rewards);
				IOfferParam[] @params = _mapper.Map<COfferParamDto, IOfferParam>(o.Params);
				return new COffer(
					o.Guid,
					price,
					rewards,
					@params,
					o.Tags,
					o.IsSeen
				);
			});

			_mapper.AddMap<COfferGroupDto, COfferGroup>(o =>
			{
				IOfferParam[] @params = _mapper.Map<COfferParamDto, IOfferParam>(o.Params);
				return new COfferGroup(o.Tags, @params);
			});
		}

		private void AddGlobalVariables()
		{
			_mapper.AddMap<CGlobalVariableDto, CGlobalVariable>(o =>
			{
				CGlobalVariable variable = new (o.Id);
				variable.SetValue(o.StringValue);
				return variable;
			});
		}
	}
}
