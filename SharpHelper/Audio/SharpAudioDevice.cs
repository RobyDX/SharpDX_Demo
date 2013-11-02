using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.XAudio2;

namespace SharpHelper.Audio
{
    /// <summary>
    /// Audio Device
    /// </summary>
    public class SharpAudioDevice : IDisposable
    {
        XAudio2 _device;
        MasteringVoice _master;

        /// <summary>
        /// Audio Device
        /// </summary>
        public XAudio2 Device
        {
            get { return _device; }
        }

        /// <summary>
        /// Master voice
        /// </summary>
        public MasteringVoice Master
        {
            get { return _master; }
            set { _master = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SharpAudioDevice()
        {
            _device = new XAudio2();
            _master = new MasteringVoice(_device);
        }

        /// <summary>
        /// Release resource
        /// </summary>
        public void Dispose()
        {
            _master.Dispose();
            _device.Dispose();
        }
    }
}
