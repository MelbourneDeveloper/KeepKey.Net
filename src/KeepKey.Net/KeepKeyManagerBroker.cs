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
        public KeepKeyManagerBroker(EnterPinArgs enterPinArgs, int? pollInterval) : base(enterPinArgs, pollInterval, null)
        {
        }
        #endregion

        #region Protected Overrides
        //Define the types of devices to search for. This particular device can be connected to via USB, or Hid
        protected override List<FilterDeviceDefinition> DeviceDefinitions { get; } = new List<FilterDeviceDefinition>
        {
            new FilterDeviceDefinition{ DeviceType= DeviceType.Hid, VendorId= 0x2B24, ProductId=0x1, Label="KeepKey Hid Legacy Firmware"},
            new FilterDeviceDefinition{ DeviceType= DeviceType.Usb, VendorId= 0x2B24, ProductId=0x1, Label="Android Only USB Interface Legacy Firmware"},
            new FilterDeviceDefinition{ DeviceType= DeviceType.Hid, VendorId= 0x2B24, ProductId=0x2, Label="KeepKey Hid"},
            new FilterDeviceDefinition{ DeviceType= DeviceType.Usb, VendorId= 0x2B24, ProductId=0x2, Label="Android Only USB Interface"}
        };

        protected override KeepKeyManager CreateTrezorManager(IDevice device)
        {
            //TODO: This a hack for Hid.Net. This problem should go away when KeepKey switches over to USB instead of Hid
            var dataHasExtraByteProperty = device.GetType().GetProperty("DataHasExtraByte");
            if (dataHasExtraByteProperty != null) dataHasExtraByteProperty.SetValue(device, false);

            return new KeepKeyManager(EnterPinArgs, device);
        }
        #endregion
    }
}
