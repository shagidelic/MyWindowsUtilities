using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace FileSearchUtility
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<FoundGridItem> foundFiles = new ObservableCollection<FoundGridItem>();

        public MainWindow()
        {
            InitializeComponent();

            Initialize();
        }

        private void Initialize()
        {
            lstFoundFiles.ItemsSource = foundFiles;
            txtSearchDirectory.Focus();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            var progress = new Progress<Tuple<int, float, string>>((fileCount) =>
            {
                lblMessages.Content = "Files found: " + fileCount.Item1 + "\tFiles Scanned: " + fileCount.Item2;
                foundFiles.Add(new FoundGridItem() { FilePath = fileCount.Item3 });
            });

            foundFiles.Clear();

            string dir = txtSearchDirectory.Text;
            string fileSearch = txtFileSearchPattern.Text;
            string contentSearch = txtContentSearchPattern.Text;
            bool topOnly = (bool)chkTopOnly.IsChecked;

            progressBar.IsIndeterminate = true;
            progressBar.Value = 1;

            Task<List<string>>.Factory.StartNew(() =>
            {
                return DirectoryNFileSearch.DirectorySearch.FindFilesRegex(dir,
                    fileSearch, contentSearch, progress, searchTopOnly: topOnly);
            }
            ).ContinueWith( t => Dispatcher.Invoke(() => { progressBar.IsIndeterminate = false; }));
        }
    }

    public class FoundGridItem
    {
        public string FilePath { get; set; }
    }
}
