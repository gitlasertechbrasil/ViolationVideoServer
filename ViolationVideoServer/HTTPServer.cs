using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Diagnostics;

namespace ViolationVideoServer
{
    public class HTTPServer
    {
        public const String VERSION = "HTTP/1.0";
        public const String NAME = "ViolationVideo 1.0";

        public const String MSG_DIR = "/root/msg/";
        public const String WEB_DIR = "/root/web/";

        private bool _running = false;
        private TcpListener _listener;

        private List<TcpClient> _clients = new List<TcpClient>();

        public delegate void MessagesHandler(string message);
        public event MessagesHandler Messages;

        public delegate void TriggerHandler(string message);
        public event TriggerHandler Trigger;

        static string expression = "({\\s*\"CameraDateTime\":\\s*\"([0-9]{2}\\/[0-9]{2}\\/[0-9]{2}\\s[0-9]{2}:[0-9]{2}:[0-9]{2})\\s *\",\\s*\"SystemDateTime\":\\s*\"([0-9]{2}\\/[0-9]{2}\\/[0-9]{4}\\s[0-9]{2}:[0-9]{2}:[0-9]{2})\\s*\",\\s*\"ImageName\":\\s*\"([0-9]{14}-[0-9]+)\"\\s*})";
        Regex rgx = new Regex(expression);


