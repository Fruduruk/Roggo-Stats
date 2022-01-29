using System;
using System.Collections.Generic;

namespace Discord_Bot.Configuration
{
    public interface IConfigList<T> where T : IEquatable<T>
    {
        List<T> ConfigEntries { get; set; }
    }
}