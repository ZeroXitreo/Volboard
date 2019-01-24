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

namespace Volboard
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Sound> sounds = new ObservableCollection<Sound>();
        private Sound selectedSound;

        private Sound SelectedSound
        {
            get
            {
                return selectedSound;
            }
            set
            {
                selectedSound = value;
                SoundList.SelectedItem = selectedSound;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            SoundList.ItemsSource = sounds;

            List<SoundStore> sndStores = SerializeManager.Load<List<SoundStore>>();
            foreach (SoundStore sndStore in sndStores)
            {
                sounds.Add(sndStore.Export());
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
            dlg.Multiselect = true;
            dlg.Title = "Browse for sounds or music";
            dlg.Filter = "Sound Files (*.mp3;*.wav)|*.mp3;*.wav";
            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                foreach (string file in dlg.FileNames)
                {
                    Sound snd = new Sound(file);
                    sounds.Add(snd);
                    SortSounds();
                }
            }
        }

        private void SortSounds()
        {
            sounds = new ObservableCollection<Sound>(sounds.OrderBy(o => o.Key == null).ThenBy(o => o.Key).ThenBy(o => o.Name));
            SoundList.ItemsSource = sounds;
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
            MessageBoxResult messageBoxResult = MessageBox.Show($"Are you sure you want to remove {SelectedSound.Name}?", "Remove Confirmation", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                SelectedSound.Stop();

                int index = sounds.IndexOf(SelectedSound);

                sounds.Remove(SelectedSound);

                if (sounds.Count > 0)
                {
                    if (sounds.Count - 1 < index)
                    {
                        SelectedSound = sounds[index - 1];
                    }
                    else
                    {
                        SelectedSound = sounds[index];
                    }
                }
            }
        }

        private void ChangeBind(object sender, RoutedEventArgs e)
        {
            SelectedSound.Key = RequestKey();
            
            SortSounds();
        }

        private void LoopUnchecked(object sender, RoutedEventArgs e)
        {
            if (SelectedSound != null)
            {
                SelectedSound.Loop = false;
            }
        }

        private void LoopChecked(object sender, RoutedEventArgs e)
        {
            if (SelectedSound != null)
            {
                SelectedSound.Loop = true;
            }
        }

        private void UpdateVolume(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SelectedSound != null)
            {
                SelectedSound.Volume = (sender as Slider).Value;
            }
        }

        private void TogglePlay(object sender, RoutedEventArgs e)
        {
            if (SelectedSound.Playing)
            {
                SelectedSound.Stop();
            }
            else
            {
                SelectedSound.Play();
            }
        }

        private void ClosingMain(object sender, CancelEventArgs e)
        {
            List<SoundStore> soundStores = new List<SoundStore>();

            foreach (Sound sound in sounds)
            {
                soundStores.Add(new SoundStore(sound));
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
                        soundsShown.Add(sound);
                    }
                }
            }
        }

        private void SoundList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var sound = (sender as ListBox).SelectedItem as Sound;

            SelectedSound = sound;

            RightPanel.DataContext = SelectedSound;
        }

        private void StartLatencyChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectedSound != null)
            {
                int.TryParse((sender as TextBox).Text, out int latencyChange);
                SelectedSound.PlayLatency = latencyChange;
            }
        }

        private void LoopLatencyChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectedSound != null)
            {
                int.TryParse((sender as TextBox).Text, out int latencyChange);
                SelectedSound.LoopLatency = latencyChange;
            }
        }
    }
}
