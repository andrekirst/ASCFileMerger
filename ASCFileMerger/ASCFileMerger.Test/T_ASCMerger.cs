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
            string[] fileNames = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\TestDaten");

            ASCMerger merger = new ASCMerger(filenames: fileNames, columnName: "ColumnName");

            List<Datensatz> actual = merger.DateienAuslesenUndInDatensaetzeSpeichern();

            List<Datensatz> expected = new List<Datensatz>()
            {
                new Datensatz() { Spaltenname = "Spalte1", Werte = new List<double>() { -2, -3, -2 } },
                new Datensatz() { Spaltenname = "Spalte2", Werte = new List<double>() { -3, -2, -2 } }
            };

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Merge_2_Arrays_in_ein_Array_mit_jeweils_1_Header_1_Datenzeile()
        {
            string[] links = new string[] { "\"HeaderLinks\"", "\"Wert1\"" };
            string[] rechts = new string[] { "\"HeaderRechts\"", "\"Wert2\"" };
            string trenner = ";";

            IEnumerable<string> expected = (new string[] { "\"HeaderLinks\";\"HeaderRechts\"", "\"Wert1\";\"Wert2\"" }).AsEnumerable();
            IEnumerable<string> actual = ASCMerger.Merge(links: links, rechts: rechts, trenner: trenner);

            CollectionAssert.AreEqual(expected.ToList(), actual.ToList());
        }
    }
}
