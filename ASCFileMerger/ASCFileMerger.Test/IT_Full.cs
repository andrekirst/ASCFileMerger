using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace ASCFileMerger.Test
{
    [TestClass]
    public class IT_Full
    {
        [TestMethod, TestCategory("IntegrationTest")]
        public void IT_Full_Merge_2_Files()
        {
            string[] fileNames = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\TestDaten");

            ASCMerger merger = new ASCMerger(filenames: fileNames, columnName: "ColumnName");

            string actual = merger.GeneriereTextAusgabe();

            string expected = "Spalte1,Spalte2" + Environment.NewLine +
                "-2,-3" + Environment.NewLine +
                "-3,-2" + Environment.NewLine +
                "-2,-2";

            Assert.AreEqual(expected, actual);
        }

        private static int Count4(string s)
        {
            int n = 0;
            foreach(var c in s)
            {
                if(c == '\n')
                    n++;
            }
            return n + 1;
        }

        [TestMethod, TestCategory("IntegrationTest")]
        public void IT_Full_Merge_8_Files_10000_Zeilen()
        {
            string[] fileNames = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\TestDaten_Anzahl_10000");

            ASCMerger merger = new ASCMerger(filenames: fileNames, columnName: "ColumnName");

            int actual = Count4(merger.GeneriereTextAusgabe());

            int expected = 10001;

            Assert.AreEqual(expected, actual);
        }
    }
}
