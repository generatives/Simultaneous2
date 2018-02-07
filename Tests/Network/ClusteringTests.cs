﻿using System;
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
            var host = ObjectSpace.Start(10000);
            EggTimer.Until(10, () => host.Update());
            var firstClient = ObjectSpace.Join("localhost", 10000);
            EggTimer.Until(3000, () =>
            {
                host.Update();
                firstClient.Update();
                return firstClient.State == MemberState.JOINED;
            });

            Assert.AreEqual(MemberState.JOINED, firstClient.State);
            Assert.AreEqual(2, host.SubSpaces.Count());
            Assert.AreEqual(2, firstClient.SubSpaces.Count());
            Assert.IsTrue(host.KnowsOfOnly(firstClient));
            Assert.IsTrue(firstClient.KnowsOfOnly(host));

            var secondClient = ObjectSpace.Join("localhost", 10000);
            EggTimer.Until(3000, () =>
            {
                host.Update();
                firstClient.Update();
                secondClient.Update();
                return secondClient.State == MemberState.JOINED && firstClient.SubSpaces.Count() == 3;
            });

            Assert.AreEqual(MemberState.JOINED, secondClient.State);

            Assert.AreEqual(3, host.SubSpaces.Count());
            Assert.AreEqual(3, secondClient.SubSpaces.Count());
            Assert.AreEqual(3, firstClient.SubSpaces.Count());

            Assert.IsTrue(host.KnowsOfOnly(firstClient, secondClient));
            Assert.IsTrue(firstClient.KnowsOfOnly(host, secondClient));
            Assert.IsTrue(secondClient.KnowsOfOnly(host, firstClient));
        }
    }
}
