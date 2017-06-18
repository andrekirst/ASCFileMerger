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
            List<Datensatz> datensaetze = DateienAuslesenUndInDatensaetzeSpeichern();

            int minAnzahl = datensaetze.Min(d => d.Werte.Count);
            int maxAnzahl = datensaetze.Max(d => d.Werte.Count);

            if(minAnzahl != maxAnzahl)
            {
                return String.Empty;
            }

            string rueckgabe = String.Empty;

            for(int i = 0; i < datensaetze.Count; i++)
            {
                rueckgabe += String.Format("{0}{1}{0}{2}", string.Empty, datensaetze[i].Spaltenname, i == datensaetze.Count - 1 ? string.Empty : ";");
            }

            for(int indexWerte = 0; indexWerte < minAnzahl; indexWerte++)
            {
                rueckgabe += Environment.NewLine;
                for(int indexDatensaetze = 0; indexDatensaetze < datensaetze.Count; indexDatensaetze++)
                {
                    rueckgabe += String.Format("{0}{1}", datensaetze[indexDatensaetze].Werte[indexWerte], indexDatensaetze == datensaetze.Count - 1 ? String.Empty : ";");
                }
            }

            return rueckgabe;
        }

        public List<Datensatz> DateienAuslesenUndInDatensaetzeSpeichern()
        {
            if(String.IsNullOrEmpty(columnName))
            {
                throw new ArgumentException("Attribut für Spaltenname nicht angegeben");
            }

            List<Datensatz> datensaetze = new List<Datensatz>();
            foreach(string file in filenames)
            {
                Datensatz aktuellerDatensatz = new Datensatz();

                Encoding fileEncoding = GetEncoding(file);

                string[] lines = File.ReadAllLines(file, fileEncoding);
                foreach(string line in lines)
                {
                    if(line.StartsWith(columnName))
                    {
                        aktuellerDatensatz.Spaltenname = Regex.Replace(line, string.Format(@"{0}[\ ]*", columnName), String.Empty).Replace("\t", string.Empty);
                    }
                    double wert;
                    if(Double.TryParse(line, out wert))
                    {
                        aktuellerDatensatz.Werte.Add(wert);
                    }
                }
                if(String.IsNullOrEmpty(aktuellerDatensatz.Spaltenname))
                {
                    throw new ArgumentException("Attribut für Spaltenname nicht gefunden");
                }
                datensaetze.Add(aktuellerDatensatz);
            }
            return datensaetze;
        }
    }
}