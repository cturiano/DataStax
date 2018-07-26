namespace Server
{
    internal class Program
    {
        #region Public Methods

        public static int Main(string[] args)
        {
            new AsynchronousSocketListener();
            return 0;
        }

        #endregion
    }
}