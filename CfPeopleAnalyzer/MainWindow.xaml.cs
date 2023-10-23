using System.Threading;
using System.Windows;
using CfPeopleAnalyzer.db;
using CfPeopleAnalyzer.db.models;

namespace CfPeopleAnalyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DbManagerFactory.DbManager.OpenConnectionAndSetup();
        }

        private void DownloadData(object sender, RoutedEventArgs e)
        {
            new Thread(() => new CfParserApi().ParseWebSiteAsync()).Start();
        }

        private void OpenStats(object sender, RoutedEventArgs e)
        {
            new StatsWindow().Show();
        }
    }
}