using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CBS.Data.TDB
{
    public class FileDatabase: ITextDatabase
    {
        private readonly string _ownDataDirectory;
        private readonly DataMode _dataMode;
        private readonly KeyMode _keyMode;
        private readonly string _ext;

        public FileDatabase(string ownDataDirectory, DataMode dataMode, KeyMode keyMode, string ext)
        {
            _ownDataDirectory = ownDataDirectory;
            _dataMode = dataMode;
            _keyMode = keyMode;
            _ext = ext;
        }
        private static string BytesToHex(byte[] value) => BitConverter.ToString(value).Replace("-", string.Empty);  // BitConverter.ToString returns in a format like "01-AB-89"
        private static byte[] Hash(string value) => SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(value));
        private static string HashedKey(string key) => BytesToHex(Hash(key));
        private string ProperKey(string key)
        {
            switch (_keyMode)
            {
                case KeyMode.Plain:
                    return key;
                case KeyMode.Hashed:
                    return HashedKey(key);
                default:
                    throw new ArgumentOutOfRangeException(nameof(_keyMode), _keyMode, null);
            }
        }
        private string ProperFilename(string key) => ProperKey(key) + _ext;
        private string ProperDatabaseDirectory(string db) => _ownDataDirectory.PathCombine(db).AsExistingDirectory();
        private string ProperPath(string db, string key) => Path.Combine(ProperDatabaseDirectory(db), ProperFilename(key));
        private static async Task<string> ReadCompressed(string path)
        {
            using (var file = File.Open(path + ".bin-gz", FileMode.Open))
            using (var decompressStream = new DeflateStream(file, CompressionMode.Decompress))
            using (var reader = new StreamReader(decompressStream, Encoding.UTF8))
                return await reader.ReadToEndAsync();
        }
        private static async Task<string> ReadPlain(string path)
        {
            using (var reader = new StreamReader(path))
                return await reader.ReadToEndAsync();
        }
        private async Task<string> Read(string path)
        {
            try
            {
                switch (_dataMode)
                {
                    case DataMode.Plain:
                        return await ReadPlain(path);
                    case DataMode.Compressed:
                        return await ReadCompressed(path);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(_dataMode), _dataMode, null);
                }
            }
            catch (FileNotFoundException)
            {
                throw new KeyNotFoundException();
            }
        }
        public async Task<string> Read(string db, string key) => await Read(ProperPath(db, key));

        public async Task<string> Read(string db, string key, string defaultValue)
        {
            try
            {
                return await Read(db, key);
            }
            catch (KeyNotFoundException)
            {
                return defaultValue;
            }
        }
        private static async Task WriteCompressed(string path, string content)
        {
            using (var file = File.Open(path + ".bin-gz", FileMode.Create))
            using (var compressStream = new DeflateStream(file, CompressionMode.Compress))
            using (var writer = new StreamWriter(compressStream, Encoding.UTF8))
                await writer.WriteAsync(content);
        }
        private static async Task WritePlain(string path, string content)
        {
            using (var writer = new StreamWriter(path))
                await writer.WriteAsync(content);
        }
        private async Task Write(string path, string content)
        {
            switch (_dataMode)
            {
                case DataMode.Plain:
                    await WritePlain(path, content);
                    break;
                case DataMode.Compressed:
                    await WriteCompressed(path, content);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_dataMode), _dataMode, null);
            }
        }
        public async Task Write(string db, string key, string value) => await Write(ProperPath(db, key), value);

        public enum DataMode
        {
            Plain,
            Compressed
        }
        public enum KeyMode
        {
            Plain,
            Hashed
        }
        public static string ExtensionForTextType(SupportedTextType textType)
        {
            switch (textType)
            {
                case SupportedTextType.Text:
                    return ".txt";
                case SupportedTextType.Json:
                    return ".json";
                default:
                    throw new ArgumentOutOfRangeException(nameof(textType), textType, null);
            }
        }
    }
}