
using Hardwarewallets.Net.AddressManagement;
using KeepKey.Net.Contracts;
using System;
using System.Collections.Generic;
using Trezor.Net;

namespace KeepKey.Net
{
    public class KeepKeyCoinUtility : ICoinUtility
    {
        #region Fields
        private readonly Dictionary<uint, CoinInfo> _CoinInfoByCoinType = new Dictionary<uint, CoinInfo>();
        #endregion

        #region Public Properties
        public bool IsLegacy { get; set; } = true;
        #endregion

        #region Public Methods
        public CoinInfo GetCoinInfo(uint coinNumber)
        {
            return !_CoinInfoByCoinType.TryGetValue(coinNumber, out var retVal)
                ? throw new ManagerException($"No coin info for coin {coinNumber}")
                : retVal;
        }

        public KeepKeyCoinUtility(IEnumerable<CoinType> coinTypes)
        {
            if (coinTypes == null) throw new ArgumentNullException(nameof(coinTypes));

            foreach (var coinType in coinTypes)
            {
                var coinTypeIndex = AddressUtilities.UnhardenNumber(coinType.Bip44AccountPath);

                //Seems like there are some coins on the KeepKey with the wrong index. I.e. they are actually Ethereum?
                if (_CoinInfoByCoinType.ContainsKey(coinTypeIndex)) continue;

                var addressType = coinType.AddressType switch
                {
                    65535 => AddressType.Ethereum,
                    _ => AddressType.Bitcoin,
                };

                _CoinInfoByCoinType.Add(coinTypeIndex, new CoinInfo(coinType.CoinName, addressType, !IsLegacy && coinType.Segwit, AddressUtilities.UnhardenNumber(coinType.Bip44AccountPath)));
            }
        }
        #endregion
    }
}
