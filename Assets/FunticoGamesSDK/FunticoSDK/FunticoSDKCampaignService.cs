using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.CampaignProviders;

namespace FunticoGamesSDK
{
	public partial class FunticoSDK : ICampaignProvider
	{
		private ICampaignProvider _campaignProvider;

		private void SetupCampaignProvider()
		{
			_campaignProvider = new CampaignProvider(_privateGameKey, _clientSessionManager);
		}

		public UniTask<CampaignAvailabilityResponse> GetCampaignAvailability(bool useCache = true) =>
			_campaignProvider.GetCampaignAvailability(useCache);

		public CampaignAvailabilityResponse GetCachedCampaignAvailability() =>
			_campaignProvider.GetCachedCampaignAvailability();

		public UniTask<CampaignLevelsResponse> GetCampaignLevels(bool useCache = true) =>
			_campaignProvider.GetCampaignLevels(useCache);

		public CampaignLevelsResponse GetCachedCampaignLevels() => _campaignProvider.GetCachedCampaignLevels();

		public UniTask<StartLevelResponse> StartCampaignLevel(Guid levelId) =>
			_campaignProvider.StartCampaignLevel(levelId);

		public StartLevelResponse GetActiveCampaignAttempt() => _campaignProvider.GetActiveCampaignAttempt();

		public void AbandonCampaignAttempt() => _campaignProvider.AbandonCampaignAttempt();

		public UniTask<CompleteLevelResponse> CompleteCampaignLevel(Dictionary<string, double> metrics) =>
			_campaignProvider.CompleteCampaignLevel(metrics);

		public T GetCampaignLevelConfig<T>(CampaignLevelDto level) => _campaignProvider.GetCampaignLevelConfig<T>(level);

		public CampaignStarRules GetCampaignStarRules(CampaignLevelDto level) =>
			_campaignProvider.GetCampaignStarRules(level);
	}
}
