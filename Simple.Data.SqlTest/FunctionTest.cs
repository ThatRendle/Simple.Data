﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shitty.Data.SqlTest
{
    using NUnit.Framework;

    [TestFixture]
    public class FunctionTest
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            DatabaseHelper.Reset();
        }

        [Test]
        public void CoalesceFunctionWorks()
        {
            var db = DatabaseHelper.Open();
            var date = new DateTime(1900, 1, 1);
            List<dynamic> q = db.Orders.Query(db.Orders.OrderDate.Coalesce(date) < DateTime.Now).ToList();
            Assert.AreNotEqual(0, q.Count);
        }
    }
}
