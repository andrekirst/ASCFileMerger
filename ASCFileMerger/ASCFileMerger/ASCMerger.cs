using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASCFileMerger
{
    public class ASCMerger
    {
        private string columnName;
        private string[] filenames;

        public ASCMerger(string[] filenames, string columnName)
        {
            this.filenames = filenames;
            this.columnName = columnName;
        }

        private Encoding GetEncoding(string filename)
        {
            // Read the BOM
            var bom = new byte[4];
            using(var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                file.Read(bom, 0, 4);
            }

            // Analyze the BOM
            if(bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76)
                return Encoding.UTF7;
            if(bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf)
                return Encoding.UTF8;
            if(bom[0] == 0xff && bom[1] == 0xfe)
                return Encoding.Unicode; //UTF-16LE
            if(bom[0] == 0xfe && bom[1] == 0xff)
                return Encoding.BigEndianUnicode; //UTF-16BE
            if(bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff)
                return Encoding.UTF32;
            return Encoding.ASCII;
        }

        public string GeneriereTextAusgabe()
        {
            List<List<string>> datensaetze = DateienAuslesenUndInDatensaetzeSpeichern();

            List<string> liste = MergeListListString(werte: datensaetze, trenner: ";").FirstOrDefault();

            return string.Join(Environment.NewLine, liste);

            //int minAnzahl = datensaetze.Min(d => d.Werte.Count);
            //int maxAnzahl = datensaetze.Max(d => d.Werte.Count);

            //if(minAnzahl != maxAnzahl)
            //{
            //    return String.Empty;
            //}

            //string rueckgabe = String.Empty;

            //for(int i = 0; i < datensaetze.Count; i++)
            //{
            //    rueckgabe += String.Format("{0}{1}{0}{2}", string.Empty, datensaetze[i].Spaltenname, i == datensaetze.Count - 1 ? string.Empty : ";");
            //}

            //for(int indexWerte = 0; indexWerte < minAnzahl; indexWerte++)
            //{
            //    rueckgabe += Environment.NewLine;
            //    for(int indexDatensaetze = 0; indexDatensaetze < datensaetze.Count; indexDatensaetze++)
            //    {
            //        rueckgabe += String.Format("{0}{1}", datensaetze[indexDatensaetze].Werte[indexWerte], indexDatensaetze == datensaetze.Count - 1 ? String.Empty : ";");
            //    }
            //}

            //return rueckgabe;
        }

        public static List<List<string>> MergeListListString(List<List<string>> werte, string trenner)
        {
            if(werte.Count() == 1)
            {
                return werte;
            }
            else
            {
                List<List<string>> neueWerte = new List<List<string>>();
                for(int i = 0; i < werte.Count(); i += 2)
                {
                    if(werte.Count == i + 1)
                    {
                        neueWerte.Add(werte[i]);
                    }
                    else
                    {
                        IEnumerable<string> links = werte[i];
                        IEnumerable<string> rechts = werte[i + 1];

                        List<string> ergebnis = Merge(links: links, rechts: rechts, trenner: trenner).ToList();
                        neueWerte.Add(ergebnis);

                    }
                }

                return MergeListListString(werte: neueWerte, trenner: trenner);
            }
        }

        public static IEnumerable<string> Merge(IEnumerable<string> links, IEnumerable<string> rechts, string trenner)
        {
            for(int i = 0; i < links.Count(); i++)
            {
                yield return $"{links.ElementAt(i)}{trenner}{rechts.ElementAt(i)}";
            }
        }

        public List<List<string>> DateienAuslesenUndInDatensaetzeSpeichern()
        {
            if(String.IsNullOrEmpty(columnName))
            {
                throw new ArgumentException("Attribut für Spaltenname nicht angegeben");
            }

            List<List<string>> datensaetze = new List<List<string>>();
            foreach(string file in filenames)
            {
                List<string> aktuellerDatensatz = new List<string>();

                Encoding fileEncoding = GetEncoding(file);

                bool headerGefunden = false;

                string[] lines = File.ReadAllLines(file, fileEncoding);
                foreach(string line in lines)
                {
                    if(line.StartsWith(columnName))
                    {
                        aktuellerDatensatz.Add(Regex.Replace(line, string.Format(@"{0}[\ ]*", columnName), String.Empty).Replace("\t", string.Empty));
                        headerGefunden = true;
                    }
                    double wert;
                    if(Double.TryParse(line, out wert))
                    {
                        //aktuellerDatensatz.Werte.Add(wert);
                        aktuellerDatensatz.Add(line);
                    }
                }
                if(!headerGefunden)
                {
                    throw new ArgumentException("Attribut für Spaltenname nicht gefunden");
                }
                datensaetze.Add(aktuellerDatensatz);
            };
            return datensaetze;
        }
    }
}