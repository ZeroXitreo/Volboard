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
        private Sound selectedSound;

        public MainWindow()
        {
            InitializeComponent();

            UpdateSoundPanel();

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
                }
            }
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
            MessageBoxResult messageBoxResult = MessageBox.Show($"Are you sure you want to remove {selectedSound.Name}?", "Remove Confirmation", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                selectedSound.Stop();
                sounds.Remove(selectedSound);
            }
        }

        private void ChangeBind(object sender, RoutedEventArgs e)
        {
            selectedSound.Key = RequestKey();

            UpdateSoundPanel();
        }

        private void LoopUnchecked(object sender, RoutedEventArgs e)
        {
            if (selectedSound != null)
            {
                selectedSound.Loop = false;
            }

            LoopLatency.IsEnabled = false;
        }

        private void LoopChecked(object sender, RoutedEventArgs e)
        {
            if (selectedSound != null)
            {
                selectedSound.Loop = true;
            }

            LoopLatency.IsEnabled = true;
        }

        private void UpdateVolume(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (selectedSound != null)
            {
                selectedSound.Volume = (sender as Slider).Value;
            }
        }

        private void TogglePlay(object sender, RoutedEventArgs e)
        {
            if (selectedSound.Playing)
            {
                selectedSound.Stop();
            }
            else
            {
                selectedSound.Play();
            }

            UpdateSoundPanel();
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
                        soundsShown.Add(sound);
                    }
                }
            }
        }

        private void SoundList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var sound = ((ListBox)sender).SelectedItem as Sound;

            selectedSound = sound;

            UpdateSoundPanel();
        }

        private void UpdateSoundPanel()
        {
            if (selectedSound == null)
            {
                ((FrameworkElement)PathBlock.Parent).IsEnabled = false;

                PathBlock.Text = string.Empty;
                PathBlock.ToolTip = string.Empty;

                BindButton.Content = "[Unbound]";

                PlayButton.Content = "Play";

                VolumeSlider.Value = VolumeSlider.Maximum;

                LoopCheckBox.IsChecked = false;

                return;
            }

            ((FrameworkElement)PathBlock.Parent).IsEnabled = true;

            PathBlock.Text = selectedSound.Name;
            PathBlock.ToolTip = selectedSound.FilePath;

            BindButton.Content = selectedSound.Key != null ? "Bound to " + selectedSound.Key.ToString() : "[Unbound]";

            PlayButton.Content = selectedSound.Playing ? "Stop" : "Play";

            VolumeSlider.Value = selectedSound.Volume;

            LoopCheckBox.IsChecked = selectedSound.Loop;

            LoopLatency.IsEnabled = LoopCheckBox.IsChecked ?? false;
            LoopLatency.Text = selectedSound.LoopLatency.ToString();

            StartLatency.Text = selectedSound.PlayLatency.ToString();
        }

        private void StartLatencyChanged(object sender, TextChangedEventArgs e)
        {
            int.TryParse((sender as TextBox).Text, out int latencyChange);
            selectedSound.PlayLatency = latencyChange;
        }

        private void LoopLatencyChanged(object sender, TextChangedEventArgs e)
        {
            int.TryParse((sender as TextBox).Text, out int latencyChange);
            selectedSound.LoopLatency = latencyChange;
        }
    }
}
