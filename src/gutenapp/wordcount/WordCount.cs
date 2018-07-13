﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using interfaces;

namespace Lit
{
    public class WordCount : IProvider
    {
        int _count = 0;
        public WordCount(){}

        public Task<int> ProcessTextAsync(Task<string> text)
        {
            return CountAddAsync(text);
        }

        public Dictionary<string, object> GetReport()
        {
            var d = new Dictionary<string,object>();
            d.Add("count",_count);
            return d;
        }

        public int CountAdd(string text)
        {
            var wasSpace = false;
            var count = 0;
            foreach(var t in text)
            {
                var isSpace = Char.IsWhiteSpace(t);
                if (isSpace &&
                    !wasSpace)
                {
                    count++;
                    wasSpace = true;
                }
                else if (!isSpace && wasSpace)
                {
                    wasSpace = false;
                }
            }

            if (count > 0 && !wasSpace)
            {
                count++;
            }
            _count += count;
            return count;
        }

        public async Task<int> CountAddAsync(Task<string> text)
        {
            var t = await text;
            var count = CountAdd(t);
            return count;
        }

    }
}
