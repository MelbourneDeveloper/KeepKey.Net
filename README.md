# KeepKey.Net
Cross Platform C# Library for the KeepKey Cryptocurrency Hardwarewallet

## Quick Start

- Clone the repo and open the solution
- There is a console sample, Xamarin Forms sample and unit tests
- Compile one of the unit test apps, run the UWP/Android Xamarin Forms apps or,
- Go to Test->Windows->Text Explorer in Visual Studio
- Run one of the unit tests in the pane.

All KeepKey messages are in the KeepKey.Net.Contracts namespace. To implement them, you need to call SendMessageAsync

NuGet: Install-Package KeepKey.Net

[Example](https://github.com/MelbourneDeveloper/KeepKey.Net/blob/a7911dd0f6f37dd4eb008a7320a0c786c90dfb37/src/KeepKey.Net.UnitTest/UnitTest.cs#L39):
````cs
private static async Task<string> GetAddressAsync(uint coinNumber, bool isChange, uint index, bool display, bool isPublicKey = false, bool isLegacy = true)
{
    WindowsUsbDeviceFactory.Register();
    WindowsHidDeviceFactory.Register();
    var keepKeyManagerBroker = new KeepKeyManagerBroker(GetPin, GetPassphrase, 2000);
    var keepKeyManager = await keepKeyManagerBroker.WaitForFirstTrezorAsync();
    await keepKeyManager.InitializeAsync();
    var coinTable = await keepKeyManager.GetCoinTable();
    keepKeyManager.CoinUtility = new KeepKeyCoinUtility(coinTable);
    var coinInfo = KeepKeyManager.CoinUtility.GetCoinInfo(coinNumber);
    var addressPath = new BIP44AddressPath(!isLegacy && coinInfo.IsSegwit, coinNumber, 0, isChange, index);
    var address = await KeepKeyManager.GetAddressAsync(addressPath, isPublicKey, display);
    return address;
}
````
## Contact

- Join us on [Slack](https://join.slack.com/t/hardwarewallets/shared_invite/enQtNjA5MDgxMzE2Nzg2LWUyODIzY2U0ODE5OTFlMmI3MGYzY2VkZGJjNTc0OTUwNDliMTg2MzRiNTU1MTVjZjI0YWVhNjQzNjUwMjEyNzQ)
- PM me on [Twitter](https://twitter.com/cfdevelop)
- Blog: https://christianfindlay.com/

## [Contribution](https://github.com/MelbourneDeveloper/KeepKey.Net/blob/master/CONTRIBUTING.md)

The community needs your help! Unit tests, integration tests, more app integrations and bug fixes please! Check out the Issues section.

## Donate

All my libraries are open source and free. Your donations will contribute to making sure that these libraries keep up with the latest firmware, functions are implemented, and the quality is maintained.

| Coin           | Address |
| -------------  |:-------------:|
| Bitcoin        | [33LrG1p81kdzNUHoCnsYGj6EHRprTKWu3U](https://www.blockchain.com/btc/address/33LrG1p81kdzNUHoCnsYGj6EHRprTKWu3U) |
| Ethereum       | [0x7ba0ea9975ac0efb5319886a287dcf5eecd3038e](https://etherdonation.com/d?to=0x7ba0ea9975ac0efb5319886a287dcf5eecd3038e) |

## Based On

| Library           | Description |
| -------------  |:-------------:|
| [Trezor.Net](https://github.com/MelbourneDeveloper/Trezor.Net)                   | Trezor Hardwarewallet Library. KeepKey's and Trezor's protocol are very similar |
| [Hardwarewallets.Net](https://github.com/MelbourneDeveloper/Hardwarewallets.Net) | This library is part of the Hardwarewallets.Net suite. It is aimed toward putting a set of common C# interfaces, and utilities that will work with all hardwarewallets. |
| [Hid.Net, Usb.Net](https://github.com/MelbourneDeveloper/Device.Net)             | Trezor.Net communicates with the devices via the Hid.Net and Usb.Net libraries. You can see the repo for this library here. |

## See Also

| Library           | Description |
| -------------  |:-------------:|
| [Trezor.Net](https://github.com/MelbourneDeveloper/Trezor.Net)                   | Trezor Hardwarewallet Library |
| [Ledger.Net](https://github.com/MelbourneDeveloper/Ledger.Net)                   | Ledger Hardwarewallet Library |
| [Ledger .NET API](https://github.com/LedgerHQ/ledger-dotnet-api)                 | A similar Ledger library |
| [Ledger Bitcoin App](https://github.com/LedgerHQ/blue-app-btc)                   | Bitcoin wallet application for Ledger Blue and Nano S |
| [Ledger Ethereum App](https://github.com/LedgerHQ/blue-app-eth)                  | Ethereum wallet application for Ledger Blue and Nano S |

## Hardfolio - Store App Production Usage

https://play.google.com/store/apps/details?id=com.Hardfolio (Android)

https://www.microsoft.com/en-au/p/hardfolio/9p8xx70n5d2j (UWP)