using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace KeepKey.Net
{
    public partial class UnitTest
    {
        private async Task<KeepKeyManager> ConnectAsync()
        {
            using (var keepKeyManagerBroker = new KeepKeyManagerBroker(GetPin, 2000))
            {
                var keepKeyManager = await keepKeyManagerBroker.WaitForFirstTrezorAsync();
                await keepKeyManager.InitializeAsync();
                var coinTable = await keepKeyManager.GetCoinTable();
                keepKeyManager.CoinUtility = new KeepKeyCoinUtility(coinTable);
                return keepKeyManager;
            }
        }

        private async Task<string> GetPin()
        {
            var passwordExePath = Path.Combine(GetExecutingAssemblyDirectoryPath(), "Misc", "GetPassword.exe");
            if (!File.Exists(passwordExePath))
            {
                throw new Exception($"The pin exe doesn't exist at passwordExePath {passwordExePath}");
            }

            var process = Process.Start(passwordExePath);
            process.WaitForExit();
            await Task.Delay(100);
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
