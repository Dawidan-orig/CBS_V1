using System.Threading.Tasks;

namespace CBS.Data.TDB
{
    public interface ITextDatabase
    {
        Task<string> Read(string db, string key);
        Task<string> Read(string db, string key, string defaultValue);
        Task Write(string db, string key, string value);
    }
    public enum SupportedTextDatabase
    {
        File,
        Virtual
    }

    public enum SupportedTextType
    {
        Text,
        Json
    }
}
