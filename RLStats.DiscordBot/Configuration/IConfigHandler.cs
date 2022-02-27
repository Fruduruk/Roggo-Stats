using System;
using System.Collections.Generic;

namespace Discord_Bot.Configuration
{
    public interface IConfigHandler<T> where T : IEquatable<T>, new()
    {
        void AddConfigEntry(T entry);
        List<T> GetConfig();
        void SetConfig(List<T> value);
        bool HasConfigEntryInIt(T entry);
        void RemoveConfigEntry(T entry);
    }
}