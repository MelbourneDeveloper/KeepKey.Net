using Hid.Net;
using Trezor.Manager;

namespace KeepKey.Net
{
    public class KeepKeyManager : TrezorManager
    {
        public KeepKeyManager(EnterPinArgs enterPinCallback, IHidDevice trezorHidDevice) : base(enterPinCallback, trezorHidDevice)
        {
        }
    }
}
