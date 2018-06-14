using ASCFileMerger.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ASCFileMerger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> _fileNames;

        public MainWindow()
        {
            InitializeComponent();

            Settings.Default.Reload();
            textBoxSpaltenname.Text = Settings.Default.DefaultColumnName;
        }

        private void buttonDateienAuswaehlen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "ASC Dateien (*.asc)|*.asc|Alle Dateien (*.*)|*.*"
            };
            if (ofd.ShowDialog() == true)
            {
                _fileNames = ofd.FileNames.ToList();
                buttonGenerierenUndSpeichern.IsEnabled = true;
                labelNDateienausgewaehlt.Content = String.Format("{0} Datei(-en) ausgewählt", _fileNames.Count());
                labelErgebnis.Content = String.Empty;
            }
        }

        public ASCMerger Merger
        {
            get; set;
        }

        private void buttonGenerierenUndSpeichern_Click(object sender, RoutedEventArgs e)
        {
            Merger = new ASCMerger(_fileNames, textBoxSpaltenname.Text);

            DateiSpeichern();
        }

        private void DateiSpeichern()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            string content = Merger.GeneriereTextAusgabe();
            sw.Stop();

            FileInfo fi = new FileInfo(_fileNames[0]);

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "CSV Dateien (*.csv)|*.csv|Alle Dateien (*.*)|*.*",
                InitialDirectory = fi.DirectoryName,
                FileName = string.Format("{0}.csv", DateTime.Now.ToString("yyyyMMddHHmmss"))
            };

            string targetFileName;

            if(sfd.ShowDialog() == true)
            {
                targetFileName = sfd.FileName;

                File.WriteAllText(
                    path: targetFileName,
                    contents: content,
                    encoding: Merger.GetEncoding(_fileNames[0]));

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
