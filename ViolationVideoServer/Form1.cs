using AForge.Video;
using br.com.ltb.Camera.Pumatronix;
using br.com.ltb.tools2.queue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
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
        private int maxLength = 30;

        private int beforeFrames = 0;
        private int afterFrames = 30;
        


        public Form1()
        {
            InitializeComponent();
            server = new HTTPServer(51000);
            server.Messages += Server_MessagesHandler;
            server.Trigger += Server_Trigger;
            var _videoPath = "http://" + "192.168.50.172" + "/api/mjpegvideo.cgi?framerate=10";
            _frameVideo = new MJPEGStream(_videoPath);
            _frameVideo.Login = "admin";
            _frameVideo.Password = "1234";
            _frameVideo.NewFrame += _frameVideo_NewFrame;
        }

        private void Server_Trigger(string message)
        {
            try
            {
                var info = JsonConvert.DeserializeObject<TriggerInfo>(message);
            }
            catch (Exception ex)
            {

            }
        }

        private void Server_MessagesHandler(string message)
        {
            var recebido = message;
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
                                imagens.Add(imgDateTime, stream.ToArray());
                                if(imagens.Count > maxLength)
                                {
                                    imagens.Remove(imagens.Keys.First());
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

        private void button1_Click(object sender, EventArgs e)
        {
            //_frameVideo.Start();
            server.Start();
        }
    }

    public class TriggerInfo
    {
        public string CameraDateTime { get; set; }
        public string SystemDateTime { get; set; }
        public string ImageName { get; set; }
    }
}
