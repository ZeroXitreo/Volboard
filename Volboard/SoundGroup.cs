using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Volboard
{
    class SoundGroup
    {
        internal List<Sound> sounds = new List<Sound>();
        private Key? key = null;
        public bool Playing
        {
            get
            {
                return sounds.Any(o => o.Playing);
            }
        }

        public SoundGroup(List<Sound> sounds)
        {
            this.sounds = sounds;
        }

        public void Start()
        {
            Sound sound = sounds.First();
            sound.Play();
            sound.MediaEnded += StartNext;
        }

        private void StartNext(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            sounds.ForEach(sound => sound.Stop());
        }
    }
}
