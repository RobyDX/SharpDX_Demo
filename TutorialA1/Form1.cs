using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpHelper.Audio;

namespace TutorialA1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //Init audio device
        SharpAudioDevice device = new SharpAudioDevice();
        SharpAudioVoice voice;

        private void btnLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog diag = new OpenFileDialog();
            diag.Filter = "Audio (wave)|*.wav";
            diag.InitialDirectory = Application.StartupPath;
            if (diag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                if (voice != null)
                    voice.Dispose();

                //load from file
                voice = new SharpAudioVoice(device, diag.FileName);
                btnPlay.Enabled = true;
                btnStop.Enabled = false;
            }

        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            //play
            voice.Play();
            btnPlay.Enabled = false;
            btnStop.Enabled = true;

            //on stop
            voice.Stopped += (v) =>
            {
                this.Invoke(new UpdateStatusInvoker(SetButton));
                
            };

        }

        delegate void UpdateStatusInvoker();

        private void SetButton()
        {
            btnPlay.Enabled = true;
            btnStop.Enabled = false;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            //stop
            voice.Stop();
            btnPlay.Enabled = true;
            btnStop.Enabled = false;
        }
    }
}
