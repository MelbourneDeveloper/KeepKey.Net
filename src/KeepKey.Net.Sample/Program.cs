using Hardwarewallets.Net.AddressManagement;
using Hid.Net.Windows;
using KeepKey.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        private static async Task<IHidDevice> Connect()
        {
            var devices = WindowsHidDevice.GetConnectedDeviceInformations();
            var keepKeyDeviceInformation = devices.FirstOrDefault(d => d.VendorId == KeepKeyManager.VendorId && d.ProductId == KeepKeyManager.ProductId);

            if (keepKeyDeviceInformation == null)
            {
                throw new Exception("No KeepKey is not connected or USB access was not granted to this application.");
            }

            var keepKeyHidDevice = new WindowsHidDevice(keepKeyDeviceInformation);

            keepKeyHidDevice.DataHasExtraByte = false;

            await keepKeyHidDevice.InitializeAsync();

            return keepKeyHidDevice;
        }

        /// <summary>
        /// TODO: This should be made in to a unit test but it's annoying to add the UI for a unit test as the KeepKey requires human intervention for the pin
        /// </summary>
        /// <returns></returns>
        private static async Task Go()
        {
            try
            {
                using (var keepKeyHid = await Connect())
                {
                    using (var keepKeyManager = new KeepKeyManager(GetPin, keepKeyHid))
                    {
                        await keepKeyManager.InitializeAsync();

                        var cointTable = await keepKeyManager.GetCoinTable();

                        keepKeyManager.CoinUtility = new KeepKeyCoinUtility(cointTable);

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

        private static async Task<string> GetPin()
        {
            Console.WriteLine("Enter PIN based on values: ");
            return Console.ReadLine().Trim();
        }
        #endregion
    }
}
