using Hid.Net;
using KeepKey.Net.Contracts;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trezor.Net;

namespace KeepKey.Net
{
    /// <summary>
    /// Manages communication with the KeepKey device
    /// </summary>
    public class KeepKeyManager : TrezorManagerBase
    {
        #region Private Constants
        private readonly string LogSection = nameof(KeepKeyManager);
        #endregion

        #region Public Constants
        public const ushort VendorId = 11044;
        public const ushort ProductId = 1;
        #endregion

        #region Public Properties
        public Features Features { get; private set; }
        #endregion

        #region Protected Properties
        protected override bool HasFeatures => Features != null;
        protected override string ContractNamespace => "KeepKey.Net.Contracts";

        protected override Type MessageTypeType => typeof(MessageType);
        #endregion

        #region Constructor
        public KeepKeyManager(EnterPinArgs enterPinCallback, IHidDevice trezorHidDevice) : base(enterPinCallback, trezorHidDevice)
        {
        }
        #endregion

        #region Private Methods
        private CoinType GetCoinType(string coinShortcut)
        {
            if (!HasFeatures)
            {
                throw new Exception("The KeepKey has not been successfully initialised.");
            }

            return Features.Coins.FirstOrDefault(c => c.CoinShortcut == coinShortcut);
        }
        #endregion

        #region Protected Override Methods
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
        #endregion

        #region Public Methods
        /// <summary>
        /// Get the KeepKey's public key at the specified index.
        /// </summary>
        public async Task<PublicKey> GetPublicKeyAsync(string coinShortcut, uint addressNumber)
        {
            return await SendMessageAsync<PublicKey, GetPublicKey>(new GetPublicKey { AddressNs = new[] { addressNumber } });
        }
        #endregion

        #region Public Overrides
        /// <summary>
        /// Get an address from the KeepKey
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

                var path = ManagerHelpers.GetAddressPath(isSegwit.Value, account, isChange, index, coinNumber);

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
                Logger.Log("Error Getting KeepKey Address", ex, LogSection);
                throw;
            }
        }

        /// <summary>
        /// Initialize the KeepKey. Should only be called once.
        /// </summary>
        public override async Task InitializeAsync()
        {
            Features = await SendMessageAsync<Features, Initialize>(new Initialize());

            if (Features == null)
            {
                throw new Exception("Error initializing KeepKey. Features were not retrieved");
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

        protected override void CheckForFailure(object returnMessage)
        {
            if (returnMessage is Failure failure)
            {
                throw new FailureException<Failure>($"Error sending message to Trezor.\r\nCode: {failure.Code} Message: {failure.Message}", failure);
            }
        }

        protected override object GetEnumValue(string messageTypeString)
        {
            var isValid = Enum.TryParse(messageTypeString, out MessageType messageType);
            if (!isValid)
            {
                throw new Exception($"{messageTypeString} is not a valid MessageType");
            }

            return messageType;
        }
        #endregion
    }
}
