using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MP4BoxWrapper.Models {
    public class Subtitle {
        /// <summary>
        /// Gets or sets the track number of this Subtitle
        /// </summary>
        public int TrackId { get; set; }

        /// <summary>
        /// Gets or sets the The language (if detected) of this Subtitle
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the Subtitle Type
        /// </summary>
        public string SubtitleType { get; set; }

        private const string TRACK_ID_PATTERN = "Info - TrackID ";
        private const string MEDIA_INFO_LANG_PATTERN = @"Media Info: Language """;
        private const string MEDIA_INFO_TYPE_PATTERN = @" - Type ""sbtl:";
        private const string END_DELIMITER = " - ";
        public static List<Subtitle> ParseList(string mp4BoxInfo) {
            StringReader reader = new StringReader(mp4BoxInfo);

            var subtitles = new List<Subtitle>();

            string trackID = string.Empty;
            string language = string.Empty;
            string type = string.Empty;

            string curLine = reader.ReadLine();
            while(curLine != null) {
                int offset = curLine.IndexOf(TRACK_ID_PATTERN);
                if (offset > -1) {
                    trackID = curLine.Substring(offset + TRACK_ID_PATTERN.Length);
                    trackID = trackID.Substring(0, trackID.IndexOf(END_DELIMITER));
                }

                offset = curLine.IndexOf(MEDIA_INFO_LANG_PATTERN);
                if (offset > -1) {
                    language = curLine.Substring(offset + MEDIA_INFO_LANG_PATTERN.Length);
                    language = language.Substring(0, language.IndexOf(" ("));

                    offset = curLine.IndexOf(MEDIA_INFO_TYPE_PATTERN);
                    if (offset > -1) {
                        type = curLine.Substring(offset + MEDIA_INFO_TYPE_PATTERN.Length);
                        type = "sbtl:" + type.Substring(0, type.IndexOf($"\"{END_DELIMITER}"));

                        Subtitle subtitle = new Subtitle() {
                            TrackId = int.Parse(trackID),
                            Language = language,
                            SubtitleType = type
                        };

                        subtitles.Add(subtitle);

                    }
                }

                curLine = reader.ReadLine();
            }

            return subtitles;
        }

        /// <summary>
        /// Override of the ToString method to make this object easier to use in the UI
        /// </summary>
        /// <returns>A string formatted as: {track #} {language}</returns>
        public override string ToString() {
            return $"Track ID: {TrackId}: {Language} {SubtitleType}";
        }
    }
}
