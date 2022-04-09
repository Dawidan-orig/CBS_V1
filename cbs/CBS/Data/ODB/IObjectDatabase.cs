using System.Threading.Tasks;

namespace CBS.Data.ODB
{
    public interface IObjectDatabase
    {
        Task<T> Read<T>(string db, string key);
        Task<T> Read<T>(string db, string key, T defaultValue);
        Task Write(string db, string key, object value);
    }

    public enum SupportedObjectDatabase
    {
        Json,
        Virtual
    }
}
