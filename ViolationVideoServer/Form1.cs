using Accord.Video.VFW;
using AForge.Video;
using br.com.ltb.Camera.Pumatronix;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ViolationVideoServer
{
    public partial class Form1 : Form
    {

        private MJPEGStream _frameVideo;
        private HTTPServer server;
        ConcurrentDictionary<DateTime, byte[]> imagens = new ConcurrentDictionary<DateTime, byte[]>();
        private ConcurrentQueue<byte[]> receivedFrames = new ConcurrentQueue<byte[]>();
        private ConcurrentQueue<Image> framesToShow = new ConcurrentQueue<Image>();
        private int maxArrayLength = 300; //3000 em um framerate de 10fps = 300s = 5min


        private int timeBefore = 1;
        private int timeAfter = 3;
        private int minNumOffFrames = 30;
        private int cameraFramerate = 10;
        private int quality = 70;

        private int currentMs = 0;

        private string olderFramesPath = Environment.CurrentDirectory + "/OlderFrames";
        private string outputVideos = Environment.CurrentDirectory + "/OutputVideos";

        List<ImageField.TypeOfField> desired = new List<ImageField.TypeOfField>() { ImageField.TypeOfField.Horario, ImageField.TypeOfField.TempoCaptura };

        protected bool running = false;

        ManualResetEvent mreDelay = new ManualResetEvent(false);


        public Form1()
        {
            InitializeComponent();
        }

        private void Server_Trigger(string message)
        {
            if (IsHandleCreated)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    try
                    {
                        var info = JsonConvert.DeserializeObject<TriggerInfo>(message);
                        textBox1.AppendText("Triggered from image " + info.ImageName + Environment.NewLine);
                        _ = SaveVideo(info);
                    }
                    catch (Exception ex)
                    {
                        textBox1.AppendText(ex.Message + Environment.NewLine);
                    }

                });
            }

        }

        private void Server_MessagesHandler(string message)
        {
            if (IsHandleCreated)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    textBox1.AppendText(message + Environment.NewLine);
                });
            }
        }


        private List<byte[]> FilterDict(DateTime startPosition, int offsetInSeconds)
        {
            List<byte[]> result = new List<byte[]>();

            DateTime endPosition = startPosition.AddSeconds(offsetInSeconds);

            var currentList = new List<KeyValuePair<DateTime, byte[]>>(imagens.ToArray());

            var filtered = currentList.Where(i => i.Key >= startPosition && i.Key <= endPosition).ToList();


            foreach (var i in filtered)
            {
                result.Add(i.Value);
            }

            return result;
        }

        private async Task SaveVideo(TriggerInfo inf)
        {

            Stopwatch stp = new Stopwatch();
            stp.Start();

            bool numFramesOK = false;
            DateTime startPosition = DateTime.Now.AddSeconds(timeBefore * -1);

            List<byte[]> frames = new List<byte[]>();


            //Debug.WriteLine($"Inicial: {startPosition.ToString()} - Ultimo frame {imagens.Keys.Last().ToString()}");
            //if (startPosition <= imagens.Keys.Last())
            //{
            int lastFramesCount = 0;
            do
            {
                frames = FilterDict(startPosition, (timeAfter + timeBefore));


                //if (frames.Count / cameraFramerate >= timeAfter + timeBefore)
                if (frames.Count != 0 && frames.Count == lastFramesCount)
                {
                    numFramesOK = true;
                }
                else
                {
                    lastFramesCount = frames.Count;
                }
                double timeToSleep = Math.Round((timeAfter + timeBefore) - (double)frames.Count / cameraFramerate, MidpointRounding.AwayFromZero);
                Thread.Sleep((int)timeToSleep * 1000);
            } while (!numFramesOK);
            //}
            //else
            //{
            //    frames = GetVideoFromFrameFiles(startPosition, startPosition.AddSeconds((timeAfter + timeBefore)));
            //}

            EncodingAndSave(frames, inf);

        }

        private List<byte[]> GetVideoFromFrameFiles(DateTime startPosition, DateTime endPosition)
        {
            List<byte[]> result = new List<byte[]>();

            string[] files = Directory.GetFiles(olderFramesPath);

            string startComparission = startPosition.ToString("yyyyMMddHHmmssfff");
            string endComparission = endPosition.ToString("yyyyMMddHHmmssfff");

            files = files.Where(f => Convert.ToInt32(f) >= Convert.ToInt32(startComparission)).ToArray();
            files = files.Where(f => Convert.ToInt32(f) <= Convert.ToInt32(endComparission)).ToArray();

            foreach (string s in files)
            {
                byte[] currentFrame = File.ReadAllBytes(s);
                result.Add(currentFrame);
            }

            return result;

        }

        private void EncodingAndSave(List<byte[]> frames, TriggerInfo inf)
        {
            if (!Directory.Exists(outputVideos))
            {
                Directory.CreateDirectory(outputVideos);
            }

            try
            {
                AVIWriter writer = new AVIWriter("mpg4");
                int realframerate = frames.Count / (timeBefore + timeAfter);
                textBox1.AppendText($"Real video framerate: {realframerate}{Environment.NewLine}");
                writer.FrameRate = realframerate;
                writer.Open($"{outputVideos}/{inf.ImageName}.avi", 1280, 960);
                foreach (var frame in frames)
                {
                    using (MemoryStream ms = new MemoryStream(frame))
                    {
                        Bitmap image = (Bitmap)Image.FromStream(ms);
                        writer.AddFrame(image);
                    }
                }
                writer.Close();
            }
            catch (Exception ex)
            {


            }



            Invoke((MethodInvoker)delegate
            {
                textBox1.AppendText($"Saved file {$"{inf.ImageName}.avi"}");
            });
        }

        private async void _frameVideo_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {

            using (var stream = new MemoryStream())
            {
                //framesToShow.Enqueue((Image)eventArgs.Frame.Clone());
                eventArgs.Frame.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                receivedFrames.Enqueue(stream.ToArray());

                FillingDictionary();
            }
        }

        private void FillingDictionary()
        {
            Stopwatch stp = new Stopwatch();

            _ = Task.Factory.StartNew(() =>
            {
                stp.Start();
                try
                {

                    while (receivedFrames.Count > 0)
                    {
                        stp.Restart();
                        byte[] currentImage;

                        if (receivedFrames.TryDequeue(out currentImage))
                        {
                            var data = new ImageData(currentImage, desired);
                            DateTime imgDateTime = DateTime.Parse(data.SortedFields[ImageField.TypeOfField.Horario]);

                            if (imagens.ContainsKey(imgDateTime) || imagens.ContainsKey(imgDateTime.AddMilliseconds(currentMs)))
                            {
                                currentMs = currentMs + 1000 / cameraFramerate;
                                if (currentMs > 1000)
                                {
                                    //pequena gambeta para evitar passar o segundo
                                    currentMs = currentMs - (1000 / cameraFramerate) + (1000 / (cameraFramerate * 2));
                                }

                                imgDateTime = imgDateTime.AddMilliseconds(currentMs);
                            }
                            else
                            {
                                currentMs = 0;
                            }

                            bool stop = false;
                            if (imagens.TryAdd(imgDateTime, currentImage))
                            {
                                if (imagens.Count > maxArrayLength)
                                {
                                    RemoveOlderFrame(false);
                                    stop = true;
                                }
                            }
                            stp.Stop();
                            if (stop)
                            {
                                var a = stp.ElapsedMilliseconds;
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                }
            });
        }


        private void RemoveOlderFrame(bool save = true)
        {
            if (save)
            {
                if (!Directory.Exists(olderFramesPath))
                {
                    Directory.CreateDirectory(olderFramesPath);
                }

                byte[] toSave = imagens[imagens.Keys.First()];

                File.WriteAllBytes($"{olderFramesPath}/{imagens.Keys.First().ToString("yyyyMMddHHmmssfff")}.jpg", toSave);
            }

            byte[] removedValue;
            imagens.TryRemove(imagens.Keys.First(), out removedValue);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (btnStart.Text == "Start")
            {
                running = true;
                server = new HTTPServer(51000);
                server.Messages += Server_MessagesHandler;
                server.Trigger += Server_Trigger;
                cameraFramerate = (int)nupframerate.Value;
                quality = (int)nupQuality.Value;
                timeBefore = (int)nupBefore.Value;
                timeAfter = (int)nupAfter.Value;
                var _videoPath = $"http://{tbxCameraIP.Text}/api/mjpegvideo.cgi?framerate={cameraFramerate}&quality={quality}";
                _frameVideo = new MJPEGStream(_videoPath);
                _frameVideo.Login = "admin";
                _frameVideo.Password = "1234";
                _frameVideo.NewFrame += _frameVideo_NewFrame;

                btnStart.Text = "Stop";
                _frameVideo.Start();
                server.Start();
                //_ = ShowVideo();
            }
            else
            {
                running = false;
                btnStart.Text = "Start";
                _frameVideo.Stop();
                server.Stop();
                server = null;                
                textBox1.AppendText("Process was aborted by user\r\n");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private async Task ShowVideo()
        {
            while (running)
            {
                Image currentImage;
                if (framesToShow.TryDequeue(out currentImage))
                {
                    Invoke((MethodInvoker)delegate
                    {

                        using (var srce = currentImage)
                        {
                            var dest = new Bitmap(pctVideo.Width, pctVideo.Height, PixelFormat.Format32bppPArgb);
                            using (var gr = Graphics.FromImage(dest))
                            {
                                gr.DrawImage(srce, new Rectangle(Point.Empty, dest.Size));
                            }
                            pctVideo.Image = dest;
                        }
                    });
                }
                mreDelay.WaitOne(50);
            }
        }
    }





public class TriggerInfo
{
    public string CameraDateTime { get; set; }
    public string SystemDateTime { get; set; }
    public string ImageName { get; set; }
    public string SerialNumber { get; set; }
    public long ElapsedTime { get; set; }
}
}
