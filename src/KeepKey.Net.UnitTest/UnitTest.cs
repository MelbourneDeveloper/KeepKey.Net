using Hardwarewallets.Net;
using Hardwarewallets.Net.AddressManagement;
using KeepKey.Net.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NBitcoin;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Trezor.Net;

namespace KeepKey.Net
{
    [TestClass]
    public partial class UnitTest
    {
        #region Static Fields
        private static KeepKeyManager KeepKeyManager;
        private static readonly string[] _Addresses = new string[50];
        #endregion

        #region Initialization
        [TestInitialize]
        public async Task GetAndInitialize()
        {
            if (KeepKeyManager != null)
            {
                return;
            }

            KeepKeyManager = await ConnectAsync();
        }
        #endregion

        #region Helpers
        private static async Task<string> GetAddressAsync(uint index)
        {
            return await GetAddressAsync(0, false, index, false);
        }

        /// <summary>
        /// Note assumes we are looking for non segwit legacy addresses
        /// </summary>
        private static async Task<string> GetAddressAsync(uint coinNumber, bool isChange, uint index, bool display, bool isPublicKey = false, bool isLegacy = true)
        {
            var coinInfo = KeepKeyManager.CoinUtility.GetCoinInfo(coinNumber);
            var addressPath = new BIP44AddressPath(!isLegacy && coinInfo.IsSegwit, coinNumber, 0, isChange, index);
            var address = await KeepKeyManager.GetAddressAsync(addressPath, isPublicKey, display);
            Assert.IsNotNull(address);
            return address;
        }

        private static async Task DoGetAddress(uint i)
        {
            var address = await GetAddressAsync(i);
            _Addresses[i] = address;
        }
        #endregion

        #region Tests
        [TestMethod]
        public async Task DisplayBitcoinAddress()
        {
            var address = await GetAddressAsync(0, false, 0, true);
        }

        [TestMethod]
        public async Task GetBitcoinAddress()
        {
            var address = await GetAddressAsync(0, false, 0, false);
        }

        [TestMethod]
        public async Task GetBitcoinAddresses()
        {
            var addressManager = new AddressManager(KeepKeyManager, new BIP44AddressPathFactory(true, 0));

            //Get 10 addresses with all the trimming
            const int numberOfAddresses = 3;
            const int numberOfAccounts = 2;
            var addresses = await addressManager.GetAddressesAsync(0, numberOfAddresses, numberOfAccounts, true, true);

            Assert.IsTrue(addresses != null);
            Assert.IsTrue(addresses.Accounts != null);
            Assert.IsTrue(addresses.Accounts.Count == numberOfAccounts);
            Assert.IsTrue(addresses.Accounts[0].Addresses.Count == numberOfAddresses);
            Assert.IsTrue(addresses.Accounts[1].Addresses.Count == numberOfAddresses);
            Assert.IsTrue(addresses.Accounts[0].ChangeAddresses.Count == numberOfAddresses);
            Assert.IsTrue(addresses.Accounts[1].ChangeAddresses.Count == numberOfAddresses);
            Assert.IsTrue(addresses.Accounts[0].Addresses[0].PublicKey.Length > addresses.Accounts[0].Addresses[0].Address.Length);
        }

        [TestMethod]
        public async Task GetBitcoinCashAddress()
        {
            var address = await GetAddressAsync(145, false, 0, false);
        }

        [TestMethod]
        public async Task GetBitcoinGoldAddress()
        {
            var address = await GetAddressAsync(156, false, 0, false);
        }

        [TestMethod]
        public async Task GetLitecoinAddress()
        {
            var address = await GetAddressAsync(2, false, 0, false);
        }

        [TestMethod]
        public async Task GetDashAddress()
        {
            var address = await GetAddressAsync(5, false, 0, false);
        }

        [TestMethod]
        public async Task GetDogeAddress()
        {
            var address = await GetAddressAsync(3, false, 0, false);
        }

        [TestMethod]
        public async Task DisplayDogeAddress()
        {
            var address = await GetAddressAsync(3, false, 0, true);
        }

        [TestMethod]
        public async Task DisplayBitcoinCashAddress()
        {
            //Coin name must be specified when displaying the address for most coins
            var address = await GetAddressAsync(145, false, 0, true);
        }

        [TestMethod]
        public async Task DisplayEthereumAddress()
        {
            //Ethereum coins don't need the coin name
            var address = await GetAddressAsync(60, false, 0, true);
        }

        [TestMethod]
        public async Task GetEthereumAddress()
        {
            //Ethereum coins don't need the coin name
            var address = await GetAddressAsync(60, false, 0, false);
        }

        [TestMethod]
        public async Task DisplayEthereumClassicAddress()
        {
            //Ethereum coins don't need the coin name
            var address = await GetAddressAsync(61, false, 0, true);
        }

        [TestMethod]
        public async Task TestThreadSafety()
        {
            var tasks = new List<Task>();

            for (uint i = 0; i < 50; i++)
            {
                tasks.Add(DoGetAddress(i));
            }

            await Task.WhenAll(tasks);

            for (uint i = 0; i < 50; i++)
            {
                var address = await GetAddressAsync(i);

                Console.WriteLine($"Index: {i} (No change) - Address: {address}");

                if (address != _Addresses[i])
                {
                    throw new Exception("The ordering got messed up");
                }
            }
        }

