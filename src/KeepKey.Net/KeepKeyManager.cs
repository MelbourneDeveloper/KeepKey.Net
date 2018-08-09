using Hid.Net;
using KeepKey.Net.Contracts;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trezor.Manager;

namespace KeepKey.Net
{
    public class KeepKeyManager : TrezorManagerBase
    {
        private string LogSection = nameof(KeepKeyManager);

        public Features Features { get; private set; }

        protected override bool HasFeatures => Features != null;

        protected override string ContractNamespace => "KeepKey.Net.Contracts";

        #region Constructor
        public KeepKeyManager(EnterPinArgs enterPinCallback, IHidDevice trezorHidDevice) : base(enterPinCallback, trezorHidDevice)
        {
        }
        #endregion

        #region Protected Overrides
        protected override async Task<object> PinMatrixAckAsync(string pin)
        {
            var retVal = await SendMessageAsync(new PinMatrixAck { Pin = pin });

            if (retVal is Failure failure)
            {
                throw new FailureException<Failure>("PIN Attempt Failed.", failure);
            }

            return retVal;
        }

        protected override async Task<object> ButtonAckAsync()
        {
            var retVal = await SendMessageAsync(new ButtonAck());

            if (retVal is Failure failure)
            {
                throw new FailureException<Failure>("PIN Attempt Failed.", failure);
            }

            return retVal;
        }

        protected CoinType GetCoinType(string coinShortcut)
        {
            if (!HasFeatures)
            {
                throw new Exception("The Trezor has not been successfully initialised.");
            }

            return Features.Coins.FirstOrDefault(c => c.CoinShortcut == coinShortcut);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get the Trezor's public key at the specified index.
        /// </summary>
        public async Task<PublicKey> GetPublicKeyAsync(string coinShortcut, uint addressNumber)
        {
            return await SendMessageAsync<PublicKey, GetPublicKey>(new GetPublicKey { AddressNs = new[] { addressNumber } });
        }
        #endregion

        #region Public Overrides
        /// <summary>
        /// Get an address from the Trezor
        /// </summary>
        public override async Task<string> GetAddressAsync(string coinShortcut, uint coinNumber, uint account, bool isChange, uint index, bool showDisplay, AddressType addressType, bool? isSegwit)
        {
            if (isSegwit == null)
            {
                throw new ArgumentNullException(nameof(isSegwit));
            }

            try
            {
                //ETH and ETC don't appear here so we have to hard code these not to be segwit
                var coinType = Features.Coins.FirstOrDefault(c => c.CoinShortcut.ToLower() == coinShortcut.ToLower());

                var path = GetAddressPath(isSegwit.Value, account, isChange, index, coinNumber);

                switch (addressType)
                {
                    case AddressType.Bitcoin:

                        return (await SendMessageAsync<Address, GetAddress>(new GetAddress { ShowDisplay = showDisplay, AddressNs = path, CoinName = GetCoinType(coinShortcut)?.CoinName, ScriptType = isSegwit.Value ? InputScriptType.Spendp2shwitness : InputScriptType.Spendaddress })).address;

                    case AddressType.Ethereum:

                        var ethereumAddress = await SendMessageAsync<EthereumAddress, EthereumGetAddress>(new EthereumGetAddress { ShowDisplay = showDisplay, AddressNs = path });

                        var sb = new StringBuilder();
                        foreach (var b in ethereumAddress.Address)
                        {
                            sb.Append(b.ToString("X2").ToLower());
                        }

                        var hexString = sb.ToString();

                        return $"0x{hexString}";

                    case AddressType.NEM:
                        throw new NotImplementedException();
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error Getting Trezor Address", ex, LogSection);
                throw;
            }
        }

        /// <summary>
        /// Initialize the Trezor. Should only be called once.
        /// </summary>
        public override async Task InitializeAsync()
        {
            Features = await SendMessageAsync<Features, Initialize>(new Initialize());

            if (Features == null)
            {
                throw new Exception("Error initializing Trezor. Features were not retrieved");
            }
        }

        protected override bool IsButtonRequest(object response)
        {
            return response is ButtonRequest;
        }

        protected override bool IsPinMatrixRequest(object response)
        {
            return response is PinMatrixRequest;
        }

        protected override bool IsInitialize(object response)
        {
            return response is Initialize;
        }
        #endregion
    }
}
