using TMPro;
using UnityEngine;
using UnityEngine.UI;
using FunticoGamesSDK.ViewModels;

namespace FunticoGamesSDK.UI.HallOfFame
{
    public class RoomPrizeView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text amountText;
        
        public void Initialize() { }
        
        public void Show(RoomPrizeViewModel prizeData)
        {
            if (prizeData == null) return;
            icon.sprite = prizeData.Sprite;
            amountText.text = prizeData.Amount.ToString();
        }
    }
}