        [TestMethod]
        public async Task SignEthereumTransaction()
        {
            var txMessage = new EthereumSignTx
            {
                Nonce = 10.ToBytesForRLPEncoding().ToHex().ToHexBytes(),
                GasPrice = 1000000000.ToBytesForRLPEncoding().ToHex().ToHexBytes(),
                GasLimit = 21000.ToBytesForRLPEncoding().ToHex().ToHexBytes(),
                To = "689c56aef474df92d44a1b70850f808488f9769c".ToHexBytes(),
                Value = BigInteger.Parse("10000000000000000000").ToBytesForRLPEncoding().ToHex().ToHexBytes(),
                AddressNs = KeyPath.Parse("m/44'/60'/0'/0/0").Indexes,
                ChainId = 1
            };
            var transaction = await KeepKeyManager.SendMessageAsync<EthereumTxRequest, EthereumSignTx>(txMessage);

            Assert.AreEqual(transaction.SignatureR.Length, 32);
            Assert.AreEqual(transaction.SignatureS.Length, 32);
        }

        [TestMethod]
        public async Task GetCoinTable()
        {
            var coinTables = new List<CoinInfo>();
            await GetAndInitialize();
            var coinTable = await KeepKeyManager.GetCoinTable();
            KeepKeyManager.CoinUtility = new KeepKeyCoinUtility(coinTable);
        }

        [TestMethod]
        public async Task SignBitcoinTransactionAsync()
        {
            // initialize connection with device
            await GetAndInitialize();

            //get address path for address in Trezor
            var addressPath = AddressPathBase.Parse<BIP44AddressPath>("m/49'/0'/0'/0/0").ToArray();

            // previous unspent input of Transaction
            var txInput = new TxInputType()
            {
                AddressNs = addressPath,
                Amount = 100837,
                ScriptType = InputScriptType.Spendp2shwitness,
                PrevHash = "797ad8727ee672123acfc7bcece06bf648d3833580b1b50246363f3293d9fe20".ToHexBytes(), // transaction ID
                PrevIndex = 0,
                Sequence = 4294967293 // Sequence  number represent Replace By Fee 4294967293 or leave empty for default 
            };

            // TX we want to make a payment
            var txOut = new TxOutputType()
            {
                AddressNs = new uint[0],
                Amount = 100837,
                Address = "3HN7CbEPY7FiuUKGq51g9e3UegFak1WZb5",
                ScriptType = OutputScriptType.Paytoaddress // if is segwit use Spendp2shwitness

            };

            // Must be filled with basic data like below
            var signTx = new SignTx()
            {
                Expiry = 0,
                LockTime = 0,
                CoinName = "Bitcoin",
                Version = 2,
                OutputsCount = 1,
                InputsCount = 1
            };

            // For every TX request from Trezor to us, we response with TxAck like below
            var txAck = new TxAck()
            {
                Tx = new TransactionType()
                {
                    Inputs = { txInput }, // Tx Inputs
                    Outputs = { txOut },   // Tx Outputs
                    Expiry = 0,
                    InputsCnt = 1, // must be exact number of Inputs count
                    OutputsCnt = 1, // must be exact number of Outputs count
                    Version = 2
                }
            };

            // If the field serialized.serialized_tx from Trezor is set,
            // it contains a chunk of the signed transaction in serialized format.
            // The chunks are returned in the right order and just concatenating all returned chunks will result in the signed transaction.
            // So we need to add chunks to the list
            var serializedTx = new List<byte>();

            // We send SignTx() to the Trezor and we wait him to send us Request
            var request = await KeepKeyManager.SendMessageAsync<TxRequest, SignTx>(signTx);

            // We do loop here since we need to send over and over the same transactions to trezor because his 64 kilobytes memory
            // and he will sign chunks and return part of signed chunk in serialized manner, until we receive finall type of Txrequest TxFinished
            while (request.RequestType != RequestType.Txfinished)
            {
                switch (request.RequestType)
                {
                    case RequestType.Txinput:
                        {
                            //We send TxAck() with  TxInputs
                            request = await KeepKeyManager.SendMessageAsync<TxRequest, TxAck>(txAck);

                            // Now we have to check every response is there any SerializedTx chunk 
                            if (request.Serialized != null)
                            {
                                // if there is any we add to our list bytes
                                serializedTx.AddRange(request.Serialized.SerializedTx);
                            }

                            break;
                        }
                    case RequestType.Txoutput:
                        {
                            //We send TxAck()  with  TxOutputs
                            request = await KeepKeyManager.SendMessageAsync<TxRequest, TxAck>(txAck);

                            // Now we have to check every response is there any SerializedTx chunk 
                            if (request.Serialized != null)
                            {
                                // if there is any we add to our list bytes
                                serializedTx.AddRange(request.Serialized.SerializedTx);
                            }

                            break;
                        }

                    case RequestType.Txextradata:
                        {
                            // for now he didn't ask me for extra data :)
                            break;
                        }
                    case RequestType.Txmeta:
                        {
                            // for now he didn't ask me for extra Tx meta data :)
                            break;
                        }
                }
            }

            Debug.WriteLine($"TxSignature: {serializedTx.ToArray().ToHexCompact()}");
        }
        #endregion
    }
}
