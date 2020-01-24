using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AspNetFileNicluder.Logic.Util;
using AspNetFileNicluder.Logic.ChangeConstant;
using UnitTest.Util;

namespace UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var settings = GetSettings();
            Assert.AreEqual(settings?.ChangeConstants?.Count ?? 0, 1);
        }

        [TestMethod]
        public void CheckFileExecuter()
        {
            var setting = GetSettings();
            ChangeFiles chackFile = new ChangeFiles(setting, TestAppOutput.ConsoleWriteLine, TestAppOutput.ConsoleWriteException, TestFile.WriteAllText);
            var executeResults = chackFile.Execute(setting.ChangeConstants);
            Assert.IsTrue(executeResults);
        }

        private Settings GetSettings(string path = null)
        {
            string filePath = path ?? @"C:\Users\gogi_\source\AspNetFileNicluder\AspNetFileNicluder\ExapleData\anfnConfig-text.json";
            return new AspNetFileNicluder.Logic.Util.Settings(filePath);
        }
    }
}
