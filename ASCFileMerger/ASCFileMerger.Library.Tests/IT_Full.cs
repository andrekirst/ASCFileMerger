using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace ASCFileMerger.Library.Test
{
    public class IT_Full
    {
        [Fact(DisplayName = "IntegrationTest")]
        public void IT_Full_Merge_2_Files()
        {
            List<string> fileNames = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\TestDaten").ToList();

            ASCMerger merger = new ASCMerger(filenames: fileNames, columnName: "ColumnName");

            string actual = merger.GeneriereTextAusgabe();

            string expected = "Spalte1;Spalte2" + Environment.NewLine +
                "-2;-3" + Environment.NewLine +
                "-3;-2" + Environment.NewLine +
                "-2;-2";

            //Assert.AreEqual(expected, actual);
            actual.ShouldBe(expected);
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

        [Fact(DisplayName = "IntegrationTest")]
        public void IT_Full_Merge_8_Files_10000_Zeilen()
        {
            List<string> fileNames = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\TestDaten_Anzahl_10000").ToList();

            ASCMerger merger = new ASCMerger(filenames: fileNames, columnName: "ColumnName");

            int actual = Count4(merger.GeneriereTextAusgabe());

            int expected = 10001;

            actual.ShouldBe(expected);
        }
    }
}
