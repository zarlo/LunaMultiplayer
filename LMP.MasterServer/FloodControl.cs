using System;
using System.Collections.Concurrent;

namespace LMP.MasterServer
{
    internal class FloodControl
    {
        internal static int MaxRequestsPerMs { get; set; } = 500;

        private static readonly ConcurrentDictionary<string, DateTime> FloodControlDictionary = new ConcurrentDictionary<string, DateTime>();

        public static bool AllowRequest(string address)
        {
            if (FloodControlDictionary.TryGetValue(address, out var lastRequest) && (DateTime.UtcNow - lastRequest).TotalMilliseconds < MaxRequestsPerMs)
            {
                return false;
            }

            FloodControlDictionary.AddOrUpdate(address, DateTime.UtcNow, (key, existingVal) => DateTime.UtcNow);
            return true;
        }
    }
}
