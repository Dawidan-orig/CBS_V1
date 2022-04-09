using System.Collections.Generic;
using CBS.Data.ODB;
using CBS.Data.TDB;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CBSTests.Data.ODB
{
    [TestClass]
    public class JsonDatabaseTests
    {
        private static JsonDatabase Instance() => new JsonDatabase(new VirtualTextDatabase());
        [TestMethod]
        public void ReadFailsOnEmptyDb()
        {
            var db = Instance();
            Assert.ThrowsException<KeyNotFoundException>(() => db.Read<string>("test-db", "test-key").GetAwaiter().GetResult());
        }

        [TestMethod]
        public void ExtendedReadReturnsDefaultOnEmptyDb()
        {
            var db = Instance();
            Assert.AreEqual(db.Read("test-db", "test-key", "default-value").GetAwaiter().GetResult(), "default-value");
        }

        [TestMethod]
        public void ReadReturnsWrittenValue()
        {
            var db = Instance();
            db.Write("test-db", "test-key", "written-value").GetAwaiter().GetResult();
            Assert.AreEqual(db.Read<string>("test-db", "test-key").GetAwaiter().GetResult(), "written-value");
        }

        [TestMethod]
        public void ExtendedReadReturnsWrittenValue()
        {
            var db = Instance();
            db.Write("test-db", "test-key", "written-value").GetAwaiter().GetResult();
            Assert.AreEqual(db.Read("test-db", "test-key", "default-value").GetAwaiter().GetResult(), "written-value");
        }
    }
}
