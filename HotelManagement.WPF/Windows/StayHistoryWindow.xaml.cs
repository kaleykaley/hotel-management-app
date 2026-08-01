using HotelManagement.WPF.Data.Entities;
using HotelManagement.WPF.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Windows;

namespace HotelManagement.WPF.Windows
{
    /// <summary>
    /// Interaction logic for StayHistoryWindow.xaml
    /// </summary>
    public partial class StayHistoryWindow : Window
    {
        private Guest guest;

        private HttpClient client = new HttpClient(); // bridge to API

        // check if selectedGuest null?
        public StayHistoryWindow(Guest selectedGuest)
        {
            InitializeComponent();
            client.BaseAddress = new Uri("https://localhost:44380/");
            guest = selectedGuest;

            LoadStayHistory();
        }

        // Load the stay history for the selected guest
        private async void LoadStayHistory()
        {
            txtGuestName.Text = guest.Name;

            HttpResponseMessage response = await client.GetAsync($"api/Guests/{guest.GuestId}/StayHistory");

            if (response.IsSuccessStatusCode)
            {
                List<StayHistory> history =
                    await response.Content.ReadAsAsync<List<StayHistory>>();

                // Fill DataGrid
                dgStayHistory.ItemsSource = history;

                // Fill summary information
                txtTotalStays.Text = history.Count.ToString();

                txtTotalNights.Text = history.Sum(h => h.Nights).ToString(); // sum all nights spent at hotel
            }
            else
            {
                MessageBox.Show("Could not load stay history.");
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
