using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FunticoGamesSDK.APIModels;
using FunticoGamesSDK.APIModels.UserData;
using UnityEngine;

namespace FunticoGamesSDK.ViewModels
{
    [Serializable]
    public class RoomViewModel
    {
        public string Guid { get; set; }
        public RoomTierEnum Tier { get; set; }
        public string Name { get; set; }
        public Func<Task<Sprite>> LoadRoomImageTask { get; set; }
        public Ticket Ticket { get; set; }
        public bool IsFree => Ticket.IsFree();
        public bool UserCanJoinWithVoucher { get; set; }
        public VoucherData Voucher { get; set; }
        public Sprite FeeIcon { get; set; }
        public ulong EntryFee => (ulong) (Ticket?.CurrencyAmount ?? 0);
        public float EntryFeeUSDT { get; set; }
        public ulong TotalPrize { get; set; }
        public bool IsPrePaid { get; set; }
        public bool RequireKyc { get; set; }

        public RoomViewModel() { }

        public RoomViewModel(RoomConfig config, List<VoucherData> vouchers)
        {
            Guid = config.Id;
            if (config.Details.Tier != null)
                Tier = (RoomTierEnum) config.Details.Tier;
            Name = config.Details.Name;
            Voucher = vouchers.FirstOrDefault(voucher => voucher.Tier == config.Details.Tier);
            UserCanJoinWithVoucher = Voucher != null && Voucher.Count > 0 && Voucher.PlayCount >= Voucher.PlaysRequiredToActivate;
            Ticket = config.Ticket;
            if (config.Type is RoomType.external)
            {
                EntryFeeUSDT = Tier switch {
                    RoomTierEnum.Contender => 2,
                    RoomTierEnum.Challenger => 4,
                    RoomTierEnum.Champion => 8,
                    _ => 0
                };
            }
            else
            {
                EntryFeeUSDT = Tier switch {
                    RoomTierEnum.Contender => 2,
                    RoomTierEnum.Challenger => 10,
                    RoomTierEnum.Champion => 20,
                    _ => 0
                };
            }
            RequireKyc = false;
            TotalPrize = (ulong) (config.Computed.Stake - config.Computed.PlatformFee);
        }
    }
}