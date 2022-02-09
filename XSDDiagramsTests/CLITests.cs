using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using XSDDiagramConsole;

namespace XSDDiagramsTests
{
    [TestClass]
    public class CLITests
    {
        [TestMethod]
        public void RandomArgsGivesUsage()
        {
            string[] args = { "a", "b", "c" };
            var options = new Options(args);
            Program.Execute(options);
        }
    }
}
