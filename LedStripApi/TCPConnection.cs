using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace LedStripApi
{
    public class TCPConnection : IConnection
    {
        public string Host { get; private set; }

        public int Port { get; private set; }

        public TcpClient TcpClient { get; private set; }

        private NetworkStream Stream => this.TcpClient?.GetStream();

        public bool Connect(string host, int port)
        {
            this.Disconnect();

            this.Host = host;
            this.Port = port;
            this.TcpClient = new TcpClient
            {
                NoDelay = true
            };
            this.TcpClient.ConnectAsync(this.Host, this.Port).Wait(3000);

            return this.TcpClient.Connected;
        }

        public void Disconnect()
        {
            this.TcpClient?.Dispose();
            this.TcpClient = null;
        }

        public bool Send(byte[] data)
        {
            bool result = false;

            if (this.Stream != null && this.Stream.CanWrite)
            {
                this.Stream.Write(data, 0, data.Length);

                this.Stream.Flush();

                result = true;
            }

            return result;
        }

        public string SendCommand(string command)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(command);

            return Send(bytes).ToString();
        }

        public bool SendSocket(byte[] data)
        {
            bool result = false;

            if (this.Stream != null)
            {
                this.TcpClient.Client.Send(data, data.Length, SocketFlags.None);

                result = true;
            }

            return result;
        }


        public byte[] Request(byte[] data, out bool timeOut)
        {
            timeOut = true;

            bool sendResult = this.Send(data);

            byte[] response = null;

            if (sendResult)
            {
                bool ok = Task.Factory.StartNew(() => response = this.GetResponse()).Wait(1000);

                timeOut = !ok;
            }

            return response;
        }

        public byte[] RequestAndroid(byte[] data, out bool timeOut)
        {
            timeOut = true;

            bool sendResult = this.Send(data);

            byte[] response = null;

            if (sendResult)
            {
                response = this.GetResponseAndroid();

                timeOut = response == null;
            }

            return response;
        }

        private byte[] GetResponse()
        {
            byte[] buffer = new byte[1024];

            int numBytesRead = 0;
            int chunkSize = 1;

            while (!this.Stream.DataAvailable) ;

            while (this.Stream.DataAvailable)
            {
                numBytesRead += this.Stream.Read(buffer, numBytesRead, chunkSize);
            }

            byte[] data = new byte[numBytesRead];

            for (int i = 0; i < numBytesRead; i++)
            {
                data[i] = buffer[i];
            }

            return data;
        }

        private byte[] GetResponseAndroid()
        {
            byte[] buffer = new byte[1024];

            int numBytesRead = 0;
            int chunkSize = 1;

            Stopwatch s = Stopwatch.StartNew();

            while (!this.Stream.DataAvailable)
            {
                if (s.ElapsedMilliseconds > 1000)
                    return null;
            }

            while (this.Stream.DataAvailable)
            {
                numBytesRead += this.Stream.Read(buffer, numBytesRead, chunkSize);
            }

            byte[] data = new byte[numBytesRead];

            for (int i = 0; i < numBytesRead; i++)
            {
                data[i] = buffer[i];
            }

            return data;
        }

        public void Dispose()
        {
            TcpClient.Dispose();        }
    }
}
