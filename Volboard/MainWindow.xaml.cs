using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VolBoard
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<Sound> sounds = new ObservableCollection<Sound>();

        public MainWindow()
        {
            InitializeComponent();

            SoundList.ItemsSource = sounds;
            SoundList.DataContext = this;

            List<SoundStore> sndStores = SerializeManager.Load<List<SoundStore>>();

            foreach (SoundStore sndStore in sndStores)
            {
                Sound snd = new Sound(sndStore.Path);
                snd.Loop = sndStore.Loop;
                snd.Volume = sndStore.Volume;
                snd.Key = sndStore.Key;
                sounds.Add(snd);
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!SearchBox.IsFocused)
            {
                if (e.Key == Key.Escape)
                {
                    foreach (Sound sound in sounds)
                    {
                        sound.Stop();
                    }
                }
                else
                {
                    foreach (Sound sound in sounds)
                    {
                        if (e.Key == sound.Key)
                        {
                            if (sound.Playing)
                            {
                                sound.Stop();
                            }
                            else
                            {
                                sound.Play();
                            }
                        }
                    }

                    SoundList.Items.Refresh();
                }
            }
        }

        private void BrowseForFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                Sound snd = new Sound(dlg.FileName);
                sounds.Add(snd);
                snd.MediaEnded += Snd_MediaEnded;
            }
        }

        private void Snd_MediaEnded(object sender, EventArgs e)
        {
            SoundList.Items.Refresh();
        }

        private Key? RequestKey()
        {
            Window1 w = new Window1();
            w.Owner = this;
            w.ShowDialog();

            return w.key;
        }

        private void RemoveSound(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            Sound mySound = (Sound)fe.DataContext;

            mySound.Stop();

            sounds.Remove(mySound);
        }

        private void ChangeBind(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            Sound mySound = (Sound)fe.DataContext;

            mySound.Key = RequestKey();

            SoundList.Items.Refresh();
        }

        private void LoopUnchecked(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            Sound mySound = (Sound)fe.DataContext;

            mySound.Loop = false;
        }

        private void LoopChecked(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            Sound mySound = (Sound)fe.DataContext;

            mySound.Loop = true;
        }

        private void UpdateVolume(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            Sound mySound = (Sound)fe.DataContext;

            Slider slider = sender as Slider;

            mySound.Volume = slider.Value;
        }

        private void TogglePlay(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            Sound sound = (Sound)fe.DataContext;
            
            if (sound.Playing)
            {
                sound.Stop();
            }
            else
            {
                sound.Play();
            }

            SoundList.Items.Refresh();
        }

        private void ClosingMain(object sender, CancelEventArgs e)
        {
            List<SoundStore> soundStores = new List<SoundStore>();

            foreach (Sound sound in sounds)
            {
                SoundStore sndStore = new SoundStore(sound.FilePath, sound.Loop, sound.Volume, sound.Key);
                soundStores.Add(sndStore);
            }

            SerializeManager.Save(soundStores);
        }

        private void Search(object sender, TextChangedEventArgs e)
        {
            TextBox search = sender as TextBox;
            if (search.Text == string.Empty)
            {
                SoundList.ItemsSource = sounds;
            }
            else
            {
                ObservableCollection<Sound> soundsShown = new ObservableCollection<Sound>();
                SoundList.ItemsSource = soundsShown;

                foreach (Sound sound in sounds)
                {
                    if (Regex.IsMatch(sound.Name, search.Text, RegexOptions.IgnoreCase)) // search exists in the name of the file
                    {
                        Debug.WriteLine("Found");
                        soundsShown.Add(sound);
                    }
                }
            }
        }
    }
}
