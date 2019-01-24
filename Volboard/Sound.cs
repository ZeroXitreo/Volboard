using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace Volboard
{
    public class Sound : MediaPlayer, INotifyPropertyChanged
    {
        private int playLatency;
        private int loopLatency;
        private bool loop;
        private bool initiated = false;
        private Key? key = null;
        private bool playing = false;
        private Thread worker;
        private ManualResetEvent brake = new ManualResetEvent(false);

        public event PropertyChangedEventHandler PropertyChanged;
        
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Sound(string path)
        {
            FilePath = path;
            Volume = 1;

            MediaEnded += SoundMediaEnded;
        }
        
        public string FilePath { get; }

        public string Name
        {
            get
            {
                return Path.GetFileNameWithoutExtension(FilePath);
            }
        }

        public Key? Key
        {
            get
            {
                return key;
            }
            set
            {
                key = value;
                NotifyPropertyChanged();
            }
        }

        public bool Playing
        {
            get
            {
                return playing;
            }
            set
            {
                playing = value;
                NotifyPropertyChanged();
            }
        }

        public bool Loop
        {
            get
            {
                return loop;
            }
            set
            {
                loop = value;
                NotifyPropertyChanged();
            }
        }

        public int LoopLatency
        {
            get
            {
                return loopLatency;
            }
            set
            {
                loopLatency = value;
                NotifyPropertyChanged();
            }
        }

        public int PlayLatency
        {
            get
            {
                return playLatency;
            }
            set
            {
                playLatency = value;
                NotifyPropertyChanged();
            }
        }

        public new void Play()
        {
            brake.Reset();

            if (!initiated)
            {
                Open(new Uri(FilePath));
                initiated = true;
            }

            Playing = true;

            if (PlayLatency == 0)
            {
                base.Play();
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
                    base.Play();
                });
            }
        }

        public new void Stop()
        {
            Playing = false;
            base.Stop();
            brake.Set();
        }

        private void SoundMediaEnded(object sender, EventArgs e)
        {
            if (Loop && Playing)
            {
                base.Stop();

                if (LoopLatency == 0)
                {
                    base.Play();
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
                    base.Play();
                });
            }
        }
    }
}
