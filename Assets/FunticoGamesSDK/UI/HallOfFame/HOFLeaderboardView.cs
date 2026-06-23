using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.HallOfFameProviders;

namespace FunticoGamesSDK.UI.HallOfFame
{
    public class HOFLeaderboardView : MonoBehaviour
    {
        private const int POOL_MAX_SIZE = 200;
        
        [SerializeField] private HOFLeaderboardItemView leaderboardItemPrefab;
        [SerializeField] private HOFLeaderboardItemView currentUserRow; // Рядок поточного гравця (закріплений)
        [SerializeField] private List<GameObject> currentUserGOs;     // Об'єкти для приховування/показу даних гравця
        
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private Transform container;
        [SerializeField] private int limit = 10;
        
        private ObjectPool<HOFLeaderboardItemView> _pool;
        private CancellationTokenSource _tokenSource;
        private List<HOFLeaderboardItemView> _activeViews = new List<HOFLeaderboardItemView>();
        private IHallOfFameProvider _provider;
        
        private bool _isLoading;
        private int _currentPage = 1;
        private HallOfFamePeriodMode _currentPeriodModeFilter;
        private HallOfFameGameModeFilter _currentGameModeFilter;
        
        public HallOfFameResponse Leaderboard { get; private set; }
        
        public void Initialize(IHallOfFameProvider provider)
        {
            _provider = provider;
            _pool ??= CreatePool();
        }
        
        public async UniTask Show(HallOfFamePeriodMode currentPeriodMode, HallOfFameGameModeFilter currentGameMode)
        {
            _currentGameModeFilter = currentGameMode;
            _currentPeriodModeFilter = currentPeriodMode;
            
            Clear();
            ScrollToTop();
            UpdateCancellationToken();
            var token = _tokenSource.Token;
            
            var leaderboardData = await GetLeaderboard(_currentPage);
            if (token.IsCancellationRequested) return;
            Leaderboard = leaderboardData;
            
            if (Leaderboard != null)
            {
                ShowLeaderboard(Leaderboard);
                SubscribeLoadMore();
            }
        }
        
        public void Clear()
        {
            if (currentUserGOs != null)
            {
                currentUserGOs.ForEach(go => {
                    if (go != null) go.SetActive(false);
                });
            }
            
            UnsubscribeLoadMore();
            UpdateCancellationToken();
            Leaderboard = null;
            _currentPage = 1;
            ClearScroll();
        }
        
        public void ScrollToTop()
        {
            if (scroll != null)
            {
                scroll.verticalNormalizedPosition = 1f;
            }
        }
        
        private void ShowLeaderboard(HallOfFameResponse leaderboard, bool updateMe = true)
        {
            if (updateMe)
            {
                UpdateUserData(leaderboard);
            }
            
            var meUserId = leaderboard.CurrentPlayer?.UserId;
            if (leaderboard.Leaderboard != null)
            {
                foreach (var userRecord in leaderboard.Leaderboard)
                {
                    CreateView(userRecord, meUserId);
                }
            }
        }
        
        private void UpdateUserData(HallOfFameResponse leaderboard)
        {
            var userParticipated = leaderboard.CurrentPlayer != null;
            if (currentUserGOs != null)
            {
                currentUserGOs.ForEach(go => {
                    if (go != null) go.SetActive(userParticipated);
                });
            }
            
            if (userParticipated && currentUserRow != null)
            {
                currentUserRow.Show(leaderboard.CurrentPlayer, true, _currentPeriodModeFilter == HallOfFamePeriodMode.Global);
            }
        }
        
        private void ClearScroll()
        {
            while (_activeViews.Count > 0)
            {
                _pool.Release(_activeViews[0]);
            }
        }
        
        private void CreateView(HallOfFamePlayerEntry userRecord, int? meUserId)
        {
            if (userRecord == null) return;
            var isMe = meUserId != null && userRecord.UserId.Equals(meUserId.Value);
            var item = _pool.Get();
            item.Show(userRecord, isMe, _currentPeriodModeFilter == HallOfFamePeriodMode.Global);
        }
        
        private async UniTask<HallOfFameResponse> GetLeaderboard(int page)
        {
            _isLoading = true;
            var result = await _provider.GetHallOfFame(_currentPeriodModeFilter, page, limit, _currentGameModeFilter);
            _isLoading = false;
            return result;
        }
        
        private void SubscribeLoadMore()
        {
            if (scroll != null)
            {
                scroll.onValueChanged.AddListener(CheckForLoadMore);
            }
        }
        
        private void UnsubscribeLoadMore()
        {
            if (scroll != null)
            {
                scroll.onValueChanged.RemoveListener(CheckForLoadMore);
            }
        }
        
        private async void CheckForLoadMore(Vector2 scrollPosition)
        {
            if (_isLoading || Leaderboard == null) return;
            if (scrollPosition.y <= 0.05f)
            {
                await LoadNextPage(_tokenSource.Token);
            }
        }
        
        private async UniTask LoadNextPage(CancellationToken cToken)
        {
            var loadedItemsCount = _currentPage * limit;
            if (Leaderboard.TotalPlayers <= loadedItemsCount) return;
            
            _currentPage++;
            var nextLeaderboard = await GetLeaderboard(_currentPage);
            
            if (cToken.IsCancellationRequested) return;
            if (nextLeaderboard != null && nextLeaderboard.Leaderboard != null)
            {
                if (Leaderboard.Leaderboard == null)
                {
                    Leaderboard.Leaderboard = new List<HallOfFamePlayerEntry>();
                }
                Leaderboard.Leaderboard.AddRange(nextLeaderboard.Leaderboard);
                ShowLeaderboard(nextLeaderboard, updateMe: false);
            }
        }
        
        private void UpdateCancellationToken()
        {
            if (_tokenSource != null)
            {
                _tokenSource.Cancel();
                _tokenSource.Dispose();
            }
            _tokenSource = new CancellationTokenSource();
        }
        
        private ObjectPool<HOFLeaderboardItemView> CreatePool()
        {
            return new ObjectPool<HOFLeaderboardItemView>(
                createFunc: () => Instantiate(leaderboardItemPrefab, container),
                actionOnGet: item => {
                    item.transform.SetAsLastSibling();
                    item.gameObject.SetActive(true);
                    _activeViews.Add(item);
                },
                actionOnRelease: item => {
                    item.gameObject.SetActive(false);
                    _activeViews.Remove(item);
                    item.Clear();
                },
                actionOnDestroy: item => {
                    if (item != null && item.gameObject != null)
                    {
                        Destroy(item.gameObject);
                    }
                },
                maxSize: POOL_MAX_SIZE
            );
        }
    }
}
