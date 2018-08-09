# KeepKey.Net

## Getting Started

- Clone the repo and open the solution
- There is a sample/unit test app there called KeepKey.Net.Sample
- This sample is a .NET Core console app and shows you how to get addresses

Note: not all messages have a public method, but all messages exist. If you want to send a message to the KeepKey you need to construct the message object and send it to the KeepKey with the SendMessageAsync method. This requires that you know the result type before calling.

## NuGet

Install-Package KeepKey.Net

## Suported Platforms

- .NET Framework
- .NET Core
- Android
- UWP 

## More Samples

More samples are coming. This library will soon be used for KeepKey support in Hardfolio

Windows Store
https://www.microsoft.com/en-au/p/hardfolio/9p8xx70n5d2j

Google Play
https://play.google.com/store/apps/details?id=com.Hardfolio

## Trezor.Net & Hid.Net

KeepKey.Net is based on Trezor.Net which is based on Hid.Net. You can see the repo for this library here:

https://github.com/MelbourneDeveloper/Trezor.Net

https://github.com/MelbourneDeveloper/Hid.Net

## Contribution

Contribution is welcome. Please fork, tighten up the code (real tight), test, and submit a pull request.


