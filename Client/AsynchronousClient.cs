using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Server;

namespace Client
{
    public class AsynchronousClient
    {
        #region Fields

        // ManualResetEvent instances signal completion.  
        private readonly ManualResetEvent _connectDone;
        private readonly ManualResetEvent _receiveDone;
        private readonly ManualResetEvent _sendDone;

        // The address of the remote device
        private readonly string _host;

        // The port number for the remote device.  
        private readonly int _port;

        // The response from the remote device.  
        private string _response = string.Empty;

        #endregion

        #region Constructors

        public AsynchronousClient(string host = "localhost", int port = 5050)
        {
            _connectDone = new ManualResetEvent(false);
            _receiveDone = new ManualResetEvent(false);
            _sendDone = new ManualResetEvent(false);
            _host = host;
            _port = port;

            StartClient();
        }

        #endregion

        #region Private Methods

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                var client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                // Signal that the connection has been made.  
                _connectDone.Set();

                Debug.WriteLine("Socket connected to {0}", client.RemoteEndPoint);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        private void Receive(Socket client)
        {
            try
            {
                // Create the state object.  
                var state = new StateObject {WorkSocket = client};

                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                var state = (StateObject)ar.AsyncState;
                var client = state.WorkSocket;

                // Read data from the remote device.  
                var bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.  
                    state.Sb.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));

                    // Get the rest of the data.  
                    client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
                }
                else
                {
                    // All the data has arrived; put it in response.  
                    if (state.Sb.Length > 1)
                    {
                        _response = state.Sb.ToString();
                    }

                    // Signal that all bytes have been received.  
                    _receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        private void Send(Socket client, string data)
        {
            // Convert the string data to byte data using ASCII encoding.
            var byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            client.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                var client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                var bytesSent = client.EndSend(ar);

                // Signal that all bytes have been sent.
                _sendDone.Set();

                Debug.WriteLine("Sent {0} bytes to server.", bytesSent);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        private void StartClient()
        { 
            try
            {
                // Establish the remote endpoint for the socket.
                var ipHostInfo = Dns.GetHostEntry(_host);
                var ipAddress = ipHostInfo.AddressList[1];
                var remoteEp = new IPEndPoint(ipAddress, _port);
                var client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint and wait for connection to complete.
                client.BeginConnect(remoteEp, ConnectCallback, client);
                _connectDone.WaitOne();

                // Send test data to the remote device, then wait for sending to complete.
                Send(client, "/");
                _sendDone.WaitOne();

                // Receive the response from the remote device, then wait for receiving to complete.
                Receive(client);
                _receiveDone.WaitOne();

                // Release the socket.  
                client.Shutdown(SocketShutdown.Both);
                client.Close();

                Debug.WriteLine("Response received : {0}", _response);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        #endregion
    }
}