using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace TestPythonFromNET
{

    public partial class Form1 : Form
    {
        private bool IsRunning = false;
        private int FrameCount = 0;
        private Process pyProcess;
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;

        public Form1()
        {
            InitializeComponent();
            StartPythonProcess();
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
                txtBoxLogs.ScrollToCaret();
            });
        }

        private void SendFrameToPython(Bitmap frame)
        {
            FrameCount++;
            if ((FrameCount % 5) != 0)
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

            LogErrorText("\nsent to python!\n");
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
            float boxAspect = (float)box.Width / box.Height;
            float frameAspect = (float)frame.Width / frame.Height;

            int cropWidth, cropHeight;

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

            int x = (frame.Width - cropWidth) / 2;
            int y = (frame.Height - cropHeight) / 2;

            return new Rectangle(x, y, cropWidth, cropHeight);
        }

        private Bitmap CropBitmap(Bitmap src, Rectangle cropArea)
        {
            return src.Clone(cropArea, src.PixelFormat);
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
                while (true)
                {
                    try
                    {
                        string line = pyProcess.StandardOutput.ReadLine();
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        var result = JsonSerializer.Deserialize<PythonResult>(line);
                        if (result != null && result.overlay != null)
                        {
                            byte[] bytes = Convert.FromBase64String(result.overlay);
                            using var ms = new MemoryStream(bytes);
                            Bitmap bmp = new Bitmap(ms);
                            mainFeedPixBox.Invoke(new Action(() =>
                            {
                                mainFeedPixBox.Image?.Dispose();
                                mainFeedPixBox.Image = new Bitmap(bmp);
                                //lblStatus.Text = $"Status: {result.status} | Score: {result.score:F3}";
                            }));
                        }
                    }
                    catch (Exception ex)
                    {
                        LogErrorText(ex.Message);
                    }
                }
            });
        }
        private void btnRunPyScript_Click(object sender, EventArgs e)
        {
            MessageBox.Show("running!");
        }
    }
    public class PythonResult
    {
        public string status { get; set; }
        public double score { get; set; }
        public string overlay { get; set; }
    }

}
