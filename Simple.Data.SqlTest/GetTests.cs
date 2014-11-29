using NUnit.Framework;

namespace Simple.Data.SqlTest
{
    using System;
    using System.Linq;

    [TestFixture]
    public class GetTests
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            DatabaseHelper.Reset();
        }

        [Test]
        public void TestGet()
        {
            var db = DatabaseHelper.Open();
            var user = db.Users.Get(1);
            Assert.AreEqual(1, user.Id);
        }

        [Test]
        public void GetWithNonExistentPrimaryKeyShouldReturnNull()
        {
            var db = DatabaseHelper.Open();
            var user = db.Users.Get(1138);
            Assert.IsNull(user);
        }

        [Test]
        public void GetWithSelectClauseShouldRestrictColumn()
        {
            var db = DatabaseHelper.Open();
           
            var actual = db.Users.Select(db.Users.Name, db.Users.Age).Get(1);
            
            Assert.AreEqual("Bob", actual.Name);
            Assert.AreEqual(32, actual.Age);
            Assert.Throws<UnresolvableObjectException>(() => Console.WriteLine(actual.Id), "Column 'Id' not found.");
            Assert.Throws<UnresolvableObjectException>(() => Console.WriteLine(actual.Password), "Column 'Password' not found.");
        }

        [Test]
        public void GetWithMisplacedSelectClauseThrowsException()
        {
            var db = DatabaseHelper.Open();
            Assert.Throws<UnresolvableObjectException>(
                        () => db.Users.Get(1).Select(db.Users.Name, db.Users.Age));
        }

        [Test]
        public void SelectClauseWithGetScalarShouldLimitQuery()
        {
            var db = DatabaseHelper.Open();
            string actual = db.Customers.Select(db.Customers.Name).GetScalar(1);
            Assert.IsNotNull(actual);
            Assert.AreEqual("Test", actual);
        }

        [Test]
        public void WithClauseShouldCastToStaticTypeWithComplexProperty()
        {
            var db = DatabaseHelper.Open();
            Order actual = db.Orders.WithCustomer().Get(1);
            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.Customer);
            Assert.AreEqual("Test", actual.Customer.Name);
            Assert.AreEqual("100 Road", actual.Customer.Address);
        }

        [Test]
        public void WithClauseShouldCastToStaticTypeWithCollection()
        {
            var db = DatabaseHelper.Open();
            Customer actual = db.Customers.WithOrders().Get(1);
            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.Orders.Single().OrderId);
            Assert.AreEqual(new DateTime(2010, 10, 10), actual.Orders.Single().OrderDate);
        }
    }
}