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
            new FilterDeviceDefinition{ DeviceType= DeviceType.Hid, VendorId= 0x2B24, ProductId=0x1, Label="KeepKey Hid"}
        };

        protected override KeepKeyManager CreateTrezorManager(IDevice device)
        {
            var asdasd = (dynamic) device;
            asdasd.DataHasExtraByte = false;

            return new KeepKeyManager(EnterPinArgs, device);
        }
        #endregion
    }
}
