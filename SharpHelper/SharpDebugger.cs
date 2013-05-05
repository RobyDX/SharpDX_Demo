using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;

namespace SharpHelper
{
    /// <summary>
    /// Debugger
    /// </summary>
    public class SharpDebugger
    {
        /// <summary>
        /// Debug device
        /// </summary>
        public DeviceDebug Debug { get; private set; }

        /// <summary>
        /// Info Queue
        /// </summary>
        public InfoQueue Queue { get; private set; }


        SharpDevice _device;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="breakOnWarning">Generate an error on warning</param>
        public SharpDebugger(SharpDevice device, bool breakOnWarning)
        {
            _device = device;
            //init the debug device
            Debug = new DeviceDebug(device.Device);
            //init the queue interface
            Queue = Debug.QueryInterface<InfoQueue>();

            if (breakOnWarning)
                Queue.SetBreakOnSeverity(MessageSeverity.Warning, true);

        }

        /// <summary>
        /// Get message stored inside debugger
        /// </summary>
        /// <param name="clearCache">Delete messages inside debugger</param>
        /// <returns>List of messages</returns>
        public List<Message> GetMessage(bool clearCache)
        {
            List<Message> messages = new List<Message>();
            for (int i = 0; i < Queue.NumStoredMessages; i++)
            {
                messages.Add(Queue.GetMessage(i));
            }
            if (clearCache)
                Queue.ClearStoredMessages();
            return messages;
        }

        /// <summary>
        /// Do a check on device context status
        /// </summary>
        public void Check()
        {
            Debug.ValidateContext(_device.DeviceContext);
            Debug.ValidateContextForDispatch(_device.DeviceContext);
        }

    }
}
