using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.DemoMap;

namespace JobUnitTest.DemoMap
{
    [TestClass]
    public class NameHelperTest
    {
        [TestMethod]
        public void TestExtraction()
        {
            NameHelper v = new NameHelper("JingleHeimer-Smith", "John", "Jacob");
            Assert.AreEqual("JingleHeimer-Smith, John Jacob", v.ComputedName);
            Assert.AreEqual("JingleHeimer-Smith", v.ExtractLastName());
            Assert.AreEqual("J", v.ExtractMI(false));
            Assert.IsNull(v.ExtractMI(true));
            Assert.AreEqual("John", v.FirstName);
            Assert.AreEqual("Jacob", v.ExtractMiddleName());

            v = new NameHelper("John Jacob JingleHeimer-Smith", null ,string.Empty);
            Assert.IsNull(v.MiddleOriginal);
            Assert.AreEqual("John", v.FirstName);
            Assert.AreEqual("JingleHeimer-Smith", v.LastName);
            Assert.AreEqual("J", v.ExtractMI());
            Assert.AreEqual("Jacob", v.ExtractMiddleName());

            v = new NameHelper("JingleHeimer-SMith", "John Jacob", null);
            Assert.AreEqual("John", v.FirstName);
            Assert.AreEqual(v.LastNameOriginal, v.LastName);
            Assert.AreEqual("Jacob", v.ExtractMiddleName());
            Assert.AreEqual("J", v.ExtractMI());
        }
    }
}
