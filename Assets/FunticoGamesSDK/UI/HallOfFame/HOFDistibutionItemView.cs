using System.Collections.Generic;
using TMPro;
using UnityEngine;
using FunticoGamesSDK.ViewModels;

namespace FunticoGamesSDK.UI.HallOfFame
{
    public class HOFDistibutionItemView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI placeRangeText;
        [SerializeField] private RoomPrizeView prizePrefab;
        [SerializeField] private Transform prizeContainer;
        
        public void Show(HOFDistributionItemViewModel itemData)
        {
            if (itemData == null) return;
            
            if (!string.IsNullOrEmpty(itemData.Range))
            {
                placeRangeText.text = itemData.Range;
            }
            else if (itemData.EndPlace.HasValue && itemData.EndPlace.Value > itemData.Place)
            {
                placeRangeText.text = $"{itemData.Place}-{itemData.EndPlace.Value}";
            }
            else
            {
                placeRangeText.text = itemData.Place.ToString();
            }
            
            SpawnPrizes(itemData.Prizes);
        }
        
        private void SpawnPrizes(List<RoomPrizeViewModel> prizes)
        {
            if (prizes == null) return;
            
            foreach (var prize in prizes)
            {
                var prizeView = Instantiate(prizePrefab, prizeContainer);
                prizeView.Initialize();
                prizeView.Show(prize);
            }
        }
    }
}
