using System;
using System.Collections.Generic;

namespace BackgroundAudio.Models
{
    /// <summary>
    /// Verwaltet eine Wiedergabeliste von Audiotracks und stellt Navigationsfunktionen bereit.
    /// </summary>
    public sealed class Playlist
    {
        /// <summary>
        /// Die interne Liste der Tracks.
        /// </summary>
        private readonly List<Track> _tracks;

        /// <summary>
        /// Ruft den Index des aktuell ausgewählten Tracks ab oder legt diesen fest.
        /// </summary>
        public int CurrentIndex { get; set; }

        /// <summary>
        /// Ruft den aktuell ausgewählten Track ab.
        /// </summary>
        public Track CurrentTrack
        {
            get
            {
                if (_tracks == null || _tracks.Count == 0 || CurrentIndex < 0 || CurrentIndex >= _tracks.Count)
                {
                    return null;
                }
                return _tracks[CurrentIndex];
            }
        }

        /// <summary>
        /// Ruft die Anzahl der Tracks in der Wiedergabeliste ab.
        /// </summary>
        public int Count => _tracks.Count;

        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="Playlist"/>-Klasse und füllt sie mit Beispieltracks.
        /// </summary>
        public Playlist()
        {
            _tracks = new List<Track>();
            CurrentIndex = 0;
            PopulateSampleTracks();
        }

        /// <summary>
        /// Fügt der Wiedergabeliste einen Track hinzu.
        /// </summary>
        /// <param name="track">Der hinzuzufügende Track.</param>
        public void AddTrack(Track track)
        {
            if (track != null)
            {
                _tracks.Add(track);
            }
        }

        /// <summary>
        /// Wechselt zum nächsten Track in der Wiedergabeliste.
        /// </summary>
        /// <returns>Den nächsten Track oder null, wenn die Liste leer ist.</returns>
        public Track MoveNext()
        {
            if (_tracks.Count == 0)
            {
                return null;
            }
            CurrentIndex = (CurrentIndex + 1) % _tracks.Count;
            return CurrentTrack;
        }

        /// <summary>
        /// Wechselt zum vorherigen Track in der Wiedergabeliste.
        /// </summary>
        /// <returns>Den vorherigen Track oder null, wenn die Liste leer ist.</returns>
        public Track MovePrevious()
        {
            if (_tracks.Count == 0)
            {
                return null;
            }
            CurrentIndex = (CurrentIndex - 1 + _tracks.Count) % _tracks.Count;
            return CurrentTrack;
        }

        /// <summary>
        /// Ruft den Track am angegebenen Index ab.
        /// </summary>
        /// <param name="index">Der nullbasierte Index des Tracks.</param>
        /// <returns>Den Track am angegebenen Index.</returns>
        public Track GetTrackAt(int index)
        {
            if (index < 0 || index >= _tracks.Count)
            {
                return null;
            }
            return _tracks[index];
        }

        /// <summary>
        /// Mischt die Wiedergabeliste nach dem Fisher-Yates-Verfahren.
        /// </summary>
        public void Shuffle()
        {
            var random = new Random();
            int n = _tracks.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                Track temp = _tracks[k];
                _tracks[k] = _tracks[n];
                _tracks[n] = temp;
            }
            CurrentIndex = 0;
        }

        /// <summary>
        /// Ruft alle Tracks der Wiedergabeliste ab.
        /// </summary>
        /// <returns>Eine Liste aller Tracks.</returns>
        public IReadOnlyList<Track> GetAllTracks()
        {
            return _tracks.AsReadOnly();
        }

        /// <summary>
        /// Füllt die Wiedergabeliste mit Beispieltracks.
        /// </summary>
        private void PopulateSampleTracks()
        {
            AddTrack(new Track("Lo-Fi Beats", "XboxDev Radio", new Uri("https://example.com/audio/track1.mp3"), TimeSpan.FromSeconds(210)));
            AddTrack(new Track("Ambient Space", "Tutorial Beats", new Uri("https://example.com/audio/track2.mp3"), TimeSpan.FromSeconds(305)));
            AddTrack(new Track("Chillwave", "XboxDev Radio", new Uri("https://example.com/audio/track3.mp3"), TimeSpan.FromSeconds(185)));
            AddTrack(new Track("Synthwave Drive", "Neon Sounds", new Uri("https://example.com/audio/track4.mp3"), TimeSpan.FromSeconds(240)));
            AddTrack(new Track("Jazz Café", "Tutorial Beats", new Uri("https://example.com/audio/track5.mp3"), TimeSpan.FromSeconds(360)));
        }
    }
}
