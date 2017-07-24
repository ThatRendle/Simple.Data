﻿namespace Shitty.Data.IntegrationTest
{
    using System;
    using Mocking.Ado;
    using NUnit.Framework;

    [TestFixture]
    public class GetCountTest : DatabaseIntegrationContext
    {
        protected override void SetSchema(MockSchemaProvider schemaProvider)
        {
            schemaProvider.SetTables(new[] {"dbo", "Users", "BASE TABLE"});

            schemaProvider.SetColumns(new object[] {"dbo", "Users", "Id", true},
                                      new[] {"dbo", "Users", "Name"},
                                      new[] {"dbo", "Users", "Password"},
                                      new[] {"dbo", "Users", "Age"});
        }
         
        [Test]
        public void GetCountBasic()
        {
            EatException<SimpleDataException>(() => _db.Users.GetCount());
            GeneratedSqlIs("select count(*) from [dbo].[users]");
        }

        [Test]
        public void QueryCountBasic()
        {
            EatException<SimpleDataException>(() => _db.Users.All().Count());
            GeneratedSqlIs("select count(*) from [dbo].[users]");
        }

        [Test]
        public void GetCountByBasic()
        {
            EatException<SimpleDataException>(() => _db.Users.GetCountByAge(42));
            GeneratedSqlIs("select count(*) from [dbo].[users] where [dbo].[users].[age] = @p1");
        }

        [Test]
        public void GetCountByNamedParameters()
        {
            EatException<SimpleDataException>(() => _db.Users.GetCountBy(Age: 42));
            GeneratedSqlIs("select count(*) from [dbo].[users] where [dbo].[users].[age] = @p1");
        }

        [Test]
        public void GetCountWithColumnThrowsException()
        {
            Assert.Throws<BadExpressionException>(() => _db.Users.GetCount(_db.Users.Id));
        }

        [Test]
        public void AssigningToColumnThrowsException()
        {
            Assert.Throws<BadExpressionException>(() => _db.Users.GetCount(_db.Users.Id = 1));
        }
        
        [Test]
        public void MultipleArgumentsThrowsException()
        {
            Assert.Throws<ArgumentException>(() => _db.Users.GetCount(_db.Users.Id == 1, _db.Users.Name == "Bob"));
        }
    }
}