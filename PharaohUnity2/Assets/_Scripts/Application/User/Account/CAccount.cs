// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.07.2025
// =========================================

using AldaEngine;
using ServerData;
using ServerData.Dto;
using ServerData.Hits;
using ServiceEngine;

namespace TycoonBuilder
{
	public class CAccount : CBaseUserComponent
	{
		public string PublicId { get; private set; }
		public string PublicIdShort { get; private set; }
		public string EncryptedUid { get; private set; }
		private string FacebookUserId { get; set; }
		public string Nickname { get; private set; }
		public EProfileAvatar Avatar { get; private set; }
		public EProfileFrame Frame { get; private set; }
		public ECountryCode CountryCode { get; set; }
		
		private readonly CFacebookAvatarGateway _fbAvatarGateway;
		private readonly ICtsProvider _ctsProvider;
		private readonly CHitBuilder _hitBuilder;
		private readonly IEventBus _eventBus;

		public CAccount(
			CFacebookAvatarGateway fbAvatarGateway,
			ICtsProvider ctsProvider,
			CHitBuilder hitBuilder,
			IEventBus eventBus
			)
		{
			_fbAvatarGateway = fbAvatarGateway;
			_ctsProvider = ctsProvider;
			_hitBuilder = hitBuilder;
			_eventBus = eventBus;
		}
		
		public void InitialSync(CAccountDto dtoAccount)
		{
			PublicId = dtoAccount.PublicId;
			PublicIdShort = dtoAccount.PublicIdShort;
			EncryptedUid = dtoAccount.EncryptedUid;
			FacebookUserId = dtoAccount.FacebookUserId;
			Nickname = dtoAccount.Nickname;
			Avatar = dtoAccount.Avatar;
			Frame = dtoAccount.Frame;
			CountryCode = dtoAccount.CountryCode;
			
			_fbAvatarGateway.PreloadAvatars(new []{GetFacebookUserId()}, _ctsProvider.Token);
		}
		
		public string GetFacebookUserId()
		{
			return CPlatform.IsEditor ? IFacebookService.TestingUserId : FacebookUserId;
		}

		public void SetFacebookUserId(string facebookId)
		{
			FacebookUserId = facebookId;
		}

		public void SetAccount(string nickname, EProfileAvatar avatar, EProfileFrame frame)
		{
			Nickname = nickname;
			Avatar = avatar;
			Frame = frame;
			
			CHitRecordBuilder hit = _hitBuilder.GetBuilder(new CEditUserSocialRequest(nickname, avatar, frame));
			hit.BuildAndSend();
			
			_eventBus.Send(new CAccountSocialsChangedSignal());
		}
	}
}