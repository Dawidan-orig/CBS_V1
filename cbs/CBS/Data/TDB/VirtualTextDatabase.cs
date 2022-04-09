using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CBS.Data.TDB
{
    public class VirtualTextDatabase: ITextDatabase
    {
        private readonly Dictionary<Tuple<string, string>, string> _dictionary = new Dictionary<Tuple<string, string>, string>();
        public Task<string> Read(string db, string key) =>
            Task.FromResult(_dictionary[new Tuple<string, string>(db, key)]);

        public Task<string> Read(string db, string key, string defaultValue) =>
            Task.FromResult(_dictionary.GetValueOrDefault(new Tuple<string, string>(db, key), defaultValue));

        public Task Write(string db, string key, string value)
        {
            _dictionary[new Tuple<string, string>(db, key)] = value;
            return Task.CompletedTask;
        }
    }
}