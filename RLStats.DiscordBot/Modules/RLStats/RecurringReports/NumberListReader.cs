using System;
using System.Collections.Generic;

namespace Discord_Bot.Modules.RLStats.RecurringReports
{
    public class NumberListReader
    {
        public bool CollectAll { get; private set; }
        
        public IEnumerable<int> ReadIndexNuberList(List<string> indexList)
        {
            var numberList = new List<int>();
            bool collectAll = false;
            foreach (var index in indexList)
            {
                try
                {
                    if (index.ToLower().Equals("all"))
                    {
                        collectAll = true;
                        break;
                    }
                    int indexNumber = Convert.ToInt32(index);
                    numberList.Add(indexNumber);
                }
                catch
                {
                    continue;
                }
            }
            CollectAll = collectAll;
            return numberList;
        }
    }
}
