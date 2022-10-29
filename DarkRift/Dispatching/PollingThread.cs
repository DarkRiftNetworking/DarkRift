using System;
using System.Collections.Generic;
using System.Threading;

namespace DarkRift.Dispatching
{
    internal static class PollingThread
    {
        public static Action<Exception> ExceptionHandler { get; set; }

        private static readonly object myLock = new object();
        private static bool threadStarted;
        private static readonly List<Action> workList = new List<Action>();

        private static readonly ManualResetEvent stopEvent = new ManualResetEvent(false);

        public static void AddWork(Action work)
        {
            lock (myLock)
            {
                workList.Add(work);

                if (!threadStarted)
                {
                    StartThread();
                }
            }
        }

        public static void RemoveWork(Action work)
        {
            //this blocking on a polling iteration is intended
            lock (myLock)
            {
                InternalRemoveWork(work);
            }
        }

        private static void InternalRemoveWork(Action work)
        {
            workList.Remove(work);
        }

        public static void StopThread()
        {
            stopEvent.Set();

            //No support to restart currently since it is presently regarded superfluous (see threadStarted).
        }

        private static void StartThread()
        {
            stopEvent.Reset();
            threadStarted = true;

            var thread = new Thread(PollingThreadLogic);
            thread.IsBackground = true;
            thread.Name = "DarkRift 2 Polling Thread";
            thread.Start();
        }

        private static void PollingThreadLogic()
        {
            var rng = new Random();
            
            while (!stopEvent.WaitOne(1))
            {
                lock (myLock)
                {
                    PollingIteration(rng);
                }
            }
        }

        private static void PollingIteration(Random rng)
        {
            var work = workList;
            rng.Shuffle(work);

            for (int i = work.Count - 1; i >= 0; --i)
            {
                var item = work[i];

                try
                {
                    item?.Invoke();
                }
                catch (Exception ex)
                {
                    InternalRemoveWork(item);
                    ExceptionHandler?.Invoke(ex);
                }
            }
        }

        private static void Shuffle<T>(this Random rng, IList<T> array)
        {
            int n = array.Count;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }
    }
}
