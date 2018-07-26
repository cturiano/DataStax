using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    public class SynchronousClient
    {
        #region Fields

        // The address of the remote device
        private readonly string _host;

        // The port number for the remote device.  
        private readonly int _port;

        #endregion

        #region Constructors

        public SynchronousClient(string host = "localhost", int port = 5050)
        {
            _host = host;
            _port = port;

            StartClient();
        }

        #endregion

        #region Public Methods

        public void StartClient()
        {
            // Data buffer for incoming data.  
            var bytes = new byte[1024];

            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                var ipHostInfo = Dns.GetHostEntry(_host);
                var ipAddress = ipHostInfo.AddressList[1];
                var remoteEp = new IPEndPoint(ipAddress, _port);

                // Create a TCP/IP  socket.  
                var sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    sender.Connect(remoteEp);

                    // Send the data through the socket.  
                    sender.Send(Encoding.ASCII.GetBytes("/"));

                    // Receive the response from the remote device.  
                    sender.Receive(bytes);

                    // Release the socket.  
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane);
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #endregion
    }
}