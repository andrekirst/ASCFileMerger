using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ASCFileMerger
{
    public class ASCMerger
    {
        private string columnName;
        private IEnumerable<string> filenames;

        public ASCMerger(IEnumerable<string> filenames, string columnName)
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
        }

        public static List<List<string>> MergeListListString(List<List<string>> werte, string trenner)
        {
            if(werte.Count() == 1)
            {
                return werte;
            }
            else
            {
                List<List<string>> neueWerte = new List<List<string>>((werte.Count() / 2) + 1);

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
                yield return links.ElementAt(i) + trenner + rechts.ElementAt(i);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException" />
        public List<List<string>> DateienAuslesenUndInDatensaetzeSpeichern()
        {
            if(String.IsNullOrEmpty(columnName))
            {
                throw new ArgumentException("Attribut für Spaltenname nicht angegeben");
            }
            object locker = new object();
            Dictionary<int, List<string>> datensaetze = new Dictionary<int, List<string>>();

            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = -1
            };

            int char0 = '0';
            int char9 = '9';

            Parallel.ForEach(filenames, options, (file, state, currentIndex) =>
            {
                List<string> aktuellerDatensatz = new List<string>();

                Encoding fileEncoding = GetEncoding(file);

                bool headerGefunden = false;
                bool datenGefunden = false;

                IEnumerable<string> lines = File.ReadLines(file, fileEncoding);
                foreach(string line in lines)
                {
                    if(!headerGefunden && line.StartsWith(columnName))
                    {
                        aktuellerDatensatz.Add(Regex.Replace(line, string.Format(@"{0}[\ ]*", columnName), String.Empty).Replace("\t", string.Empty));
                        headerGefunden = true;
                    }
                    else
                    {
                        if(datenGefunden)
                        {
                            aktuellerDatensatz.Add(line);
                        }
                        else
                        {
                            char firstChar = line.First();
                            int firstCharInt = firstChar;

                            if(firstChar == '-' || (firstCharInt >= char0 && firstCharInt <= char9))
                            {
                                aktuellerDatensatz.Add(line);
                                datenGefunden = true;
                            }
                        }
                    }
                }
                if(!headerGefunden)
                {
                    throw new ArgumentException("Attribut für Spaltenname nicht gefunden");
                }
                lock(locker)
                {
                    datensaetze.Add((int)currentIndex, aktuellerDatensatz);
                }
            });

            return datensaetze.OrderBy(s => s.Key).Select(s => s.Value).ToList();
        }
    }
}