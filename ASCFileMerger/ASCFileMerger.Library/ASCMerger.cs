using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ASCFileMerger.Library
{
    public class ASCMerger
    {
        private string _columnName;
        private List<string> _filenames;

        public ASCMerger(List<string> filenames, string columnName)
        {
            _filenames = filenames;
            _columnName = columnName;
        }

        public Encoding GetEncoding(string filename)
        {
            // Read the BOM
            var bom = new byte[4];
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                file.Read(array: bom, offset: 0, count: 4);
            }

            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76)
            {
                return Encoding.UTF7;
            }

            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf)
            {
                return Encoding.UTF8;
            }

            if (bom[0] == 0xff && bom[1] == 0xfe)
            {
                return Encoding.Unicode; //UTF-16LE
            }

            if (bom[0] == 0xfe && bom[1] == 0xff)
            {
                return Encoding.BigEndianUnicode; //UTF-16BE
            }

            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff)
            {
                return Encoding.UTF32;
            }

            return Encoding.ASCII;
        }

        public string GeneriereTextAusgabe()
        {
            List<List<string>> datensaetze = DateienAuslesenUndInDatensaetzeSpeichern();

            List<string> liste = MergeListListString(werte: datensaetze, trenner: ";")[0];
            
            string content = string.Join(Environment.NewLine, liste);
            return content;
        }

        public static List<List<string>> MergeListListString(List<List<string>> werte, string trenner)
        {
            if (werte.Count == 1)
            {
                return werte;
            }
            else
            {
                List<List<string>> neueWerte = new List<List<string>>((werte.Count / 2) + 1);

                for (int i = 0; i < werte.Count; i += 2)
                {
                    if (werte.Count == i + 1)
                    {
                        neueWerte.Add(werte[i]);
                    }
                    else
                    {
                        List<string> links = werte[i];
                        List<string> rechts = werte[i + 1];

                        List<string> ergebnis = Merge(links: links, rechts: rechts, trenner: trenner).ToList();
                        neueWerte.Add(item: ergebnis);
                    }
                }

                return MergeListListString(werte: neueWerte, trenner: trenner);
            }
        }

        public static List<string> Merge(List<string> links, List<string> rechts, string trenner)
        {
            int count = links.Count;
            List<string> werte = new List<string>(count);
            for (int i = 0; i < count; i++)
            {
                werte.Add(links[i] + trenner + rechts[i]);
            }
            return werte;
        }

        public List<List<string>> DateienAuslesenUndInDatensaetzeSpeichern()
        {
            if (String.IsNullOrEmpty(_columnName))
            {
                throw new ArgumentException("Attribut für Spaltenname nicht angegeben");
            }
            object locker = new object();
            List<List<string>> datensaetze = new List<List<string>>();

            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = -1
            };

            int char0 = '0';
            int char9 = '9';

            foreach (string file in _filenames)
            {
                List<string> aktuellerDatensatz = new List<string>();
                Encoding fileEncoding = GetEncoding(file);

                bool headerGefunden = false;
                bool datenGefunden = false;

                string[] lines = File.ReadAllLines(file, fileEncoding);

                foreach (string line in lines)
                {
                    if (!headerGefunden && line.StartsWith(_columnName))
                    {
                        aktuellerDatensatz.Add(item: Regex.Replace(
                            input: line,
                            pattern: $@"{_columnName}[\ ]*",
                            replacement: String.Empty,
                            options: RegexOptions.Compiled)
                            .Replace("\t", string.Empty));
                        headerGefunden = true;
                    }
                    else
                    {
                        if (datenGefunden)
                        {
                            aktuellerDatensatz.Add(line);
                        }
                        else
                        {
                            char firstChar = line[0];
                            int firstCharInt = firstChar;

                            if (firstChar == '-' || (firstCharInt >= char0 && firstCharInt <= char9))
                            {
                                aktuellerDatensatz.Add(line);
                                datenGefunden = true;
                            }
                        }
                    }
                }

                if (!headerGefunden)
                {
                    throw new ArgumentException("Attribut für Spaltenname nicht gefunden");
                }

                lock (locker)
                {
                    //datensaetze.TryAdd((int)currentIndex, aktuellerDatensatz);
                    datensaetze.Add(aktuellerDatensatz);
                }
            }

            return datensaetze;
        }
    }
}