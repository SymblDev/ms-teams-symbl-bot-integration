using System;
using System.Collections.Concurrent;

namespace PsiBot.Services.Bot
{
    /// <summary>
    /// The Singleton Meeting Generator
    /// </summary>
    public sealed class SingletonMeetingGenerator
    {
        internal static ConcurrentDictionary<string, string> concurrentDictionary = new ConcurrentDictionary<string, string>();

        SingletonMeetingGenerator()
        {
        }

        public static string GetUniqueId(string callId)
        {
            if (!concurrentDictionary.ContainsKey(callId))
            {
                concurrentDictionary.TryAdd(callId,
                    Guid.NewGuid().ToString());
            }

            return concurrentDictionary[callId];
        }
    }
}
