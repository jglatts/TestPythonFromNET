using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;

namespace TestPythonFromNET
{
  
    public partial class Form1 : Form
    {
        private bool IsRunning = false;
        private Process pyProcess;
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;

        public Form1()
        {
            InitializeComponent();
            InitCamera();
        }


        private void InitCamera()
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count == 0)
                throw new ApplicationException("No camera found.");

            videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource.NewFrame += VideoSource_NewFrame;
            videoSource.Start();
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap frame = (Bitmap)eventArgs.Frame.Clone();

            liveFeedPixBox.Invoke(new Action(() =>
            {
                var old = liveFeedPixBox.Image;
                liveFeedPixBox.Image = (Bitmap)frame.Clone();
                old?.Dispose();
            }));

            // Save current frame to temp file to send to Python
            string tempPath = Path.Combine(Path.GetTempPath(), "frame.jpg");
            frame.Save(tempPath);
            if (pyProcess != null)
            {
                pyProcess.StandardInput.WriteLine(tempPath);
                pyProcess.StandardInput.Flush();
            }

            frame.Dispose(); // Dispose the clone we made
        }


        private void StartPythonProcess()
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = "infer.py",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            pyProcess = Process.Start(psi);

            Task.Run(() =>
            {
                while (!pyProcess.StandardOutput.EndOfStream)
                {
                    string line = pyProcess.StandardOutput.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var result = JsonSerializer.Deserialize<PythonResult>(line);
                    if (result.overlay != null)
                    {
                        byte[] bytes = Convert.FromBase64String(result.overlay);
                        using var ms = new MemoryStream(bytes);
                        Bitmap bmp = new Bitmap(ms);

                        // Update PictureBox in UI thread
                        mainFeedPixBox.Invoke(new Action(() =>
                        {
                            mainFeedPixBox.Image?.Dispose();
                            mainFeedPixBox.Image = new Bitmap(bmp);
                            //lblStatus.Text = $"Status: {result.status} | Score: {result.score:F3}";
                        }));
                    }
                }
            });
        }
        private void btnRunPyScript_Click(object sender, EventArgs e)
        {
            if (!IsRunning)
            {
                StartPythonProcess();
                IsRunning = true;
            }
            else
            {
                MessageBox.Show("running!");
            }
        }
    }
    public class PythonResult
    {
        public string status { get; set; }
        public double score { get; set; }
        public string overlay { get; set; }
    }

}
