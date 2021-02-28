using Device.Net;
using KeepKey.Net.Contracts;
using System;
using System.Collections.Generic;
using Trezor.Net;
using Trezor.Net.Manager;

namespace KeepKey.Net
{
    public class KeepKeyManagerBroker : TrezorManagerBrokerBase<KeepKeyManager, MessageType>, IDisposable
    {
        #region Constructor
        public KeepKeyManagerBroker(EnterPinArgs enterPinArgs, EnterPinArgs enterPassphraseArgs, int? pollInterval) : base(enterPinArgs, enterPassphraseArgs, pollInterval, null)
        {
        }
        #endregion

        #region Protected Overrides
        //Define the types of devices to search for. This particular device can be connected to via USB, or Hid
        public List<FilterDeviceDefinition> DeviceDefinitions { get; } = new List<FilterDeviceDefinition>
        {
            new FilterDeviceDefinition( vendorId: 0x2B24, productId:0x1, label:"Android Only USB Interface Legacy Firmware"),
            new FilterDeviceDefinition( vendorId: 0x2B24, productId:0x2, label:"Android Only USB Interface")
        };

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