        public HTTPServer(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public void Start()
        {
            Thread thdRun = new Thread(new ThreadStart(Run));
            thdRun.Start();
        }

        public void Stop()
        {
            _running = false;
            _listener.Stop();
        }

        private void Run()
        {
            _running = true;
            _listener.Start();

            while (_running)
            {
                Messages?.Invoke("Waiting for connection.");
                bool video = false;

                try
                {
                    _clients.Add(_listener.AcceptTcpClient());
                    Messages?.Invoke("Client Connected.");
                    HandleClient(_clients.Last());
                }
                catch (Exception ex)
                {
                }
                Thread.Sleep(1);
            }

            _running = false;
            _listener.Stop();

        }

        

        private void HandleClient(TcpClient client)
        {
            NetworkStream streamReader = client.GetStream();


            Stopwatch st = new Stopwatch();

            byte[] buffer = new byte[client.ReceiveBufferSize];
            int ret = 0;
            int bodyLength = 0;
            int tail = 0;
            int currentIndex = 0;
            int startBody = 0;
            string reader = string.Empty;
            bool readerReceived = false;
            bool completeReceived = false;

            st.Start();
            while (st.ElapsedMilliseconds < 2000)
            {
                while (streamReader.DataAvailable)
                {
                    ret = streamReader.Read(buffer, currentIndex, buffer.Length - currentIndex);
                    currentIndex += ret;

                    try
                    {

                        if (bodyLength == 0)
                        {
                            reader = Encoding.Default.GetString(buffer, 0, buffer.Length);

                            startBody = reader.IndexOf("\r\n\r\n");

                            if (startBody > -1)
                            {

                                readerReceived = true;
                                if (reader.IndexOf("Content-Length") > -1)
                                {
                                    string length = Regex.Match(reader, "((?<=Content-Length: ).*)").Value.Replace("\r", "");
                                    bodyLength = Convert.ToInt32(length);
                                }
                                tail = (bodyLength + startBody + 4) - reader.Trim('\0').Length;

                                if (bodyLength > buffer.Length)
                                {
                                    SendToClient(SendHeader("application/json", 0, StatusCode.ExpectationFailed).ToString(), client);
                                }

                                bool expect = Regex.IsMatch(reader, "((?<=(?i)Expect: ).*)");
                                if (expect && !reader.Contains("HTTP/1.0"))
                                {
                                    SendToClient(SendHeader("application/json", 0, StatusCode.Expect100).ToString(), client);
                                    continue;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                if (currentIndex >= (bodyLength + startBody + 4) && readerReceived)
                {
                    completeReceived = true;
                    break;
                }

                if (st.ElapsedMilliseconds % 250 == 0)
                {
                    Thread.Sleep(50);
                }
            }
            st.Reset();
            st.Stop();

            byte[] responseBytes = new byte[] { };

            string msg = string.Empty;

            if (completeReceived)
            {
                msg = Encoding.UTF8.GetString(buffer, 0, currentIndex);
            }

            var mtch = rgx.Match(msg);

            if (mtch.Success)
            {
                Trigger?.Invoke(mtch.Groups[1].Value);
                SendToClient(SendHeader("application/json", 0, StatusCode.OK).ToString(), client);
            }
            else
            {
                SendToClient(SendHeader("application/json", 0, StatusCode.BadRequest).ToString(), client);
            }
        }

        private string GetStatusCode(StatusCode statusCode)
        {
            string code;

            switch (statusCode)
            {
                case StatusCode.OK: code = "200 OK"; break;
                case StatusCode.NoContent: code = "204 No Content"; break;
                case StatusCode.BadRequest: code = "400 Bad Request"; break;
                case StatusCode.Unauthorized: code = "401 Unauthorized"; break;
                case StatusCode.Forbidden: code = "403 Forbidden"; break;
                case StatusCode.NotFound: code = "404 Not Found"; break;
                case StatusCode.NotAllowed: code = "405 Method not allowed"; break;
                case StatusCode.MediaType: code = "415 Unsupported media type"; break;
                case StatusCode.InternalServerError: code = "500 Internal Server Error"; break;
                case StatusCode.ServiceUnavailable: code = "503 Service Unavailable"; break;
                case StatusCode.Expect100: code = "100 100-continue"; break;
                case StatusCode.ExpectationFailed: code = "417 expectation failed"; break;
                default: code = "202 Accepted"; break;
            }

            return code;
        }

        public StringBuilder SendHeader(string mimeType, long totalBytes, StatusCode statusCode)
        {
            StringBuilder header = new StringBuilder();
            header.Append(string.Format("HTTP/1.1 {0}\r\n", GetStatusCode(statusCode)));
            header.Append(string.Format("Accept: {0}\r\n", "application/json;charset=utf-8"));
            header.Append(string.Format("Content-Type: {0}\r\n", mimeType));
            header.Append(string.Format("Accept-Ranges: bytes\r\n"));
            header.Append(string.Format("Server: {0}\r\n", "Video Server"));
            header.Append(string.Format("Connection: close\r\n"));
            header.Append(string.Format("Content-Length: {0}\r\n", totalBytes));
            header.Append(string.Format("Access-Control-Allow-Headers:{0}\r\n", "*"));
            header.Append(string.Format("Access-Control-Allow-Origin:{0}\r\n", "*"));
            header.Append(string.Format("Access-Control-Allow-Methods:{0}\r\n", "OPTIONS, GET, HEAD, POST, PUT"));
            header.Append(string.Format("Allow:{0}\r\n", "OPTIONS, GET, HEAD, POST, PUT"));
            // configura requisições 'OPTIONS' recebidas pelo navegador
            header.Append(string.Format("Access-Control-Max-Age:{0}\r\n", "86400"));


            header.Append("\r\n");

            return header;
        }

        private bool CheckExpectation(string reader)
        {
            return Regex.IsMatch(reader, "((?<=Expect: ).*)");
        }

        private void SendToClient(string data, TcpClient tcpClient)
        {
            byte[] bytes = Encoding.Default.GetBytes(data);
            SendToClient(bytes, bytes.Length, tcpClient);
        }

        private void SendToClient(byte[] data, int bytesTosend, TcpClient tcpClient)
        {
            try
            {
                Socket socket = tcpClient.Client;

                if (socket.Connected)
                {
                    int sentBytes = socket.Send(data, 0, bytesTosend, 0);                  
                }
            }
            catch (Exception e)
            {
               
            }
        }


        private void Sending(TcpClient client, string reponse)
        {
            NetworkStream stream = client.GetStream();
            Thread _thdLocal = new Thread((response) =>
            {
                try
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(String.Format("{0} {1}\r\n; boundary=--boundary\r\n",
                    HTTPServer.VERSION, response));
                    writer.Flush();

                }
                catch
                {
                    try
                    {
                        client.Close();
                        _clients.Remove(client);
                    }
                    catch (Exception)
                    {
                    }
                }
            });

            _thdLocal.Priority = ThreadPriority.Lowest;
            _thdLocal.Start();
        }
    }

    public enum StatusCode
    {
        /// <summary>
        /// 200 OK
        /// </summary>
        OK = 200,
        /// <summary>
        /// 400 Bad Request
        /// </summary>
        Unauthorized = 401,
        /// <summary>
        /// 401 Unauthorized
        /// </summary>
        BadRequest = 400,
        /// <summary>
        /// 404 File not found
        /// </summary>
        NotFound = 404,
        /// <summary>
        /// 403 Access Forbidden
        /// </summary>
        Forbidden = 403,
        /// <summary>
        /// 415 Unsupported Media Type
        /// </summary>
        MediaType = 415,
        /// <summary>
        /// 405 Method not allowed
        /// </summary>
        NotAllowed = 405,
        /// <summary>
        /// 204 No Content
        /// </summary>
        NoContent = 204,
        /// <summary>
        /// 500 Internal Server Error
        /// </summary>
        InternalServerError = 500,
        /// <summary>
        /// Service Unavailable 
        /// </summary>
        ServiceUnavailable = 503,
        /// <summary>
        /// 100-Continue
        /// </summary>
        Expect100 = 100,
        /// <summary>
        /// Expectation Failed
        /// </summary>
        ExpectationFailed = 417


    };
    public enum Method
    {
        GET,
        POST,
        PUT,
        DELETE,
        UNKNOW,
        OPTIONS
    };
    public enum MimeType
    {
        Json,
        Xml,
        Html,
        Unknow,
        XwwwFormUrlEncoded
    }

    public enum InternalCode
    {
        NonexistentParameter = 1,
        InvalidParameter = 2,
        InvalidDataFormat = 3,
        InvalidCredentials = 4,
        InvalidToken = 5,
        ServiceUnavailable = 6,
        InternalServerError = 7,
        IsReadOnly = 8,
        IsRequired = 9,
        AutoUpdateError = 10
    }
}
