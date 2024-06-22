using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace GsCapture
{
    public class VideoEncoder
    {
        private Process ffmpegProcess;
        private Stream ffmpegInput;

        public void StartEncoding(string outputFilePath)
        {
            
            var startInfo = new ProcessStartInfo()
            {
                FileName = outputFilePath,
                Arguments = $"-y -f rawvideo -pix_fmt bgra -s 1920x1080 -r 30 -i - -c:v libx264 -pix_fmt yuv420p {outputFilePath}",
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true

            };

            ffmpegProcess = Process.Start(startInfo);
            ffmpegInput = ffmpegProcess.StandardInput.BaseStream;
        }

        public void WriteFrame(byte[] frameData)
        {
            ffmpegInput.Write(frameData, 0, frameData.Length);
        }

        public void StopEncoding()
        {
            ffmpegInput.Close();
            ffmpegProcess.WaitForExit();
        }
    }
}
