using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VolBoard
{
    [Serializable]
    class SoundStore
    {
        private string filePath;
        private bool loop;
        private double volume;
        private Key? key;
        private int loopLatency;
        private int playLatency;

        public SoundStore(Sound sound)
        {
            filePath = sound.FilePath;
            loop = sound.Loop;
            volume = sound.Volume;
            key = sound.Key;
            loopLatency = sound.LoopLatency;
            playLatency = sound.PlayLatency;
        }

        public Sound Export()
        {
            Sound sound = new Sound(filePath)
            {
                Loop = loop,
                Volume = volume,
                Key = key,
                LoopLatency = loopLatency,
                PlayLatency = playLatency
            };
            return sound;
        }
    }
}
