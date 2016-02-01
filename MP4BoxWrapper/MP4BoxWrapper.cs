using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MP4BoxWrapper.Models;

namespace MP4BoxWrapper
{
    public class BoxWrapper {
        private string ExeFile = @"MP4Box.exe";
        public bool RestoreSubtitles(string source, string destination) {
            // Verify that the destination video file does not have any subtitles already embedded within the file
            List<Subtitle> subTitles = GetSubtitles(destination);
            if (subTitles.Count > 0) {
                throw new ApplicationException("Destination video file already has embedded subtitles.");
            }

            // Get the list of individual .srt files extracted from the source video file
            subTitles = BackupSubtitles(source);
            if(subTitles.Count == 0) return false;

            // Get the collection of physical SRT files from the source folder
            FileInfo dirSource = new FileInfo(source);
            List<string> srtFiles = Directory.GetFiles(dirSource.DirectoryName, "*.srt").ToList<string>();

            if(subTitles.Count != srtFiles.Count) {
                throw new ApplicationException("Count of embedded subtitles from video file do not match count of extracted SRT files");
            }

            // Build the argument list for the MP4Box to add the SRT files to the optimized video
            StringBuilder arguments = new StringBuilder($"-add \"{destination}\" ");
            for (int index = 0; index < subTitles.Count; index++) {
                arguments.AppendFormat($"-add \"{srtFiles[index]}\":lang={subTitles[index].Language}:group=2:hdlr=\"{subTitles[index].SubtitleType}\" ");
            }
            arguments.AppendFormat($"-new \"{destination}\"");

            string response = ExecuteCommand(ExeFile, arguments.ToString(), false, false);

            // Clean-up the SRT files that were written out on the source folder
            //DeleteBackups(source);

            return true;
        }

        private List<Subtitle> GetSubtitles(string videoFile) {
            string arguments = $"-info {videoFile}";

            string response = ExecuteCommand(ExeFile, arguments);
            if(string.IsNullOrEmpty(response))
                return new List<Subtitle>();
            else
                return Subtitle.ParseList(response);
        }

        private List<Subtitle> BackupSubtitles(string videoFile) {
            // Step 1: Remove any .srt files in the directory of the video file
            DeleteBackups(videoFile);

            // Step 2: Read the info from the video file to determine if there are any subtitles embedded inside the video file
            List<Subtitle> subtitles = GetSubtitles(videoFile);
            if(subtitles.Count == 0) return subtitles;

            // Step 3: For each subtitle embedded inside the video file, export it as a text file
            foreach(Subtitle sub in subtitles) {
                ExecuteCommand(ExeFile, $"-srt {sub.TrackId} {videoFile}");
            }

            return subtitles;
        }

        private string ExecuteCommand(string command, string args, bool bWaitForExit = true, bool bReturnOutput = true) {
            try {

                ProcessStartInfo procStartInfo = new ProcessStartInfo(command, args);

                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.RedirectStandardError = true;
                procStartInfo.UseShellExecute = false;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;

                // Now we create a process, assign its ProcessStartInfo and start it
                Process proc = new Process();
                proc.StartInfo = procStartInfo;

                proc.Start();
                proc.WaitForExit(5000);

                // Get the output into a string
                string result = string.Empty;
                if(bReturnOutput) {
                    result = proc.StandardOutput.ReadToEnd();
                    if(string.IsNullOrWhiteSpace(result))
                        result = proc.StandardError.ReadToEnd();
                }

                // Display the command output.
                return result;
            } catch(Exception objException) {
                Debug.Assert(false, objException.Message);
                throw;
            }
        }

        private void DeleteBackups(string unOptimizedVideoFile) {
            FileInfo dir = new FileInfo(unOptimizedVideoFile);

            foreach(string srtFile in Directory.GetFiles(dir.DirectoryName, "*.srt")) {
                FileInfo fi = new FileInfo(srtFile);
                if(fi.Exists) {
                    fi.Delete();
                }
            }
        }
    }
}

