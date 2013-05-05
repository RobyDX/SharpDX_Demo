using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.XAudio2;
using SharpDX.Multimedia;
using System.IO;
using System.Threading;

namespace SharpHelper.Audio
{

    /// <summary>
    /// Delegate for sound stop
    /// </summary>
    /// <param name="voice">Voice that generate event</param>
    public delegate void SoundStop(SharpAudioVoice voice);

    /// <summary>
    /// Audio Voice
    /// </summary>
    public class SharpAudioVoice : IDisposable
    {
        SourceVoice _voice;
        AudioBuffer _buffer;
        SoundStream _stream;
        Thread _checkThread;


        /// <summary>
        /// Voice
        /// </summary>
        public SourceVoice Voice
        {
            get { return _voice; }
        }

        /// <summary>
        /// Raise event when stopped
        /// </summary>
        public event SoundStop Stopped;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="filename">Filename</param>
        public SharpAudioVoice(SharpAudioDevice device, string filename)
        {
            _stream = new SoundStream(File.OpenRead(filename));

            var waveFormat = _stream.Format;
            _voice = new SourceVoice(device.Device, waveFormat);

            _buffer = new AudioBuffer
            {
                Stream = _stream.ToDataStream(),
                AudioBytes = (int)_stream.Length,
                Flags = BufferFlags.EndOfStream
            };

        }

        

        /// <summary>
        /// Play
        /// </summary>
        public void Play()
        {
            _voice.SubmitSourceBuffer(_buffer, _stream.DecodedPacketsInfo);
            _voice.Start();

            _checkThread = new Thread(new ThreadStart(Check));
            _checkThread.Start();

        }

        //check voice status
        private void Check()
        {
            try
            {
                while (Voice.State.BuffersQueued > 0)
                {
                    Thread.Sleep(10);
                }
                _voice.Stop();
                Stopped.Invoke(this);
            }
            catch 
            {
                
            }
        }

        /// <summary>
        /// Stop audio
        /// </summary>
        public void Stop()
        {
            _voice.Stop();
            _checkThread.Abort();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _voice.DestroyVoice();
            _voice.Dispose();
            _stream.Dispose();
            _buffer.Stream.Dispose();

        }
    }
}
