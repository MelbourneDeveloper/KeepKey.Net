using Hardwarewallets.Net.Model;
using Hid.Net;
using KeepKey.Net.Contracts;
using System;
using System.Collections.Generic;
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

        #region Public Constants
        public const ushort VendorId = 11044;
        public const ushort ProductId = 1;
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
        public KeepKeyManager(EnterPinArgs enterPinCallback, IHidDevice trezorHidDevice) : base(enterPinCallback, trezorHidDevice)
        {
        }

        public KeepKeyManager(EnterPinArgs enterPinCallback, IHidDevice trezorHidDevice, ICoinUtility coinUtility) : base(enterPinCallback, trezorHidDevice, coinUtility)
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
                                if (type.FullName == typeName)
                                {
                                    contractType = type;
                                    break;
                                }
                            }
                        }
                    }

                    if (contractType == null)
                    {
                        throw new Exception($"The device returned a message of {messageType}. There was no corresponding contract type at {typeName}");
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

        #region Public Overrides
        public override Task<string> GetAddressAsync(IAddressPath addressPath, bool isPublicKey, bool display)
        {
            if (CoinUtility == null)
            {
                throw new ManagerException($"A {nameof(CoinUtility)} must be specified if {nameof(AddressType)} is not specified.");
            }

            var coinInfo = CoinUtility.GetCoinInfo(addressPath.CoinType);

            return GetAddressAsync(addressPath, isPublicKey, display, coinInfo);
        }

        public Task<string> GetAddressAsync(IAddressPath addressPath, bool isPublicKey, bool display, CoinInfo coinInfo)
        {
            var inputScriptType = addressPath.Purpose == 49 ? InputScriptType.Spendp2shwitness : InputScriptType.Spendaddress;

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
                var path = addressPath.ToHardenedArray();

                if (isPublicKey)
                {
                    var publicKey = await SendMessageAsync<PublicKey, GetPublicKey>(new GetPublicKey { AddressNs = path, ShowDisplay = display });
                    return publicKey.Xpub;
                }
                else
                {
                    var isSegwit = addressPath.Purpose == 49;

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
                Logger.Log("Error Getting Trezor Address", ex, LogSection);
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
        #endregion
    }
}
