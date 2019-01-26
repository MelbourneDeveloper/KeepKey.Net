using Device.Net;
using KeepKey.Net;
using KeepKey.Net.Contracts;
using System.Collections.Generic;

namespace Trezor.Net.Manager
{
    public class KeepKeyManagerBroker : TrezorManagerBrokerBase<KeepKeyManager, MessageType>
    {
        #region Constructor
        public KeepKeyManagerBroker(EnterPinArgs enterPinArgs, int? pollInterval, ICoinUtility coinUtility) : base(enterPinArgs, pollInterval, coinUtility)
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
            return new KeepKeyManager(EnterPinArgs, device);
        }
        #endregion
    }
}
