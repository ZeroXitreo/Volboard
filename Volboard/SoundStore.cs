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
        public readonly string Path;
        public readonly bool Loop;
        public readonly double Volume;
        public readonly Key? Key;

        public SoundStore(string path, bool loop, double volume, Key? key)
        {
            Path = path;
            Loop = loop;
            Volume = volume;
            Key = key;
        }
    }
}
