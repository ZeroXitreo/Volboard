using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace VolBoard
{
    public class Sound : MediaPlayer
    {
        private bool initiated = false;
        public string FilePath { get; }
        public string Name
        {
            get
            {
                return Path.GetFileNameWithoutExtension(FilePath);
            }
        }

        public bool Loop { get; set; } = false;

        public Sound(string path)
        {
            MediaEnded += SoundMediaEnded;
            FilePath = path;
            Volume = 1;
        }

        private void SoundMediaEnded(object sender, EventArgs e)
        {
            Stop();

            if (Loop)
            {
                Play();
            }
        }

        public new void Play()
        {
            if (!initiated)
            {
                Open(new Uri(FilePath));
                initiated = true;
            }
            base.Play();
            Playing = true;
        }

        public new void Stop()
        {
            base.Stop();
            Playing = false;
        }

        public Key? Key { get; internal set; } = null;
        public bool Playing { get; private set; } = false;
    }
}
