using System.Collections.Generic;
using System.Threading.Tasks;
using CBS.Data.TDB;
using Newtonsoft.Json;

namespace CBS.Data.ODB
{
    public class JsonDatabase : IObjectDatabase
    {
        private readonly ITextDatabase _textDatabase;
        private readonly JsonSerializerSettings _settings;

        public JsonDatabase(ITextDatabase textDatabase, JsonSerializerSettings settings = null)
        {
            _textDatabase = textDatabase;
            _settings = settings;
        }
        public async Task<T> Read<T>(string db, string key) =>
            JsonConvert.DeserializeObject<T>(await _textDatabase.Read(db, key), _settings);
        public async Task<T> Read<T>(string db, string key, T defaultValue)
        {
            try
            {
                return await Read<T>(db, key);
            }
            catch (KeyNotFoundException)
            {
                return defaultValue;
            }
        }
        public async Task Write(string db, string key, object value) =>
            await _textDatabase.Write(db, key, JsonConvert.SerializeObject(value, _settings));
    }
}
