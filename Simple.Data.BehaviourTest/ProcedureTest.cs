﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Shitty.Data.Ado;
using Shitty.Data.Mocking.Ado;

// ReSharper disable InconsistentNaming

namespace Shitty.Data.IntegrationTest
{
    [TestFixture]
    public class ProcedureTest
    {
        static dynamic CreateDatabase(MockDatabase mockDatabase)
        {
            var mockSchemaProvider = new MockSchemaProvider();
            mockSchemaProvider.SetProcedures(new[] { "dbo", "ProcedureWithParameters" },
                new[] { "dbo", "ProcedureWithoutParameters"},new[] { "foo", "ProcedureInAnotherSchema"});
            mockSchemaProvider.SetParameters(new[] { "dbo", "ProcedureWithParameters", "@One" },
                                          new[] { "dbo", "ProcedureWithParameters", "@Two" });
            return new Database(new AdoAdapter(new MockConnectionProvider(new MockDbConnection(mockDatabase), mockSchemaProvider)));
        }

        [Test]
        public void CheckMockWorking()
        {
            var mockDatabase = new MockDatabase();
            var db = CreateDatabase(mockDatabase);
            Assert.NotNull(db);
        }

        [Test]
        public void CallingMethodOnDatabase_Should_CallProcedure()
        {
            var mockDatabase = new MockDatabase();
            var db = CreateDatabase(mockDatabase);
            db.ProcedureWithoutParameters();
            Assert.IsNotNull(mockDatabase.Sql);
            Assert.AreEqual("[dbo].[ProcedureWithoutParameters]".ToLowerInvariant(), mockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual(0, mockDatabase.Parameters.Count());
        }

        [Test]
        public void CallingMethodOnDatabase_WithNamedParameters_Should_CallProcedure()
        {
            var mockDatabase = new MockDatabase();
            var db = CreateDatabase(mockDatabase);
            db.ProcedureWithParameters(One: 1, Two: 2);
            Assert.AreEqual(1, mockDatabase.CommandTexts.Count);
            Assert.AreEqual("[dbo].[ProcedureWithParameters]".ToLowerInvariant(), mockDatabase.CommandTexts[0].ToLowerInvariant());
            Assert.AreEqual(2, mockDatabase.CommandParameters[0].Count);
            Assert.IsTrue(mockDatabase.CommandParameters[0].ContainsKey("@One"));
            Assert.AreEqual(1, mockDatabase.CommandParameters[0]["@One"]);
            Assert.IsTrue(mockDatabase.CommandParameters[0].ContainsKey("@Two"));
            Assert.AreEqual(2, mockDatabase.CommandParameters[0]["@Two"]);
        }

        [Test]
        public void CallingMethodOnDatabase_WithPositionalParameters_Should_CallProcedure()
        {
            var mockDatabase = new MockDatabase();
            var db = CreateDatabase(mockDatabase);
            db.ProcedureWithParameters(1, 2);
            Assert.AreEqual(1, mockDatabase.CommandTexts.Count);
            Assert.AreEqual("[dbo].[ProcedureWithParameters]".ToLowerInvariant(), mockDatabase.CommandTexts[0].ToLowerInvariant());
            Assert.AreEqual(2, mockDatabase.CommandParameters[0].Count);
            Assert.IsTrue(mockDatabase.CommandParameters[0].ContainsKey("@One"));
            Assert.AreEqual(1, mockDatabase.CommandParameters[0]["@One"]);
            Assert.IsTrue(mockDatabase.CommandParameters[0].ContainsKey("@Two"));
            Assert.AreEqual(2, mockDatabase.CommandParameters[0]["@Two"]);
        }

        [Test]
        public void CallingMethodOnObjectInDatabase_Should_CallProcedure()
        {
            var mockDatabase = new MockDatabase();
            var db = CreateDatabase(mockDatabase);
            db.foo.ProcedureInAnotherSchema();
            Assert.IsNotNull(mockDatabase.Sql);
            Assert.AreEqual("[foo].[ProcedureInAnotherSchema]".ToLowerInvariant(), mockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual(0, mockDatabase.Parameters.Count());
        }
    }
}
