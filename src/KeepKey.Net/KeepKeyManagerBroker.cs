using Device.Net;
using KeepKey.Net.Contracts;
using Microsoft.Extensions.Logging;
using System;
using Trezor.Net;
using Trezor.Net.Manager;

namespace KeepKey.Net
{
    public class KeepKeyManagerBroker : TrezorManagerBrokerBase<KeepKeyManager, MessageType>, IDisposable
    {
        #region Constructor
        public KeepKeyManagerBroker(
            EnterPinArgs enterPinArgs,
            EnterPinArgs enterPassphraseArgs,
            IDeviceFactory deviceFactory,
            ICoinUtility coinUtility = null,
            ILoggerFactory loggerFactory = null,
            int? pollInterval = null) : base(
                enterPinArgs,
                enterPassphraseArgs,
                pollInterval,
                deviceFactory,
                coinUtility,
                loggerFactory)
        {
        }
        #endregion

        #region Protected Overrides
        protected override KeepKeyManager CreateTrezorManager(IDevice device)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));

            //TODO: This a hack for Hid.Net. This problem should go away when KeepKey switches over to USB instead of Hid
            var dataHasExtraByteProperty = device.GetType().GetProperty("DataHasExtraByte");
            if (dataHasExtraByteProperty != null) dataHasExtraByteProperty.SetValue(device, false);

            return new KeepKeyManager(EnterPinArgs, EnterPassphraseArgs, device);
        }
        #endregion
    }
}
