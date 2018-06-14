using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ASCFileMerger.Test
{
    [TestClass]
    public class T_ASCMerger
    {
        [TestMethod]
        public void ASCMerger_Lese_Dateien_aus_und_speichere_Datensatz()
        {
            List<string> fileNames = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\TestDaten").ToList();

            ASCMerger merger = new ASCMerger(filenames: fileNames, columnName: "ColumnName");

            List<List<string>> actual = merger.DateienAuslesenUndInDatensaetzeSpeichern();

            List<List<string>> expected = new List<List<string>>()
            {
                new List<string>() { "Spalte1", "-2", "-3", "-2" },
                new List<string>() { "Spalte2", "-3", "-2", "-2" }
            };

            equalidator.Equalidator.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Merge_2_Arrays_in_ein_Array_mit_jeweils_1_Header_1_Datenzeile()
        {
            List<string> links = new string[] { "\"HeaderLinks\"", "\"Wert1\"" }.ToList();
            List<string> rechts = new string[] { "\"HeaderRechts\"", "\"Wert2\"" }.ToList();
            string trenner = ";";

            IEnumerable<string> expected = (new string[] { "\"HeaderLinks\";\"HeaderRechts\"", "\"Wert1\";\"Wert2\"" }).AsEnumerable();
            IEnumerable<string> actual = ASCMerger.Merge(links: links, rechts: rechts, trenner: trenner);

            CollectionAssert.AreEqual(expected.ToList(), actual.ToList());
        }

        [TestMethod]
        public void MergeListListString_3_Einzel_List_Erwarte_Eine_List()
        {
            List<List<string>> werte = new List<List<string>>()
            {
              new List<string>() { "\"A\"", "\"1\"", "\"2\"" },
              new List<string>() { "\"B\"", "\"3\"", "\"4\"" },
              new List<string>() { "\"C\"", "\"5\"", "\"6\"" }
            };

            List<List<string>> expected = new List<List<string>>()
            {
              new List<string>() { "\"A\";\"B\";\"C\"", "\"1\";\"3\";\"5\"", "\"2\";\"4\";\"6\"" }
            };

            List<List<string>> actual = ASCMerger.MergeListListString(werte: werte, trenner: ";");

            CollectionAssert.AreEqual(expected.First(), actual.First());
        }

        [TestMethod]
        public void MergeListListString_2_Einzel_List_Erwarte_Eine_List()
        {
            List<List<string>> werte = new List<List<string>>()
            {
              new List<string>() { "\"A\"", "\"1\"", "\"2\"" },
              new List<string>() { "\"B\"", "\"3\"", "\"4\"" }
            };

            List<List<string>> expected = new List<List<string>>()
            {
              new List<string>() { "\"A\";\"B\"", "\"1\";\"3\"", "\"2\";\"4\"" }
            };

            List<List<string>> actual = ASCMerger.MergeListListString(werte: werte, trenner: ";");

            CollectionAssert.AreEqual(expected.First(), actual.First());
        }
    }
}
