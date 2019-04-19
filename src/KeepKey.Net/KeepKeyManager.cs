using Device.Net;
using Hardwarewallets.Net.Model;
using KeepKey.Net.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Trezor.Net;

namespace KeepKey.Net
{
    /// <summary>
    /// Manages communication with the KeepKey device
    /// </summary>
    public class KeepKeyManager : TrezorManagerBase<MessageType>
    {
        #region Private Constants
        private readonly string LogSection = nameof(KeepKeyManager);
        #endregion

        #region Private Static Fields
        private static Assembly[] _Assemblies;
        private static readonly Dictionary<string, Type> _ContractsByName = new Dictionary<string, Type>();
        #endregion

        #region Public Properties
        public override bool IsInitialized => Features != null;
        public Features Features { get; private set; }
        #endregion

        #region Protected Properties
        protected override string ContractNamespace => "KeepKey.Net.Contracts";
        protected override Type MessageTypeType => typeof(MessageType);
        #endregion

        #region Constructor
        public KeepKeyManager(EnterPinArgs enterPinCallback, IDevice keepKeyDevice) : base(enterPinCallback, keepKeyDevice)
        {
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

        protected override Type GetContractType(MessageType messageType, string typeName)
        {
            Type contractType;

            lock (_ContractsByName)
            {
                if (!_ContractsByName.TryGetValue(typeName, out contractType))
                {
                    contractType = Type.GetType(typeName);

                    if (contractType == null)
                    {
                        if (_Assemblies == null)
                        {
                            _Assemblies = AppDomain.CurrentDomain.GetAssemblies();
                        }

                        foreach (var assembly in _Assemblies)
                        {
                            foreach (var type in assembly.GetTypes())
                            {
                                if (string.Equals(type.FullName, typeName, StringComparison.Ordinal))
                                {
                                    contractType = type;
                                    break;
                                }
                            }
                        }
                    }

                    if (contractType == null)
                    {
                        throw new ManagerException($"The device returned a message of {messageType}. There was no corresponding contract type at {typeName}");
                    }
                    else
                    {
                        _ContractsByName.Add(typeName, contractType);
                    }
                }
            }

            return contractType;
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
                throw new FailureException<Failure>($"Error sending message to KeepKey.\r\nCode: {failure.Code} Message: {failure.Message}", failure);
            }
        }

        protected override object GetEnumValue(string messageTypeString)
        {
            var isValid = Enum.TryParse(messageTypeString, out MessageType messageType);
            if (!isValid)
            {
                throw new ManagerException($"{messageTypeString} is not a valid MessageType");
            }

            return messageType;
        }
        #endregion

        #region Public Overrides
        public override Task<string> GetAddressAsync(IAddressPath addressPath, bool isPublicKey, bool display)
        {
            if (addressPath == null) throw new ArgumentNullException(nameof(addressPath));

            if (CoinUtility == null)
            {
                throw new ManagerException($"A {nameof(CoinUtility)} must be specified if {nameof(AddressType)} is not specified.");
            }

            var cointType = addressPath.AddressPathElements.Count > 1 ? addressPath.AddressPathElements[1].Value : throw new ManagerException("The first element of the address path is considered to be the coin type. This was not specified so no coin information is available. Please use an overload that specifies CoinInfo.");

            var coinInfo = CoinUtility.GetCoinInfo(cointType);

            return GetAddressAsync(addressPath, isPublicKey, display, coinInfo);
        }

        public Task<string> GetAddressAsync(IAddressPath addressPath, bool isPublicKey, bool display, CoinInfo coinInfo)
        {
            if (coinInfo == null) throw new ArgumentNullException(nameof(coinInfo));

            var inputScriptType = coinInfo.IsSegwit ? InputScriptType.Spendp2shwitness : InputScriptType.Spendaddress;

            return GetAddressAsync(addressPath, isPublicKey, display, coinInfo.AddressType, inputScriptType, coinInfo.CoinName);
        }

        public Task<string> GetAddressAsync(IAddressPath addressPath, bool isPublicKey, bool display, AddressType addressType, InputScriptType inputScriptType)
        {
            return GetAddressAsync(addressPath, isPublicKey, display, addressType, inputScriptType, null);
        }

        public async Task<string> GetAddressAsync(IAddressPath addressPath, bool isPublicKey, bool display, AddressType addressType, InputScriptType inputScriptType, string coinName)
        {
            try
            {
                if (addressPath == null) throw new ArgumentNullException(nameof(addressPath));

                var path = addressPath.ToArray();

                if (isPublicKey)
                {
                    var publicKey = await SendMessageAsync<PublicKey, GetPublicKey>(new GetPublicKey { AddressNs = path, ShowDisplay = display, ScriptType = inputScriptType });
                    return publicKey.Xpub;
                }
                else
                {
                    switch (addressType)
                    {
                        case AddressType.Bitcoin:

                            return (await SendMessageAsync<Address, GetAddress>(new GetAddress { ShowDisplay = display, AddressNs = path, CoinName = coinName, ScriptType = inputScriptType })).address;

                        case AddressType.Ethereum:

                            var ethereumAddress = await SendMessageAsync<EthereumAddress, EthereumGetAddress>(new EthereumGetAddress { ShowDisplay = display, AddressNs = path });

                            var sb = new StringBuilder();
                            foreach (var b in ethereumAddress.Address)
                            {
                                sb.Append(b.ToString("X2").ToLower());
                            }

                            var hexString = sb.ToString();

                            return $"0x{hexString}";
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error Getting KeepKey Address", LogSection, ex, LogLevel.Error);
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
                throw new ManagerException("Error initializing KeepKey. Features were not retrieved");
            }
        }
        #endregion

        #region Public Methods
        public async Task<IEnumerable<CoinType>> GetCoinTable()
        {
            var coinInfos = new List<CoinType>();
            var coinTable = await SendMessageAsync<CoinTable, GetCoinTable>(new GetCoinTable { });

            for (uint i = 0; i < coinTable.NumCoins; i++)
            {
                coinTable = await SendMessageAsync<CoinTable, GetCoinTable>(new GetCoinTable { Start = i, End = i + 1 });
                var coinType = coinTable.Tables.First();
                coinInfos.Add(coinType);
            }

            return coinInfos;
        }
        #endregion
    }
}
