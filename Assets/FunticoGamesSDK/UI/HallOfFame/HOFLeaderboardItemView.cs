using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.AssetsProvider;
using FunticoGamesSDK.TextureResizer;

namespace FunticoGamesSDK.UI.HallOfFame
{
    public class HOFLeaderboardItemView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI placeText;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI entriesText;
        [SerializeField] private TextMeshProUGUI datePlayedText;
        [SerializeField] private GameObject selectionOutline;
        
        [SerializeField] private Color highlightColor = Color.yellow; // Default fallback
        [SerializeField] private Color defaultColor = Color.white;
        
        [SerializeField] private Image avatarIcon;
        [SerializeField] private Image borderIcon;
        
        public void Show(HallOfFamePlayerEntry itemData, bool isMe, bool isGlobal)
        {
            if (itemData == null) return;
            
            selectionOutline.SetActive(isMe);
            placeText.SetText(itemData.Rank.ToString());
            nameText.SetText(itemData.DisplayName);
            scoreText.SetText(itemData.TotalPoints.ToString());
            entriesText.SetText($"Entries: {itemData.GamesPlayed}");
            
            if (datePlayedText != null)
            {
                if (!string.IsNullOrEmpty(itemData.PlayedAt))
                {
                    datePlayedText.gameObject.SetActive(true);
                    if (DateTime.TryParse(itemData.PlayedAt, out var utcTime))
                    {
                        var localTime = utcTime.ToLocalTime();
                        datePlayedText.SetText(isGlobal 
                            ? localTime.ToString("dd.MM.yyyy HH:mm") 
                            : localTime.ToString("dd.MM HH:mm"));
                    }
                    else
                    {
                        datePlayedText.SetText(itemData.PlayedAt);
                    }
                }
                else
                {
                    datePlayedText.gameObject.SetActive(false);
                }
            }
            
            if (avatarIcon != null && !string.IsNullOrEmpty(itemData.AvatarUrl))
            {
                LoadImage(itemData.AvatarUrl, avatarIcon).Forget();
            }
            
            if (borderIcon != null && !string.IsNullOrEmpty(itemData.AvatarBorder))
            {
                LoadImage(itemData.AvatarBorder, borderIcon).Forget();
            }
            
            SetColors(isMe);
        }
        
        private async UniTaskVoid LoadImage(string url, Image targetImage)
        {
            var sprite = await AssetsLoader.LoadSpriteAsync(url, new TextureResizeOptions(Constants.AVATAR_PREFERRED_SIZE));
            if (targetImage != null && sprite != null)
            {
                targetImage.sprite = sprite;
            }
        }
        
        private void SetColors(bool isMe)
        {
            var targetColor = isMe ? highlightColor : defaultColor;
            placeText.color = targetColor;
            nameText.color = targetColor;
            scoreText.color = targetColor;
        }
        
        public void Clear()
        {
            if (avatarIcon != null)
            {
                avatarIcon.sprite = null;
            }
            if (borderIcon != null)
            {
                borderIcon.sprite = null;
            }
        }
    }
}
