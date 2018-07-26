using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    public class AsynchronousSocketListener
    {
        #region Fields

        private readonly int _port;

        #endregion

        #region Static Fields and Constants

        private static byte[] _byteData;
        private static Random _random;
        private static readonly ManualResetEvent AllDone = new ManualResetEvent(false);
        private const string HttpVersion = "HTTP/1.1";
        private const string MimeHeader = "text/plain";
        private const string StatusCode = "200 OK";

        #endregion

        #region Constructors

        public AsynchronousSocketListener(int responseDataCount = 125000, int port = 5050, int listenQueueSize = 100)
        {
            _port = port;
            _byteData = new byte[responseDataCount];
            _random = new Random();
            CreateBytes();
            StartListening(listenQueueSize);
        }

        #endregion

        #region Private Methods

        private static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the listening thread to continue (wait for another connection).
            AllDone.Set();

            // Get the socket that handles the client request.  
            var listener = (Socket)ar.AsyncState;
            var handler = listener.EndAccept(ar);

            // Create the state object.  
            var state = new StateObject {WorkSocket = handler};
            handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReadCallback, state);
        }

        private static void CreateBytes()
        {
            _random.NextBytes(_byteData);
        }

        private static void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket from the asynchronous state object.  
            var state = (StateObject)ar.AsyncState;
            var handler = state.WorkSocket;

            // Read data from the client socket.   
            var bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                state.Sb.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));
                Send(handler);
            }
        }

        private static void Send(Socket handler)
        {
            //SendHeaders(handler);

            // Begin sending the data to the remote device.  
            handler.BeginSend(_byteData, 0, _byteData.Length, 0, SendCallback, handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                var handler = (Socket)ar.AsyncState;

                // Complete sending and close the connection.
                var bytesSent = handler.EndSend(ar);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

                Console.WriteLine("Sent {0} bytes to client.", bytesSent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void StartListening(int queueSize)
        {
            var localEndPoint = new IPEndPoint(IPAddress.Loopback, _port);
            var listener = new Socket(SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(queueSize);

                while (true)
                {
                    // Set the event to nonsignaled state.  
                    AllDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(AcceptCallback, listener);

                    // Wait until a connection is made before continuing.  
                    AllDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        #endregion
    }
}