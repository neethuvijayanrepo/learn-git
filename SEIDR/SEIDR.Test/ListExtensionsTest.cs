using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SEIDR.Test
{
    [TestClass]
    public class ListExtensionsTest
    {
        [TestMethod]
        public void InsertTest()
        {
            List<string> test = new List<string>();
            test.InsertWithExpansion(10, "TEST");
            Assert.AreEqual("TEST", test[10]);
        }


        [TestMethod]
        public void InsertTestInt()
        {
            List<int> test = new List<int>();
            test.InsertWithExpansion(10, 30);
            Assert.AreEqual(30, test[10]);
            Assert.AreEqual(default(int), test[0]);
        }
        [TestMethod]
        public void InsertTestBigList()
        {
            List<string> test = new List<string>(new string[30]);
            test.SetWithExpansion(5, "TEST");
            Assert.AreEqual("TEST", test[5]);
            Assert.AreEqual(30, test.Count);
        }

    }
}
