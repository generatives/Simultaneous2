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
    public class ClusteringTests
    {
        public ClusteringTests()
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
        public void ThreeMemberClusterSetup()
        {
            var host = NetworkMember.Start(10000);
            EggTimer.Until(10, () => host.Update());
            var firstClient = NetworkMember.Join("localhost", 10000);
            EggTimer.Until(500, () =>
            {
                host.Update();
                firstClient.Update();
                return firstClient.State == MemberState.JOINED;
            });

            Assert.AreEqual(MemberState.JOINED, firstClient.State);
            Assert.AreEqual(1, host.RemoteMemberIds.Count);
            Assert.AreEqual(1, firstClient.RemoteMemberIds.Count);
            Assert.IsTrue(host.KnowsOfOnly(firstClient));
            Assert.IsTrue(firstClient.KnowsOfOnly(host));

            var secondClient = NetworkMember.Join("localhost", 10000);
            EggTimer.Until(500, () =>
            {
                host.Update();
                firstClient.Update();
                secondClient.Update();
                return secondClient.State == MemberState.JOINED && firstClient.RemoteMemberIds.Count == 2;
            });

            Assert.AreEqual(MemberState.JOINED, secondClient.State);

            Assert.AreEqual(2, host.RemoteMemberIds.Count);
            Assert.AreEqual(2, secondClient.RemoteMemberIds.Count);
            Assert.AreEqual(2, firstClient.RemoteMemberIds.Count);

            Assert.IsTrue(host.KnowsOfOnly(firstClient, secondClient));
            Assert.IsTrue(firstClient.KnowsOfOnly(host, secondClient));
            Assert.IsTrue(secondClient.KnowsOfOnly(host, firstClient));
        }
    }
}
