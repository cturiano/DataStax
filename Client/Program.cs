namespace Client
{
    internal class Program
    {
        #region Public Methods

        public static int Main(string[] args)
        {
            new AsynchronousClient("www.google.com", 443);
            return 0;
        }

        #endregion
    }
}