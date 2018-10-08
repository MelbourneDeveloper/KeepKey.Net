
using KeepKey.Net.Contracts;
using System;
using System.Collections.Generic;
using Trezor.Net;

namespace KeepKey.Net
{
    public class KeepKeyCoinUtility : ICoinUtility
    {
        private readonly Dictionary<uint, CoinType> _CoinTypesByCoinIndex = new Dictionary<uint, CoinType>();

        public KeepKeyCoinUtility(IEnumerable<CoinType> coinTypes)
        {
            foreach (var coinType in coinTypes)
            {
                _CoinTypesByCoinIndex.Add(Hardwarewallets.Net.AddressManagement.AddressUtilities.UnhardenNumber(coinType.Bip44AccountPath), coinType);
            }
        }

        public CoinInfo GetCoinInfo(uint coinNumber)
        {
            switch (coinNumber)
            {
                case 0:
                    return new CoinInfo(null, AddressType.Bitcoin);
                case 60:
                    return new CoinInfo(null, AddressType.Ethereum);
                case 61:
                    return new CoinInfo(null, AddressType.Ethereum);
                case 145:
                    return new CoinInfo("Bcash", AddressType.Bitcoin);
                default:
                    throw new NotImplementedException($"The coin number {coinNumber} has not been filled in for {nameof(DefaultCoinUtility)}. Please create a class that implements ICoinUtility, or call an overload that specifies coin information.");
            }
        }
    }
}

