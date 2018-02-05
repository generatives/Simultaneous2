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
    public class NetObjPropegateTests
    {
        public NetObjPropegateTests()
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
        public void ThreeMemberThreeObj()
        {
            var members = Utils.StartLocalNetwork(10000, 3);

            var mem0 = members[0];
            var mem1 = members[1];
            var mem2 = members[2];

            mem0.AddNetObj((id, space) => new TestNetObj(id, space));

            EggTimer.Until(100, () =>
            {
                mem0.Update();
                mem1.Update();
                mem2.Update();
                return mem0.NetObjs.Count() == 1 &&
                    mem1.NetObjs.Count() == 1 &&
                    mem2.NetObjs.Count() == 1;
            });

            Assert.AreEqual(1, mem0.NetObjs.Count());
            Assert.AreEqual(1, mem1.NetObjs.Count());
            Assert.AreEqual(1, mem2.NetObjs.Count());

            mem1.AddNetObj((id, space) => new TestNetObj(id, space));

            EggTimer.Until(100, () =>
            {
                mem0.Update();
                mem1.Update();
                mem2.Update();
                return mem0.NetObjs.Count() == 2 &&
                    mem1.NetObjs.Count() == 2 &&
                    mem2.NetObjs.Count() == 2;
            });

            Assert.AreEqual(2, mem0.NetObjs.Count());
            Assert.AreEqual(2, mem1.NetObjs.Count());
            Assert.AreEqual(2, mem2.NetObjs.Count());

            mem2.AddNetObj((id, space) => new TestNetObj(id, space));

            EggTimer.Until(100, () =>
            {
                mem0.Update();
                mem1.Update();
                mem2.Update();
                return mem0.NetObjs.Count() == 3 &&
                    mem1.NetObjs.Count() == 3 &&
                    mem2.NetObjs.Count() == 3;
            });

            Assert.AreEqual(3, mem0.NetObjs.Count());
            Assert.AreEqual(3, mem1.NetObjs.Count());
            Assert.AreEqual(3, mem2.NetObjs.Count());
        }
    }
}
