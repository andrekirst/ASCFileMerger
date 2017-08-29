using ASCFileMerger.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ASCFileMerger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string[] fileNames;

        public MainWindow()
        {
            InitializeComponent();

            Settings.Default.Reload();
            textBoxSpaltenname.Text = Settings.Default.DefaultColumnName;
        }

        private void buttonDateienAuswaehlen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "ASC Dateien (*.asc)|*.asc|Alle Dateien (*.*)|*.*";
            if(ofd.ShowDialog() == true)
            {
                fileNames = ofd.FileNames;
                buttonGenerierenUndSpeichern.IsEnabled = true;
                labelNDateienausgewaehlt.Content = String.Format("{0} Datei(-en) ausgewählt", fileNames.Count());
                labelErgebnis.Content = String.Empty;
            }
        }

        public ASCMerger Merger
        {
            get; set;
        }

        private void buttonGenerierenUndSpeichern_Click(object sender, RoutedEventArgs e)
        {
            Merger = new ASCFileMerger.ASCMerger(fileNames, textBoxSpaltenname.Text);

            DateiSpeichern();
            DataGridFuellen();
        }

        private void DataGridFuellen()
        {
            //try
            //{
            //    DataTable dataTable = new DataTable();

            //    List<List<string>> datensaetze = Merger.DateienAuslesenUndInDatensaetzeSpeichern();

            //    foreach(List<string> datensatz in datensaetze)
            //    {
            //        dataTable.Columns.Add(datensatz[0], typeof(double));
            //    }

            //    for(int indexWerte = 0; indexWerte < datensaetze[0].Werte.Count(); indexWerte++)
            //    {
            //        DataRow row = dataTable.NewRow();

            //        for(int indexDatensaetze = 0; indexDatensaetze < datensaetze.Count; indexDatensaetze++)
            //        {
            //            row[datensaetze[indexDatensaetze].Spaltenname] = datensaetze[indexDatensaetze].Werte[indexWerte];
            //        }
            //        dataTable.Rows.Add(row);
            //    }

            //    dataGridDatensaetze.DataContext = dataTable;
            //}
            //catch(Exception ex)
            //{

            //    throw ex;
            //}
        }

        private void DateiSpeichern()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            string content = Merger.GeneriereTextAusgabe();
            sw.Stop();

            FileInfo fi = new FileInfo(fileNames[0]);

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV Dateien (*.csv)|*.csv|Alle Dateien (*.*)|*.*";
            sfd.InitialDirectory = fi.DirectoryName;
            sfd.FileName = string.Format("{0}.csv", DateTime.Now.ToString("yyyyMMddHHmmss"));

            string targetFileName;

            if(sfd.ShowDialog() == true)
            {
                targetFileName = sfd.FileName;

                File.WriteAllText(targetFileName, content);

                labelErgebnis.Content = $"Fertig - {sw.Elapsed.ToString("mm\\:ss\\.fffffff")}";
            }
        }

        private void textBoxSpaltenname_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default.DefaultColumnName = textBoxSpaltenname.Text;
            Settings.Default.Save();
        }
    }
}
