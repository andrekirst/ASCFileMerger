using ASCFileMerger.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
            }

            labelNDateienausgewaehlt.Content = String.Format("{0} Datei(-en) ausgewählt", fileNames.Count());
        }

        public ASCMerger Merger
        {
            get; set;
        }

        private void buttonGenerierenUndSpeichern_Click(object sender, RoutedEventArgs e)
        {
            Merger = new ASCFileMerger.ASCMerger(fileNames, textBoxSpaltenname.Text);

            string content = Merger.GeneriereTextAusgabe();

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
            }
        }

        private void textBoxSpaltenname_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default["DefaultColumnName"] = textBoxSpaltenname.Text;
            Settings.Default.Save();
        }
    }
}
