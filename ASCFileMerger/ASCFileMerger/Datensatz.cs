using System.Collections.Generic;
using System.Linq;

namespace ASCFileMerger
{
    public class Datensatz
    {
        public Datensatz()
        {
            Werte = new List<double>();
        }

        public string Spaltenname
        {
            get;
            set;
        }
        public List<double> Werte
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            Datensatz others = obj as Datensatz;
            return this.Spaltenname == others.Spaltenname &&
                this.Werte.SequenceEqual(others.Werte);
        }
    }
}