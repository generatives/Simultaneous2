using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimultaneousNetwork;
using System.Linq;

namespace Tests.Network
{
    /// <summary>
    /// Summary description for ClusteringTests
    /// </summary>
    [TestClass]
    public class NetObjCommsTests
    {
        public NetObjCommsTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TwoMemTwoObjEcho()
        {
            var members = Utils.StartLocalNetwork(10000, 2);

            var mem0 = members[0];
            var mem1 = members[1];

            Utils.AddNetObj(mem0, (id, space) => new EchoNetObj(id, space), members);
            Utils.AddNetObj(mem1, (id, space) => new GreetNetObj(id, space), members);

            EggTimer.Until(5000, () =>
            {
                members.ForEach(mem => mem.Update());
            });
        }
    }
}
