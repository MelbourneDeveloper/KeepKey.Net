using Device.Net;
using Hardwarewallets.Net.AddressManagement;
using Hid.Net.Windows;
using KeepKey.Net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Usb.Net.Windows;

namespace KeepKeyTestApp
{
    internal class Program
    {
        #region Fields
        private static readonly string[] _Addresses = new string[50];
        #endregion

        #region Main
        private static void Main(string[] args)
        {
            try
            {
                Go();
                while (true) ;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }
        #endregion

        #region Private  Methods
        private static async Task<KeepKeyManager> ConnectAsync()
        {
            //This only needs to be done once.
            //Register the factory for creating Usb devices. Trezor One Firmware 1.7.x / Trezor Model T
            WindowsUsbDeviceFactory.Register(new DebugLogger(), new DebugTracer());

            //Register the factory for creating Hid devices. Trezor One Firmware 1.6.x
            WindowsHidDeviceFactory.Register(new DebugLogger(), new DebugTracer());

            var keepKeyManagerBroker = new KeepKeyManagerBroker(GetPin, GetPassphrase, 2000);
            var keepKeyManager = await keepKeyManagerBroker.WaitForFirstTrezorAsync();
            var coinTable = await keepKeyManager.GetCoinTable();
            keepKeyManager.CoinUtility = new KeepKeyCoinUtility(coinTable);
            return keepKeyManager;
        }

        /// <summary>
        /// TODO: This should be made in to a unit test but it's annoying to add the UI for a unit test as the KeepKey requires human intervention for the pin
        /// </summary>
        /// <returns></returns>
        private static async Task Go()
        {
            try
            {
                using (var keepKeyManager = await ConnectAsync())
                {
                    var tasks = new List<Task>();

                    for (uint i = 0; i < 50; i++)
                    {
                        tasks.Add(DoGetAddress(keepKeyManager, i));
                    }

                    await Task.WhenAll(tasks);

                    for (uint i = 0; i < 50; i++)
                    {
                        var address = await GetAddress(keepKeyManager, i);

                        Console.WriteLine($"Index: {i} (No change) - Address: {address}");

                        if (address != _Addresses[i])
                        {
                            throw new Exception("The ordering got messed up");
                        }
                    }

                    var addressPath = new BIP44AddressPath(false, 60, 0, false, 0);

                    var ethAddress = await keepKeyManager.GetAddressAsync(addressPath, false, false);
                    Console.WriteLine($"First ETH address: {ethAddress}");

                    Console.WriteLine("All good");

                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadLine();
            }
        }

        private static async Task DoGetAddress(KeepKeyManager keepKeyManager, uint i)
        {
            var address = await GetAddress(keepKeyManager, i);
            _Addresses[i] = address;
        }

        private static async Task<string> GetAddress(KeepKeyManager keepKeyManager, uint i)
        {
            return await keepKeyManager.GetAddressAsync(new BIP44AddressPath(true, 0, 0, false, i), false, false);
        }

        private static async Task<string> GetPassphrase()
        {
            Console.WriteLine("Enter passphrase: ");
            return Console.ReadLine().Trim();
        }

        private static async Task<string> GetPin()
        {
            Console.WriteLine("Enter PIN based on values: ");
            return Console.ReadLine().Trim();
        }
        #endregion
    }
}
