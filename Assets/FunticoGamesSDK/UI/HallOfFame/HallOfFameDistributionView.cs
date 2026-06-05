using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using FunticoGamesSDK.HallOfFameProviders;
using FunticoGamesSDK.ViewModels;

namespace FunticoGamesSDK.UI.HallOfFame
{
    public class HallOfFameDistributionView : MonoBehaviour
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private HOFDistibutionItemView rowPrefab;
        [SerializeField] private Transform contentParent;
        
        private List<HOFDistibutionItemView> _spawnedRows = new List<HOFDistibutionItemView>();
        private IHallOfFameProvider _provider;
        
        public void Initialize(IHallOfFameProvider provider)
        {
            _provider = provider;
            closeButton.onClick.AddListener(Close);
        }
        
        public async UniTask Open()
        {
            gameObject.SetActive(true);
            await RefreshPrizes();
        }
        
        private async UniTask RefreshPrizes()
        {
            ClearContent();
            
            var hofViewModel = await _provider.GetHallOfFameDistribution();
            if (hofViewModel != null && hofViewModel.Items != null)
            {
                foreach (var item in hofViewModel.Items)
                {
                    SpawnRow(item);
                }
            }
        }
        
        private void SpawnRow(HOFDistributionItemViewModel itemData)
        {
            var row = Instantiate(rowPrefab, contentParent);
            row.Show(itemData);
            _spawnedRows.Add(row);
        }
        
        private void ClearContent()
        {
            foreach (var row in _spawnedRows)
            {
                if (row != null && row.gameObject != null)
                {
                    Destroy(row.gameObject);
                }
            }
            _spawnedRows.Clear();
        }
        
        private void Close()
        {
            gameObject.SetActive(false);
            ClearContent();
        }
    }
}
