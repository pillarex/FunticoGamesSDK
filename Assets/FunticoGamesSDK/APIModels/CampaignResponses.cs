using System;
using System.Collections.Generic;

namespace FunticoGamesSDK.APIModels
{
    public class CampaignAvailabilityResponse
    {
        public bool IsAvailable { get; set; }
        public int LevelCount { get; set; }
        public int MaxStars { get; set; }
    }

    public class CampaignLevelsResponse
    {
        public bool IsAvailable { get; set; }
        public int TotalLevels { get; set; }
        public int CompletedLevels { get; set; }
        public int TotalStarsEarned { get; set; }
        public int MaxStars { get; set; }
        public List<CampaignLevelDto> Levels { get; set; } = new List<CampaignLevelDto>();
    }

    public enum CampaignLevelStatus
    {
        Locked = 0,
        Unlocked = 1,
        Completed = 2
    }

    public class CampaignLevelDto
    {
        public Guid LevelId { get; set; }
        public int Order { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        /// <summary>Raw wire value; use <see cref="GetStatus"/> for the typed enum.</summary>
        public int Status { get; set; }

        public int BestStars { get; set; }
        public Dictionary<string, double> BestMetrics { get; set; }

        /// <summary>Null while the level is Locked.</summary>
        public string LevelJsonData { get; set; }

        /// <summary>Null while the level is Locked.</summary>
        public string StarRulesJson { get; set; }

        public int MinDurationSeconds { get; set; }
        public int ConfigVersion { get; set; }
        public List<CampaignReward> Rewards { get; set; } = new List<CampaignReward>();
        public DateTime? CompletedAt { get; set; }

        public CampaignLevelStatus GetStatus() => (CampaignLevelStatus)Status;
    }

    public class CampaignReward
    {
        public string Type { get; set; }
        public double Amount { get; set; }
        public long? ItemId { get; set; }
        public int RequiredStars { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
    }

    public class StartLevelResponse
    {
        public Guid AttemptId { get; set; }
        public Guid LevelId { get; set; }
        public int ConfigVersion { get; set; }
        public DateTime StartedAt { get; set; }
    }

    public class CampaignCompleteLevelRequest
    {
        public Guid AttemptId { get; set; }
        public string EncryptedData { get; set; }
        public string Hash { get; set; }
    }

    public class CompleteLevelResponse
    {
        public bool IsWin { get; set; }
        public int Stars { get; set; }
        public int BestStars { get; set; }
        public bool IsNewBest { get; set; }
        public bool LevelCompletedFirstTime { get; set; }
        public Guid? UnlockedLevelId { get; set; }
        public List<CampaignReward> Rewards { get; set; } = new List<CampaignReward>();
        public bool RewardsGranted { get; set; }
        public int TotalLevels { get; set; }
        public int CompletedLevels { get; set; }
        public int TotalStarsEarned { get; set; }
        public int MaxStars { get; set; }
    }

    public class CampaignStarRules
    {
        public List<CampaignStarRuleCondition> Win { get; set; } = new List<CampaignStarRuleCondition>();
        public List<CampaignStarTier> Stars { get; set; } = new List<CampaignStarTier>();
    }

    public class CampaignStarTier
    {
        public int Star { get; set; }
        public List<CampaignStarRuleCondition> Conditions { get; set; } = new List<CampaignStarRuleCondition>();
    }

    public class CampaignStarRuleCondition
    {
        public string Metric { get; set; }
        public string Op { get; set; }
        public double Value { get; set; }
    }
}
