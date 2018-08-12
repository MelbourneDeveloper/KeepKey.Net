using Hid.Net;
using KeepKey.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trezor.Net;

namespace KeepKeyTestApp
{
    class Program
    {
        #region Fields
        private static readonly string[] _Addresses = new string[50];
        #endregion

        #region Main
        static void Main(string[] args)
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
            var keepKeyDeviceInformation = devices.FirstOrDefault(d => d.VendorId == 11044 && d.ProductId == 1);

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
        private async static Task Go()
        {
            using (var keepKeyHid = await Connect())
            {
                using (var keepKeyManager = new KeepKeyManager(GetPin, keepKeyHid))
                {
                    await keepKeyManager.InitializeAsync();

                    var tasks = new List<Task>();

                    for (var i = 0; i < 50; i++)
                    {
                        tasks.Add(DoGetAddress(keepKeyManager, i));
                    }

                    await Task.WhenAll(tasks);

                    for (var i = 0; i < 50; i++)
                    {
                        var address = await GetAddress(keepKeyManager, i);

                        Console.WriteLine($"Index: {i} (No change) - Address: {address}");

                        if (address != _Addresses[i])
                        {
                            throw new Exception("The ordering got messed up");
                        }
                    }

                    Console.WriteLine("All good");

                    Console.ReadLine();
                }
            }
        }

        private async static Task DoGetAddress(KeepKeyManager keepKeyManager, int i)
        {
            var address = await GetAddress(keepKeyManager, i);
            _Addresses[i] = address;
        }

        private static async Task<string> GetAddress(KeepKeyManager keepKeyManager, int i)
        {
            return await keepKeyManager.GetAddressAsync("BTC", 0, 0, false, (uint)i, false, AddressType.Bitcoin, false);
        }

        private async static Task<string> GetPin()
        {
            Console.WriteLine("Enter PIN based on KeepKey values: ");
            return Console.ReadLine().Trim();
        }
        #endregion
    }
}
