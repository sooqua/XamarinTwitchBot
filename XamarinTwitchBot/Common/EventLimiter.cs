// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Limits the numbers of requests in a specified period of time
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.Common
{
    using System;
    using System.Collections.Generic;

    internal class EventLimiter
    {
        private readonly Queue<DateTime> requestTimes;
        private readonly int maxRequests;
        private readonly TimeSpan timeSpan;

        public EventLimiter(int maxRequests, TimeSpan timeSpan)
        {
            this.maxRequests = maxRequests;
            this.timeSpan = timeSpan;
            this.requestTimes = new Queue<DateTime>(maxRequests);
        }

        public bool CanRequestNow()
        {
            this.SynchronizeQueue();
            return this.requestTimes.Count < this.maxRequests;
        }

        public void EnqueueRequest()
        {
            this.requestTimes.Enqueue(DateTime.UtcNow);
        }

        private void SynchronizeQueue()
        {
            while ((this.requestTimes.Count > 0) && (this.requestTimes.Peek().Add(this.timeSpan) < DateTime.UtcNow))
            {
                this.requestTimes.Dequeue();
            }
        }
    }
}