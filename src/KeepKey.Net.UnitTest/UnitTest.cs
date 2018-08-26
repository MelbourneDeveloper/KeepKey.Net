using KeepKey.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trezor.Net;

namespace KeepKey.Net
{
    [TestClass]
    public partial class UnitTest
    {
        private static KeepKeyManager KeepKeyManager;
        private static readonly string[] _Addresses = new string[50];

        [TestMethod]
        public async Task GetAddress()
        {
            await GetAndInitialize();
            var address = await GetAddress(0, true);
        }

        [TestMethod]
        public async Task TestThreadSafety()
        {
            await GetAndInitialize();

            var tasks = new List<Task>();

            for (var i = 0; i < 50; i++)
            {
                tasks.Add(DoGetAddress(KeepKeyManager, i));
            }

            await Task.WhenAll(tasks);

            for (var i = 0; i < 50; i++)
            {
                var address = await GetAddress(i, false);

                Console.WriteLine($"Index: {i} (No change) - Address: {address}");

                if (address != _Addresses[i])
                {
                    throw new Exception("The ordering got messed up");
                }
            }
        }

        private static async Task<string> GetAddress(int i, bool display)
        {
            return await KeepKeyManager.GetAddressAsync("BTC", 0, false, (uint)i, display, AddressType.Bitcoin);
        }

        private async Task GetAndInitialize()
        {
            if (KeepKeyManager != null)
            {
                return;
            }

            var keepKeyHidDevice = await Connect();
            KeepKeyManager = new KeepKeyManager(GetPin, keepKeyHidDevice);
            await KeepKeyManager.InitializeAsync();
        }

        private static async Task DoGetAddress(KeepKeyManager keepKeyManager, int i)
        {
            var address = await GetAddress(i, false);
            _Addresses[i] = address;
        }
    }
}
