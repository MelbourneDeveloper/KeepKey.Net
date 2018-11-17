using Hardwarewallets.Net;
using Hardwarewallets.Net.AddressManagement;
using KeepKey.Net.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NBitcoin;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Trezor.Net;

namespace KeepKey.Net
{
    [TestClass]
    public partial class UnitTest
    {
        private static KeepKeyManager KeepKeyManager;
        private static readonly string[] _Addresses = new string[50];

        private static async Task<string> GetAddressAsync(uint index)
        {
            return await GetAddressAsync(0, false, index, false);
        }

        /// <summary>
        /// Note assumes we are looking for non segwit legacy addresses
        /// </summary>
        private static Task<string> GetAddressAsync(uint coinNumber, bool isChange, uint index, bool display, bool isPublicKey = false, bool isLegacy = true)
        {
            var coinInfo = KeepKeyManager.CoinUtility.GetCoinInfo(coinNumber);
            var addressPath = new BIP44AddressPath(!isLegacy && coinInfo.IsSegwit, coinNumber, 0, isChange, index);
            return KeepKeyManager.GetAddressAsync(addressPath, isPublicKey, display);
        }

        [TestMethod]
        public async Task DisplayBitcoinAddress()
        {
            await GetAndInitialize();
            var address = await GetAddressAsync(0, false, 0, true);
        }

        [TestMethod]
        public async Task GetBitcoinAddress()
        {
            await GetAndInitialize();
            var address = await GetAddressAsync(0, false, 0, false);
        }

        [TestMethod]
        public async Task GetBitcoinAddresses()
        {
            await GetAndInitialize();

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
            await GetAndInitialize();
            var address = await GetAddressAsync(145, false, 0, false);
        }

        [TestMethod]
        public async Task GetBitcoinGoldAddress()
        {
            await GetAndInitialize();
            var address = await GetAddressAsync(156, false, 0, false);
        }

        [TestMethod]
        public async Task GetLitecoinAddress()
        {
            await GetAndInitialize();
            var address = await GetAddressAsync(2, false, 0, false);
        }

        [TestMethod]
        public async Task GetDashAddress()
        {
            await GetAndInitialize();
            var address = await GetAddressAsync(5, false, 0, false);
        }

        [TestMethod]
        public async Task GetDogeAddress()
        {
            await GetAndInitialize();
            var address = await GetAddressAsync(3, false, 0, false);
        }

        [TestMethod]
        public async Task DisplayDogeAddress()
        {
            await GetAndInitialize();
            var address = await GetAddressAsync(3, false, 0, true);
        }

        [TestMethod]
        public async Task DisplayBitcoinCashAddress()
        {
            await GetAndInitialize();
            //Coin name must be specified when displaying the address for most coins
            var address = await GetAddressAsync(145, false, 0, true);
        }

        [TestMethod]
        public async Task DisplayEthereumAddress()
        {
            await GetAndInitialize();
            //Ethereum coins don't need the coin name
            var address = await GetAddressAsync(60, false, 0, true);
        }

        [TestMethod]
        public async Task GetEthereumAddress()
        {
            await GetAndInitialize();
            //Ethereum coins don't need the coin name
            var address = await GetAddressAsync(60, false, 0, false);
        }

        [TestMethod]
        public async Task DisplayEthereumClassicAddress()
        {
            await GetAndInitialize();
            //Ethereum coins don't need the coin name
            var address = await GetAddressAsync(61, false, 0, true);
        }

        [TestMethod]
        public async Task TestThreadSafety()
        {
            await GetAndInitialize();

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
            await GetAndInitialize();

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

        private async Task GetAndInitialize()
        {
            if (KeepKeyManager != null)
            {
                return;
            }

            var keepKeyDevice = await Connect();
            KeepKeyManager = new KeepKeyManager(GetPin, keepKeyDevice);
            await KeepKeyManager.InitializeAsync();
            var coinTable = await KeepKeyManager.GetCoinTable();
            KeepKeyManager.CoinUtility = new KeepKeyCoinUtility(coinTable);
        }

        private static async Task DoGetAddress(uint i)
        {
            var address = await GetAddressAsync(i);
            _Addresses[i] = address;
        }
    }
}
