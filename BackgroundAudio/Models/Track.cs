using System;

namespace BackgroundAudio.Models
{
    /// <summary>
    /// Stellt einen Audiotitel mit Metadaten dar.
    /// </summary>
    public sealed class Track
    {
        /// <summary>
        /// Ruft den Titel des Tracks ab oder legt diesen fest.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Ruft den Interpreten des Tracks ab oder legt diesen fest.
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        /// Ruft die Quell-URI der Audiodatei ab oder legt diese fest.
        /// </summary>
        public Uri SourceUri { get; set; }

        /// <summary>
        /// Ruft die Gesamtdauer des Tracks ab oder legt diese fest.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Ruft die optionalen Albumcover-URI ab oder legt diese fest.
        /// </summary>
        public Uri AlbumArtUri { get; set; }

        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="Track"/>-Klasse.
        /// </summary>
        /// <param name="title">Der Titel des Tracks.</param>
        /// <param name="artist">Der Interpret des Tracks.</param>
        /// <param name="sourceUri">Die Quell-URI der Audiodatei.</param>
        /// <param name="duration">Die Gesamtdauer des Tracks.</param>
        public Track(string title, string artist, Uri sourceUri, TimeSpan duration)
        {
            Title = title;
            Artist = artist;
            SourceUri = sourceUri;
            Duration = duration;
        }
    }
}
