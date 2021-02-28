﻿using Device.Net;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Usb.Net.Windows;

#pragma warning disable CA2201 // Do not raise reserved exception types

namespace KeepKey.Net
{
    public partial class UnitTest
    {
        private async Task<KeepKeyManager> ConnectAsync()
        {
            //This only needs to be done once.
            //Register the factory for creating Usb devices. Trezor One Firmware 1.7.x / Trezor Model T
            WindowsUsbDeviceFactory.Register(new DebugLogger(), new DebugTracer());

#pragma warning disable CA2000 // Dispose objects before losing scope
            var keepKeyManagerBroker = new KeepKeyManagerBroker(GetPin, GetPassphrase, 2000);
#pragma warning restore CA2000 // Dispose objects before losing scope
            var keepKeyManager = await keepKeyManagerBroker.WaitForFirstTrezorAsync().ConfigureAwait(false);
            await keepKeyManager.InitializeAsync().ConfigureAwait(false);
            var coinTable = await keepKeyManager.GetCoinTable().ConfigureAwait(false);
            keepKeyManager.CoinUtility = new KeepKeyCoinUtility(coinTable);
            return keepKeyManager;
        }

        private async Task<string> GetPin() => await Prompt("Pin").ConfigureAwait(false);

        private async Task<string> GetPassphrase() => await Prompt("Passphrase").ConfigureAwait(false);

        private static async Task<string> Prompt(string prompt)
        {
            var passwordExePath = Path.Combine(GetExecutingAssemblyDirectoryPath(), "Misc", "GetPassword.exe");
            if (!File.Exists(passwordExePath))
            {
                throw new Exception($"The pin exe doesn't exist at passwordExePath {passwordExePath}");
            }

            var process = Process.Start(passwordExePath, prompt);
            process.WaitForExit();
            await Task.Delay(100).ConfigureAwait(false);
            var pin = File.ReadAllText(Path.Combine(GetExecutingAssemblyDirectoryPath(), "pin.txt"));
            return pin;
        }

        private static string GetExecutingAssemblyDirectoryPath()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            var executingAssemblyDirectoryPath = Path.GetDirectoryName(uri.Path);
            return executingAssemblyDirectoryPath;
        }
    }
}
