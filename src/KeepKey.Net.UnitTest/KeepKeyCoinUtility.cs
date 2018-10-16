
using Hardwarewallets.Net.AddressManagement;
using KeepKey.Net.Contracts;
using System.Collections.Generic;
using System.Linq;
using Trezor.Net;

namespace KeepKey.Net
{
    public partial class KeepKeyCoinUtility : ICoinUtility
    {
        private readonly Dictionary<uint, CoinInfo> _CoinInfoByCoinType = new Dictionary<uint, CoinInfo>();

        public CoinInfo GetCoinInfo(uint coinNumber)
        {
            if (!_CoinInfoByCoinType.TryGetValue(coinNumber, out var retVal)) throw new ManagerException($"No coin info for coin {coinNumber}");
            return retVal;
        }

        public KeepKeyCoinUtility(IEnumerable<CoinType> coinTypes)
        {
            foreach (var coinType in coinTypes)
            {
                var coinTypeIndex = AddressUtilities.UnhardenNumber(coinType.Bip44AccountPath);

                //Seems like there are some coins on the KeepKey with the wrong index. I.e. they are actually Ethereum?
                if (_CoinInfoByCoinType.ContainsKey(coinTypeIndex)) continue;

                AddressType addressType;

                //TODO: Is this a good way to do this? How can we tell which coins are Bitcoin?
                switch (coinType.AddressType)
                {
                    case 65536:
                        addressType = AddressType.Ethereum;
                        break;
                    default:
                        addressType = AddressType.Bitcoin;
                        break;
                }

                _CoinInfoByCoinType.Add(coinTypeIndex, new CoinInfo(coinType.CoinName, addressType, false, AddressUtilities.UnhardenNumber(coinType.Bip44AccountPath)));
            }
        }


    }
}
