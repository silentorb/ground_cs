using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Query;

namespace ground
{
    class Server
    {
        private TcpListener tcp_listener;
        private Thread listen_thread;
        ASCIIEncoding encoder = new ASCIIEncoding();

        public void start(int port = 4000)
        {
            Console.WriteLine("Starting Ground");
            tcp_listener = new TcpListener(IPAddress.Any, port);
            listen_thread = new Thread(listen);
            listen_thread.Start();
            Console.WriteLine("Ground is listening on port " + port + ".");
        }

        private void listen()
        {
            tcp_listener.Start();

            while (true)
            {
                TcpClient client = tcp_listener.AcceptTcpClient();
                var thread = new Thread(handle_request);
                thread.Start(client);
            }
        }

        private void handle_request(object client)
        {
            TcpClient tcp_client = (TcpClient)client;
            NetworkStream client_stream = tcp_client.GetStream();

            byte[] message = new byte[4096];

            while (true)
            {
                int bytes_read = 0;

                try
                {
                    //blocks until a client sends a message
                    bytes_read = client_stream.Read(message, 0, 4096);
                }
                catch
                {
                    //a socket error has occured
                    break;
                }

                if (bytes_read == 0)
                {
                    //the client has disconnected from the server
                    break;
                }

                //message has successfully been received
                var raw_request = encoder.GetString(message, 0, bytes_read);

                var raw_response = process_response(raw_request);
                byte[] buffer = encoder.GetBytes(raw_response);

                client_stream.Write(buffer, 0, buffer.Length);
                client_stream.Flush();

                tcp_client.Close();
            }
        }

        string process_response(string raw_request)
        {
            Console.WriteLine(raw_request);
            var content_regex = new Regex(@"(?<=\r\n\r\n).*", RegexOptions.Singleline);
            var match = content_regex.Match(raw_request);
            var content = match.Value;
            var miner = new Miner();
            var raw_response = miner.run(content);
            Console.WriteLine(raw_response);
            return "HTTP/1.1 200 OK\r\n"
                + "Content-Type: application/json\r\n"
                + "\r\n"
                + raw_response;
        }
    }
}
