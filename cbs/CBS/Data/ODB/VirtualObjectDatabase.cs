using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CBS.Data.ODB
{
    public class VirtualObjectDatabase: IObjectDatabase
    {
        private readonly Dictionary<Tuple<string, string>, object> _dictionary = new Dictionary<Tuple<string, string>, object>();
        public Task<T> Read<T>(string db, string key) =>
            _dictionary[new Tuple<string, string>(db, key)] is T result
                ? Task.FromResult(result)
                : throw new KeyNotFoundException();

        public Task<T> Read<T>(string db, string key, T defaultValue)
        {
            try
            {
                return Read<T>(db, key);
            }
            catch (KeyNotFoundException)
            {
                return Task.FromResult(defaultValue);
            }
        }

        public Task Write(string db, string key, object value)
        {
            _dictionary[new Tuple<string, string>(db, key)] = value;
            return Task.CompletedTask;
        }
    }
}