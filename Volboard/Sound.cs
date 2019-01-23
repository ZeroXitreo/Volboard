using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        public int LoopLatency { get; set; }
        public int PlayLatency { get; set; }

        private Thread worker;
        private ManualResetEvent brake = new ManualResetEvent(false);

        public Sound(string path)
        {
            FilePath = path;
            Volume = 1;

            MediaEnded += SoundMediaEnded;
        }

        public new void Play()
        {
            // TEST
            brake.Reset();

            if (!initiated)
            {
                Open(new Uri(FilePath));
                initiated = true;
            }

            Playing = true;

            if (PlayLatency == 0)
            {
                InternalPlay();
            }
            else
            {
                worker = new Thread(StartLatency);
                worker.IsBackground = true;
                worker.Start();
            }
        }
        
        private void StartLatency()
        {
            brake.WaitOne(PlayLatency);

            // Once done waiting, check if the file should still play
            if (Playing)
            {
                Dispatcher.Invoke(() =>
                {
                    InternalPlay();
                });
            }
        }

        public new void Stop()
        {
            Playing = false;
            InternalStop();

            // TEST
            brake.Set();
        }

        private void SoundMediaEnded(object sender, EventArgs e)
        {
            if (Loop && Playing)
            {
                InternalStop();

                if (LoopLatency == 0)
                {
                    InternalPlay();
                }
                else
                {
                    worker = new Thread(Repeat);
                    worker.IsBackground = true;
                    worker.Start();
                }
            }
            else
            {
                Stop();
            }
        }

        private void Repeat()
        {
            brake.WaitOne(LoopLatency);

            if (Loop && Playing)
            {
                Dispatcher.Invoke(() =>
                {
                    InternalPlay();
                });
            }
        }

        private void InternalStop()
        {
            base.Stop();
        }

        private void InternalPlay()
        {
            base.Play();
        }

        public Key? Key { get; internal set; } = null;
        public bool Playing { get; private set; } = false;
    }
}
