using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;

namespace ASCFileMerger
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
            using(FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read))
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

            List<string> liste = MergeListListString(werte: datensaetze, trenner: ";")[0];
            
            string content = string.Join(Environment.NewLine, liste);
            return content;
        }

        public static List<List<string>> MergeListListString(List<List<string>> werte, string trenner)
        {
            if(werte.Count() == 1)
            {
                return werte;
            }
            else
            {
                int werteCount = werte.Count;
                List<List<string>> neueWerte = new List<List<string>>((werteCount / 2) + 1);

                for(int i = 0; i < werteCount; i += 2)
                {
                    if(werteCount == i + 1)
                    {
                        neueWerte.Add(werte[i]);
                    }
                    else
                    {
                        List<string> links = werte[i];
                        List<string> rechts = werte[i + 1];

                        List<string> ergebnis = Merge(links: links, rechts: rechts, trenner: trenner);
                        neueWerte.Add(ergebnis);
                    }
                }

                return MergeListListString(werte: neueWerte, trenner: trenner);
            }
        }

        public static List<string> Merge(List<string> links, List<string> rechts, string trenner)
        {
            int count = links.Count;
            List<string> werte = new List<string>(count);
            for(int i = 0; i < count; i++)
            {
                werte.Add(links[i] + trenner + rechts[i]);
            }
            return werte;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException" />
        public List<List<string>> DateienAuslesenUndInDatensaetzeSpeichern()
        {
            if(String.IsNullOrEmpty(_columnName))
            {
                throw new ArgumentException("Attribut für Spaltenname nicht angegeben");
            }
            List<List<string>> datensaetze = new List<List<string>>();

            int char0 = '0';
            int char9 = '9';

            foreach(string file in _filenames)
            {
                List<string> aktuellerDatensatz = new List<string>();
                Encoding fileEncoding = GetEncoding(file);

                bool headerGefunden = false;
                bool datenGefunden = false;

                string[] lines = File.ReadAllLines(file, fileEncoding);

                foreach (string line in lines)
                {
                    if(!headerGefunden && line.StartsWith(_columnName))
                    {
                        aktuellerDatensatz.Add(Regex.Replace(line, string.Format(@"{0}[\ ]*", _columnName), String.Empty, RegexOptions.Compiled).Replace("\t", string.Empty));
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
                            char firstChar = line[0];
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

                datensaetze.Add(aktuellerDatensatz);
            }

            return datensaetze;
        }
    }
}