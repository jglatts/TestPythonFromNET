/*
    Form1.cs

    C# Windows Forms application for real-time camera capture and Python integration.

    - Captures video from the first available camera using AForge.NET.
    - Displays live feed in a PictureBox (liveFeedPixBox) and optionally cropped feed in mainFeedPixBox.
    - Sends every 5th frame to a Python process (infer.py) via standard input for processing.
    - Receives JSON results from Python via standard output, including overlay images in base64.
    - Converts Python overlays to Bitmap and displays them in the main PictureBox.
    - Logs errors and process info to a TextBox (txtBoxLogs).
    - Can be extended for real-time anomaly detection, object detection, or image analysis.
*/

using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Windows.Forms;

namespace TestPythonFromNET
{

    public partial class Form1 : Form
    {
        private Process pyProcess;
        private int FrameCount = 0;
        private long numPyFrames = 1;

        public Form1()
        {
            InitializeComponent();
            StartPythonProcess();
            InitCamera();
            this.FormClosing += Form1_FormClosing;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Stop your Python process if it's running
            if (pyProcess != null && !pyProcess.HasExited)
            {
                try
                {
                    pyProcess.Kill();
                    pyProcess.WaitForExit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error stopping Python process: " + ex.Message);
                }
            }
        }

        private void InitCamera()
        {
            VideoCaptureDevice videoSource;
            FilterInfoCollection videoDevices;
            
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count == 0)
                throw new ApplicationException("No camera found.");

            int dev_idx = videoDevices.Count > 1 ? 1 : 0;
            videoSource = new VideoCaptureDevice(videoDevices[dev_idx].MonikerString);
            videoSource.NewFrame += VideoSource_NewFrame;
            videoSource.Start();
        }

        private void UpdateLiveFeedImg(Bitmap frame)
        {
            liveFeedPixBox.Invoke(new Action(() =>
            {
                var old = liveFeedPixBox.Image;
                liveFeedPixBox.Image = (Bitmap)frame.Clone();
                old?.Dispose();
            }));
        }

        private void LogErrorText(string txt)
        {
            txtBoxLogs.Invoke(() =>
            {
                txtBoxLogs.Text += txt + "\n";
                txtBoxLogs.SelectionStart = txtBoxLogs.Text.Length;
                if (boxRawOutAutoScroll.Checked)
                    txtBoxLogs.ScrollToCaret();
            });
        }

        private void LogPyData(string txt)
        {
            txtBoxRecentStatus.Invoke(() =>
            {
                txtBoxRecentStatus.Text += txt + "\n";
                txtBoxRecentStatus.SelectionStart = txtBoxRecentStatus.Text.Length;
                if (boxPyOutAutoScroll.Checked)
                    txtBoxRecentStatus.ScrollToCaret();
            });
        }

        private bool CheckFrameCount()
        {
            FrameCount++;
            return (FrameCount % 5) == 0;
        }

        private void SendFrameToPython(Bitmap frame)
        {
            if (!CheckFrameCount())
                return;

            string tempPath = Path.Combine(Path.GetTempPath(), "frame.jpg");
            frame.Save(tempPath);
            try
            {
                if (pyProcess != null && !pyProcess.HasExited)
                {
                    pyProcess.StandardInput.WriteLine(tempPath);
                    pyProcess.StandardInput.Flush();
                }
                else if (pyProcess == null)
                {
                    LogErrorText("py process in null!");
                }
            }
            catch (Exception ex)
            {
                LogErrorText(ex.ToString());
            }

            frame.Dispose();       
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap frame = (Bitmap)eventArgs.Frame.Clone();
            frame = CropBitmap(frame, GetCropRect(frame, mainFeedPixBox));
            UpdateLiveFeedImg(frame);
            SendFrameToPython(frame);
        }

        private Rectangle GetCropRect(Bitmap frame, PictureBox box)
        {
            int cropWidth, cropHeight, x , y;
            
            float boxAspect = (float)box.Width / box.Height;
            float frameAspect = (float)frame.Width / frame.Height;

            if (frameAspect > boxAspect)
            {
                cropHeight = frame.Height;
                cropWidth = (int)(frame.Height * boxAspect);
            }
            else
            {
                cropWidth = frame.Width;
                cropHeight = (int)(frame.Width / boxAspect);
            }

            x = (frame.Width - cropWidth) / 2;
            y = (frame.Height - cropHeight) / 2;

            return new Rectangle(x, y, cropWidth, cropHeight);
        }

        private Bitmap CropBitmap(Bitmap src, Rectangle cropArea)
        {
            return src.Clone(cropArea, src.PixelFormat);
        }

        private void ImageFromPythonCallback(DataReceivedEventArgs e)
        {
            var result = JsonSerializer.Deserialize<PythonResult>(e.Data);
            if (result != null && result.overlay != null)
            {
                byte[] bytes = Convert.FromBase64String(result.overlay);
                using var ms = new MemoryStream(bytes);
                Bitmap bmp = new Bitmap(ms);
                mainFeedPixBox.Invoke(() =>
                {
                    mainFeedPixBox.Image?.Dispose();
                    mainFeedPixBox.Image = new Bitmap(bmp);
                    LogPyData($"Status: {result.status} | Score: {result.score:F3} | Frame: {numPyFrames++}");
                });
            }
        }

        private ProcessStartInfo CreatePyProcStartInfo()
        { 
            return new ProcessStartInfo
            {
                FileName = "python",
                Arguments = "infer_anomalib.py",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
        }

        private void PyOutputRecvCallback(object s, DataReceivedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Data)) 
                return;
            
            try
            {
                ImageFromPythonCallback(e);
            }
            catch
            {
                LogErrorText("[PY OUT] " + e.Data);
            }
        }

        private void PyErrorOutCallback(object s, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                LogErrorText("[PY ERR] " + e.Data);
            }
        }

        private void StartPythonProcess()
        {
            pyProcess = new Process();
            pyProcess.StartInfo = CreatePyProcStartInfo();

            // Handle python standard output
            pyProcess.OutputDataReceived += PyOutputRecvCallback;

            // Handle python standard error
            pyProcess.ErrorDataReceived += PyErrorOutCallback;

            // Start the python process
            pyProcess.Start();
            pyProcess.BeginOutputReadLine();
            pyProcess.BeginErrorReadLine();
        }

        private void btnRunPyScript_Click(object sender, EventArgs e)
        {
            MessageBox.Show("running!");
        }
    }

}
