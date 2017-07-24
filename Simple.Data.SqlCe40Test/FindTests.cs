﻿using System.Reflection;
using System.Collections.Generic;
using NUnit.Framework;
using System.IO;
using Shitty.Data.Ado;
using Shitty.Data.SqlCe40;
using Shitty.Data.SqlCeTest;

namespace Shitty.Data.SqlCe40Test
{
    /// <summary>
    /// Summary description for FindTests
    /// </summary>
    [TestFixture]
    public class FindTests
    {
        private static readonly string DatabasePath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring(8)),
            "TestDatabase.sdf");

        [TestFixtureSetUp]
        public void DeleteAlice()
        {
            var db = Database.Opener.OpenFile(DatabasePath);
            db.Users.DeleteByName("Alice");
        }

        [Test]
        public void TestProviderWithFileName()
        {
            var provider = new ProviderHelper().GetProviderByFilename(DatabasePath);
            Assert.IsInstanceOf(typeof (SqlCe40ConnectionProvider), provider);
        }

        [Test]
        public void TestProviderWithConnectionString()
        {
            var provider = new ProviderHelper().GetProviderByConnectionString(string.Format("data source={0}", DatabasePath));
            Assert.IsInstanceOf(typeof(SqlCe40ConnectionProvider), provider);
        }

        [Test]
        public void TestFindById()
        {
            var db = Database.Opener.OpenFile(DatabasePath);
            var user = db.Users.FindById(1);
            Assert.AreEqual(1, user.Id);
        }

        [Test]
        public void TestAll()
        {
            var db = Database.OpenFile(DatabasePath);
            var all = new List<object>(db.Users.All().Cast<dynamic>());
            Assert.IsNotEmpty(all);
        }

        [Test]
        public void TestImplicitCast()
        {
            var db = Database.OpenFile(DatabasePath);
            User user = db.Users.FindById(1);
            Assert.AreEqual(1, user.Id);
        }

        [Test]
        public void TestImplicitEnumerableCast()
        {
            var db = Database.OpenFile(DatabasePath);
            foreach (User user in db.Users.All())
            {
                Assert.IsNotNull(user);
            }
        }

        [Test]
        public void TestInsert()
        {
            var db = Database.OpenFile(DatabasePath);

            var user = db.Users.Insert(Name: "Alice", Password: "foo", Age: 29);

            Assert.IsNotNull(user);
            Assert.AreEqual("Alice", user.Name);
            Assert.AreEqual("foo", user.Password);
            Assert.AreEqual(29, user.Age);
        }
    }
}
