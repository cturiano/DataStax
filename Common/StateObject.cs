using System.Net.Sockets;
using System.Text;

namespace Server
{
    /// <summary>
    ///     State object for reading client data asynchronously
    /// </summary>
    public class StateObject
    {
        #region Fields

        // Receive buffer.  
        public byte[] Buffer = new byte[BufferSize];

        // Received data string.  
        public StringBuilder Sb = new StringBuilder();

        // Client  socket.  
        public Socket WorkSocket;

        #endregion

        #region Static Fields and Constants

        // Size of receive buffer.  
        public const int BufferSize = 1024;

        #endregion
    }
}