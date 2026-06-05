using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.HallOfFameProviders;

namespace FunticoGamesSDK.UI.HallOfFame
{
    public class HallOfFameView : MonoBehaviour
    {
        private const HallOfFameGameModeFilter DEFAULT_GAME_MODE_FILTER = HallOfFameGameModeFilter.All;
        private const HallOfFamePeriodMode DEFAULT_PERIOD_FILTER = HallOfFamePeriodMode.Global;
        
        [SerializeField] private HOFLeaderboardView leaderboardView;
        [SerializeField] private Button closeButton;
        
        [SerializeField] private TabSwitcher periodModeTabs;
        [SerializeField] private TabSwitcher gameModeTabs;
        
        [SerializeField] private Button checkPrizesButton;
        [SerializeField] private GameObject distributionPopup; // Вікно розподілу нагород
        
        private HallOfFamePeriodMode _currentPeriodModeFilter;
        private HallOfFameGameModeFilter _currentGameModeFilter;
        private bool _subscribed;
        
        public void Initialize(IHallOfFameProvider provider)
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }
            
            if (checkPrizesButton != null)
            {
                checkPrizesButton.onClick.AddListener(OpenPrizesPopup);
            }
            
            if (leaderboardView != null)
            {
                leaderboardView.Initialize(provider);
            }
            
            if (periodModeTabs != null)
            {
                periodModeTabs.Init();
                periodModeTabs.OnSwitchTab += index => SwitchPeriodMode((HallOfFamePeriodMode)index).Forget();
            }
            
            if (gameModeTabs != null)
            {
                gameModeTabs.Init();
                gameModeTabs.OnSwitchTab += index => SwitchGameMode((HallOfFameGameModeFilter)index).Forget();
            }
        }
        
        public async UniTask Open()
        {
            _subscribed = true;
            _currentGameModeFilter = DEFAULT_GAME_MODE_FILTER;
            _currentPeriodModeFilter = DEFAULT_PERIOD_FILTER;
            
            if (periodModeTabs != null)
            {
                periodModeTabs.SwitchTab((int)_currentPeriodModeFilter, true, true);
            }
            
            if (gameModeTabs != null)
            {
                gameModeTabs.SwitchTab((int)_currentGameModeFilter, true, true);
            }
            
            UpdateCheckPrizesState(_currentPeriodModeFilter);
            
            gameObject.SetActive(true);
            
            if (leaderboardView != null)
            {
                await leaderboardView.Show(_currentPeriodModeFilter, _currentGameModeFilter);
            }
        }
        
        private async UniTask SwitchGameMode(HallOfFameGameModeFilter filter)
        {
            _currentGameModeFilter = filter;
            if (leaderboardView != null)
            {
                await leaderboardView.Show(_currentPeriodModeFilter, filter);
            }
        }
        
        private async UniTask SwitchPeriodMode(HallOfFamePeriodMode filter)
        {
            _currentPeriodModeFilter = filter;
            UpdateCheckPrizesState(filter);
            if (leaderboardView != null)
            {
                await leaderboardView.Show(filter, _currentGameModeFilter);
            }
        }
        
        private void UpdateCheckPrizesState(HallOfFamePeriodMode periodMode)
        {
            if (checkPrizesButton != null)
            {
                // Показуємо кнопку призів тільки для щомісячного періоду
                checkPrizesButton.gameObject.SetActive(periodMode == HallOfFamePeriodMode.Monthly);
            }
        }
        
        private void OpenPrizesPopup()
        {
            if (distributionPopup != null)
            {
                distributionPopup.SetActive(true);
                var distView = distributionPopup.GetComponent<HallOfFameDistributionView>();
                if (distView != null)
                {
                    distView.Open().Forget();
                }
            }
        }
        
        private void Close()
        {
            gameObject.SetActive(false);
            CleanUp();
        }
        
        private void CleanUp()
        {
            if (leaderboardView != null)
            {
                leaderboardView.Clear();
            }
            _subscribed = false;
        }
    }
}
