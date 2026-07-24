using HotelManagement.WPF.Data.Entities;
using HotelManagement.WPF.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http; // microsoft's client for communicating with web services
using System.Windows;
using System.Windows.Controls;


namespace HotelManagement.WPF.Views
{
    /// <summary>
    /// Interaction logic for GuestsPage.xaml
    /// </summary>
    public partial class GuestsPage : Page
    {
        private HttpClient client = new HttpClient(); // bridge to API

        private List<Guest> allGuests = new List<Guest>();

        public GuestsPage()
        {
            InitializeComponent();

            // Base address of Web API
            // Every request will start with this URL
            client.BaseAddress = new Uri("https://localhost:44380/");

            // Load rooms automatically; runs after page finishes loading
            Loaded += GuestsPage_Loaded;
        }


        private async void GuestsPage_Loaded(object sender, RoutedEventArgs e)
        {
            HttpResponseMessage response = await client.GetAsync("api/Guests");

            if (response.IsSuccessStatusCode)
            {
                allGuests = await response.Content.ReadAsAsync<List<Guest>>();

                dgGuests.ItemsSource = allGuests;
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddGuest_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StayHistory_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
