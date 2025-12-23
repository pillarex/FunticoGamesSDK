using System;
using System.Collections.Generic;
using FunticoGamesSDK.APIModels.PrizesResponses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FunticoGamesSDK.APIModels.Converters
{
	public class PrizeConverter : JsonConverter
	{
		private static readonly Dictionary<PrizeType, Type> PrizeTypeMapping = new()
		{
			{ PrizeType.External, typeof(ManualPrize) },
			{ PrizeType.Placeholder, typeof(ManualPrize) },
			{ PrizeType.Item, typeof(ItemAutomatedPrize) },
			{ PrizeType.DepositStakePoolShare, typeof(DepositStakeAutomatedPrize) },
			{ PrizeType.DepositStakePerPlayer, typeof(DepositStakeAutomatedPrize) },
			{ PrizeType.GppPoolShare, typeof(GppAutomatedPrize) },
			{ PrizeType.GppPerPlayer, typeof(GppAutomatedPrize) },
			{ PrizeType.Reward, typeof(RewardAutomatedPrize) },
		};

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Prize) || objectType.IsSubclassOf(typeof(Prize));
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
			JsonSerializer serializer)
		{
			var jsonObject = JObject.Load(reader);
			if (!jsonObject.TryGetValue("type", StringComparison.OrdinalIgnoreCase, out JToken typeToken))
			{
				throw new JsonSerializationException("Missing 'type' field in JSON.");
			}

			if (!Enum.TryParse(typeToken.ToString(), out PrizeType type))
			{
				throw new JsonSerializationException($"Unknown prize type: {typeToken}");
			}

			if (!PrizeTypeMapping.TryGetValue(type, out Type prizeType))
			{
				throw new JsonSerializationException($"Unknown prize type: {type}");
			}

			var prize = (Prize)Activator.CreateInstance(prizeType);

			serializer.Populate(jsonObject.CreateReader(), prize);

			return prize;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value is not Prize prize)
			{
				throw new JsonSerializationException("Expected Prize object.");
			}

			var jsonObject = new JObject
			{
				["type"] = JToken.FromObject(prize.Type, serializer)
			};

			foreach (var prop in value.GetType().GetProperties())
			{
				if (prop.Name == nameof(Prize.Type)) continue; // Пропускаємо дублювання Type

				var propValue = prop.GetValue(value);
				if (propValue != null)
				{
					jsonObject[prop.Name] = JToken.FromObject(propValue, serializer);
				}
			}

			jsonObject.WriteTo(writer);
		}
	}
}