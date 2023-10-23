using System.Collections.Generic;
using System.Windows;
using CfPeopleAnalyzer.db;
using CfPeopleAnalyzer.db.models;

namespace CfPeopleAnalyzer;

public partial class StatsWindow : Window
{
    public StatsWindow()
    {
        InitializeComponent();
        ParsedNumber.Text = DbManagerFactory.DbManager.GetUserDao().GetParsedCount().ToString();
        CountryNotDetected.Text = DbManagerFactory.DbManager.GetUserDao().GetUndefinedCountryCount().ToString();
        
        var ratingsAsc = DbManagerFactory.DbManager.GetUserDao().GetCountryAndRatingsAsc();
        MostPopulatedCountriesTable.ItemsSource = ratingsAsc;
        
        var ratingsDesk = DbManagerFactory.DbManager.GetUserDao().GetCountryAndRatingsDesk();
        LeastPopulatedCountriesTable.ItemsSource = ratingsDesk;
        
        var populationAsc = DbManagerFactory.DbManager.GetUserDao().GetCountryAndPopulationAsc();
        AverageRatingTopTable.ItemsSource = populationAsc;
        
        var populationDesk = DbManagerFactory.DbManager.GetUserDao().GetCountryAndPopulationDesk();
        AverageRatingAntiTopTable.ItemsSource = populationDesk;
    }
}