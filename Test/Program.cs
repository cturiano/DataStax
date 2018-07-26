using System.Collections.Concurrent;
using System.Threading.Tasks;
using Client;
using Server;

namespace Test
{
    internal class Program
    {
        #region Private Methods

        private static void Main(string[] args)
        {
            var t1 = Task.Run(() => new AsynchronousSocketListener());

            Task.Delay(1000);
            var tasks = new ConcurrentBag<Task>();

            Parallel.For(0, 10, i => { tasks.Add(Task.Run(() => new AsynchronousClient())); });

            Task.WaitAll(tasks.ToArray());
            
            new SynchronousClient();   

            t1.Wait();
        }

        #endregion
    }
}