using System;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BackgroundAudio.Models;

namespace BackgroundAudio
{
    /// <summary>
    /// Hauptseite der Hintergrund-Audio-Player-Anwendung.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly MediaPlayer _mediaPlayer;
        private MediaPlaybackList _playbackList;
        private readonly Playlist _playlist;
        private bool _isPlaying;
        private readonly DispatcherTimer _progressTimer;
        private bool _isSeeking;

        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="MainPage"/>-Klasse.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            _playlist = new Playlist();

            _mediaPlayer = new MediaPlayer
            {
                AutoPlay = false
            };

            BuildPlaybackList();

            _mediaPlayer.Source = _playbackList;
            _mediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
            _playbackList.CurrentItemChanged += PlaybackList_CurrentItemChanged;

            var smtc = _mediaPlayer.SystemMediaTransportControls;
            smtc.IsEnabled = true;
            smtc.IsPlayEnabled = true;
            smtc.IsPauseEnabled = true;
            smtc.IsNextEnabled = true;
            smtc.IsPreviousEnabled = true;
            smtc.ButtonPressed += SMTC_ButtonPressed;

            PopulatePlaylistView();

            _progressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _progressTimer.Tick += Timer_Tick;

            UpdateTrackDisplay();

            Loaded += MainPage_Loaded;
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Seite geladen wurde.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Ereignisdaten.</param>
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            PlayButton.Focus(FocusState.Programmatic);
        }

        /// <summary>
        /// Erstellt die MediaPlaybackList aus den Playlist-Tracks.
        /// </summary>
        private void BuildPlaybackList()
        {
            _playbackList = new MediaPlaybackList { AutoRepeatEnabled = true };
            foreach (var track in _playlist.GetAllTracks())
            {
                var item = new MediaPlaybackItem(MediaSource.CreateFromUri(track.SourceUri));
                _playbackList.Items.Add(item);
            }
        }

        /// <summary>
        /// Füllt die PlaylistView mit den Tracks aus dem Playlist-Modell.
        /// </summary>
        private void PopulatePlaylistView()
        {
            PlaylistView.ItemsSource = _playlist.GetAllTracks();
        }

        /// <summary>
        /// Behandelt das Click-Ereignis der Play/Pause-Schaltfläche.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Ereignisdaten.</param>
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isPlaying)
            {
                _mediaPlayer.Pause();
            }
            else
            {
                _mediaPlayer.Play();
            }
        }

        /// <summary>
        /// Behandelt das Click-Ereignis der Zurück-Schaltfläche.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Ereignisdaten.</param>
        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            _playbackList.MovePrevious();
            if (_playbackList.CurrentItemIndex.HasValue)
            {
                _playlist.CurrentIndex = (int)_playbackList.CurrentItemIndex.Value;
            }
            UpdateTrackDisplay();
        }

        /// <summary>
        /// Behandelt das Click-Ereignis der Weiter-Schaltfläche.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Ereignisdaten.</param>
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            _playbackList.MoveNext();
            if (_playbackList.CurrentItemIndex.HasValue)
            {
                _playlist.CurrentIndex = (int)_playbackList.CurrentItemIndex.Value;
            }
            UpdateTrackDisplay();
        }

        /// <summary>
        /// Behandelt das Click-Ereignis der Shuffle-Schaltfläche.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Ereignisdaten.</param>
        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            _playlist.Shuffle();
            BuildPlaybackList();
            _mediaPlayer.Source = _playbackList;
            PopulatePlaylistView();
            UpdateTrackDisplay();
        }

        /// <summary>
        /// Behandelt das ItemClick-Ereignis der PlaylistView.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Ereignisdaten.</param>
        private void PlaylistView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Track track)
            {
                int index = _playlist.GetAllTracks().IndexOf(track);
                if (index >= 0)
                {
                    _playbackList.MoveTo((uint)index);
                    _playlist.CurrentIndex = index;
                    _mediaPlayer.Play();
                    UpdateTrackDisplay();
                }
            }
        }

        /// <summary>
        /// Behandelt das ManipulationStarted-Ereignis des ProgressSlider.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Ereignisdaten.</param>
        private void ProgressSlider_ManipulationStarted(object sender, Windows.UI.Xaml.Input.ManipulationStartedRoutedEventArgs e)
        {
            _isSeeking = true;
        }

        /// <summary>
        /// Behandelt das ManipulationCompleted-Ereignis des ProgressSlider.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Ereignisdaten.</param>
        private void ProgressSlider_ManipulationCompleted(object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
        {
            _isSeeking = false;
        }

        /// <summary>
        /// Behandelt das ValueChanged-Ereignis des ProgressSlider.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Ereignisdaten.</param>
        private void ProgressSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_isSeeking && _mediaPlayer != null && _mediaPlayer.PlaybackSession != null)
            {
                var duration = _mediaPlayer.PlaybackSession.NaturalDuration.TotalSeconds;
                if (duration > 0)
                {
                    _mediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(ProgressSlider.Value / 100.0 * duration);
                }
            }
        }

        /// <summary>
        /// Behandelt das Tick-Ereignis des Fortschrittstimmers.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Ereignisdaten.</param>
        private void Timer_Tick(object sender, object e)
        {
            UpdateProgress();
        }

        /// <summary>
        /// Behandelt das CurrentStateChanged-Ereignis des MediaPlayers.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Ereignisdaten.</param>
        private async void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SetPlayPauseState(sender.PlaybackSession.PlaybackState == MediaPlaybackState.Playing);
                if (_isPlaying)
                {
                    _progressTimer.Start();
                }
                else
                {
                    _progressTimer.Stop();
                }
            });
        }

        /// <summary>
        /// Behandelt das CurrentItemChanged-Ereignis der MediaPlaybackList.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Ereignisdaten.</param>
        private async void PlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (_playbackList.CurrentItemIndex.HasValue)
                {
                    _playlist.CurrentIndex = (int)_playbackList.CurrentItemIndex.Value;
                }
                UpdateTrackDisplay();
            });
        }

        /// <summary>
        /// Behandelt das ButtonPressed-Ereignis der SystemMediaTransportControls (Xbox Guide-Taste).
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="args">Ereignisdaten.</param>
        private async void SMTC_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                switch (args.Button)
                {
                    case SystemMediaTransportControlsButton.Play:
                        _mediaPlayer.Play();
                        break;
                    case SystemMediaTransportControlsButton.Pause:
                        _mediaPlayer.Pause();
                        break;
                    case SystemMediaTransportControlsButton.Next:
                        _playbackList.MoveNext();
                        if (_playbackList.CurrentItemIndex.HasValue)
                        {
                            _playlist.CurrentIndex = (int)_playbackList.CurrentItemIndex.Value;
                        }
                        UpdateTrackDisplay();
                        break;
                    case SystemMediaTransportControlsButton.Previous:
                        _playbackList.MovePrevious();
                        if (_playbackList.CurrentItemIndex.HasValue)
                        {
                            _playlist.CurrentIndex = (int)_playbackList.CurrentItemIndex.Value;
                        }
                        UpdateTrackDisplay();
                        break;
                }
            });
        }

        /// <summary>
        /// Aktualisiert die Anzeige des aktuellen Tracks.
        /// </summary>
        private void UpdateTrackDisplay()
        {
            var track = _playlist.CurrentTrack;
            if (track == null)
            {
                TrackTitle.Text = "Kein Track";
                TrackArtist.Text = "-";
                TotalTimeText.Text = "0:00";
                return;
            }

            TrackTitle.Text = track.Title;
            TrackArtist.Text = track.Artist;
            TotalTimeText.Text = FormatTime(track.Duration);

            if (track.AlbumArtUri != null)
            {
                AlbumArt.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(track.AlbumArtUri);
            }
            else
            {
                AlbumArt.Source = null;
            }

            PlaylistView.SelectedIndex = _playlist.CurrentIndex;
        }

        /// <summary>
        /// Aktualisiert den Fortschrittsbalken und die aktuelle Zeit.
        /// </summary>
        private void UpdateProgress()
        {
            if (_mediaPlayer?.PlaybackSession == null)
            {
                return;
            }

            var position = _mediaPlayer.PlaybackSession.Position;
            var duration = _mediaPlayer.PlaybackSession.NaturalDuration;

            if (!_isSeeking && duration.TotalSeconds > 0)
            {
                ProgressSlider.Value = position.TotalSeconds / duration.TotalSeconds * 100.0;
            }

            CurrentTimeText.Text = FormatTime(position);
        }

        /// <summary>
        /// Setzt den Wiedergabe/Pause-Zustand und aktualisiert die Schaltfläche.
        /// </summary>
        /// <param name="isPlaying">Gibt an, ob die Wiedergabe aktiv ist.</param>
        private void SetPlayPauseState(bool isPlaying)
        {
            _isPlaying = isPlaying;
            PlayButton.Content = _isPlaying ? "⏸" : "▶";
        }

        /// <summary>
        /// Formatiert eine Zeitspanne in das Format "m:ss".
        /// </summary>
        /// <param name="time">Die zu formatierende Zeitspanne.</param>
        /// <returns>Die formatierte Zeitangabe.</returns>
        private static string FormatTime(TimeSpan time)
        {
            if (time.TotalSeconds < 0)
            {
                time = TimeSpan.Zero;
            }
            return $"{(int)time.TotalMinutes}:{time.Seconds:D2}";
        }
    }
}
