using Accord.Video.VFW;
using AForge.Video;
using br.com.ltb.Camera.Pumatronix;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ViolationVideoServer
{
    public partial class Form1 : Form
    {

        private MJPEGStream _frameVideo;
        private HTTPServer server;
        Dictionary<DateTime, byte[]> imagens = new Dictionary<DateTime, byte[]>();
        private int maxArrayLength = 3000; //em um framerate de 10fps = 300s = 5min


        private int timeBefore = 1;
        private int timeAfter = 3;
        private int minNumOffFrames = 30;
        private int cameraFramerate = 10;

        private int currentMs = 0;

        private string olderFramesPath = Application.StartupPath + "/OlderFrames";
        private string outputVideos = Application.StartupPath + "/OutputVideos";




        public Form1()
        {
            InitializeComponent();
            server = new HTTPServer(51000);
            server.Messages += Server_MessagesHandler;
            server.Trigger += Server_Trigger;
            var _videoPath = "http://" + "192.168.50.172" + $"/api/mjpegvideo.cgi?framerate={cameraFramerate}";
            _frameVideo = new MJPEGStream(_videoPath);
            _frameVideo.Login = "admin";
            _frameVideo.Password = "1234";
            _frameVideo.NewFrame += _frameVideo_NewFrame;
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


            var filtered = imagens.Where(i => i.Key >= startPosition && i.Key <= endPosition);


            foreach (var i in filtered)
            {
                result.Add(i.Value);
            }

            return result;
        }

        private async Task SaveVideo(TriggerInfo inf)
        {
            _ = Task.Factory.StartNew(() =>
            {
                Stopwatch stp = new Stopwatch();
                stp.Start();

                bool numFramesOK = false;
                DateTime startPosition = DateTime.Now.AddSeconds(timeBefore * -1);

                List<byte[]> frames = new List<byte[]>();


                if (startPosition <= imagens.Keys.Last())
                {
                    do
                    {
                        frames = FilterDict(startPosition, (timeAfter + timeBefore));

                        if (frames.Count / cameraFramerate >= timeAfter + timeBefore)
                        {
                            numFramesOK = true;
                        }

                    } while (!numFramesOK);
                }
                else
                {
                    frames = GetVideoFromFrameFiles(startPosition, startPosition.AddSeconds((timeAfter + timeBefore)));
                }

                EncodingAndSave(frames, inf);
            });
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

            // instantiate AVI writer, use WMV3 codec
            AVIWriter writer = new AVIWriter("wmv3");
            // create new AVI file and open it
            writer.Open($"{inf.ImageName}.avi", 1280, 960);
            // create frame image
            foreach (var frame in frames)
            {
                using (MemoryStream ms = new MemoryStream(frame))
                {
                    Bitmap image = (Bitmap)Image.FromStream(ms);
                    writer.AddFrame(image);
                }
            }
            writer.Close();

            Invoke((MethodInvoker)delegate
            {
                textBox1.AppendText($"Saved file {$"{inf.ImageName}.avi"}");
            });
        }

        private void _frameVideo_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (IsHandleCreated)
            {
                Invoke((MethodInvoker)delegate
                {
                    try
                    {
                        try
                        {
                            using (var stream = new MemoryStream())
                            {
                                eventArgs.Frame.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                                var desired = new List<ImageField.TypeOfField>() { ImageField.TypeOfField.Horario };
                                var data = new ImageData(stream.ToArray(), desired);
                                DateTime imgDateTime = DateTime.Parse(data.SortedFields[ImageField.TypeOfField.Horario]);

                                if (imagens.ContainsKey(imgDateTime))
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

                                imagens.Add(imgDateTime, stream.ToArray());
                                if (imagens.Count > maxArrayLength)
                                {
                                    RemoveOlderFrame();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                });
            }
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

            imagens.Remove(imagens.Keys.First());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (btnStart.Text == "Start")
            {
                btnStart.Text = "Stop";
                _frameVideo.Start();
                server.Start();
            }
            else
            {
                btnStart.Text = "Start";
                _frameVideo.Stop();
                server.Stop();
            }
        }
    }

    public class TriggerInfo
    {
        public string CameraDateTime { get; set; }
        public string SystemDateTime { get; set; }
        public string ImageName { get; set; }
    }
}
