
using KeepKey.Net.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trezor.Net;

namespace KeepKey.Net
{
    public partial class KeepKeyCoinUtility : ICoinUtility
    {
        private readonly Dictionary<uint, CoinInfo> _CoinInfoByCoinType = new Dictionary<uint, CoinInfo>();

        public CoinInfo GetCoinInfo(uint coinNumber)
        {
            throw new NotImplementedException();
        }

        public async Task Initialize()
        {

        }


    }
}
