using Hid.Net;
using KeepKey.Net.Contracts;
using System.Threading.Tasks;
using Trezor.Manager;

namespace KeepKey.Net
{
    public class KeepKeyManager : TrezorManager
    {
        public KeepKeyManager(EnterPinArgs enterPinCallback, IHidDevice trezorHidDevice) : base(enterPinCallback, trezorHidDevice)
        {
        }

        protected override async Task<object> PinMatrixAckAsync(string pin)
        {
            var retVal = await SendMessageAsync(new PinMatrixAck { Pin = pin });

            if (retVal is Failure failure)
            {
                throw new FailureException<Failure>("PIN Attempt Failed.", failure);
            }

            return retVal;
        }

        protected override async Task<object> ButtonAckAsync()
        {
            var retVal = await SendMessageAsync(new ButtonAck());

            if (retVal is Failure failure)
            {
                throw new FailureException<Failure>("PIN Attempt Failed.", failure);
            }

            return retVal;
        }
    }
}
