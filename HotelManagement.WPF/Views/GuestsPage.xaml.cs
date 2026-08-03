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
            client.BaseAddress = new Uri("http://hotelmanagement2026.somee.com/");

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
            else
            {
                MessageBox.Show("Could not load guests.");
            }
        }

        /// <summary>
        /// Filter guests
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            string selectedFilter = (cbGuestType.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (selectedFilter == "All")
            {
                dgGuests.ItemsSource = allGuests;
            }

            else if (selectedFilter == "Guests with Active Reservation")
            {
                HttpResponseMessage response = await client.GetAsync("api/Guests/ActiveReservations");

                if (response.IsSuccessStatusCode)
                {
                    var guests = await response.Content.ReadAsAsync<List<Guest>>();

                    dgGuests.ItemsSource = guests;
                }
            }

            else if (selectedFilter == "Guests with Stay History")
            {
                HttpResponseMessage response = await client.GetAsync("api/Guests/WithStayHistory");

                if (response.IsSuccessStatusCode)
                {
                    var guests = await response.Content.ReadAsAsync<List<Guest>>();

                    dgGuests.ItemsSource = guests;
                }
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            cbGuestType.SelectedItem = cbGuestType.Items[0]; // reset to "All"
            dgGuests.ItemsSource = allGuests;

        }

        private void AddGuest_Click(object sender, RoutedEventArgs e)
        {       
            // Create a new Guestindow to add new guest info
            GuestWindow window = new GuestWindow();

            window.Show();

            window.Closed += GuestWindow_Closed; // when this window closes, run GuestWindow_Closed method to reload guests
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            // as Room: "Try to convert this to a Reservation. If it works, give me the Reservation.
            // If not, give me null."
            // Get the reservation selected by the user in the DataGrid
            Guest selectedGuest = dgGuests.SelectedItem as Guest;

            if (selectedGuest == null)
            {
                MessageBox.Show("Please select a guest to edit.");
                return;
            }
            // Create a new GuestWindow; pass the selected guest to edit
            GuestWindow window = new GuestWindow(selectedGuest);

            window.Show();

            window.Closed += GuestWindow_Closed; // when this window closes, run the GuestWindow_Closed method to reload guests
        }

        // cannot delete a guest if they have reservations; check for that first
        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            Guest selectedGuest =
                dgGuests.SelectedItem as Guest;

            if (selectedGuest == null)
            {
                MessageBox.Show("Please select a guest to delete.");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Are you sure you want to delete guest {selectedGuest.Name}?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.No)
                return;

            HttpResponseMessage response =
                await client.DeleteAsync($"api/Guests/{selectedGuest.GuestId}");

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Guest deleted successfully.");

                GuestsPage_Loaded(null, null);
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();

                MessageBox.Show(error);
            }
        }

        private void StayHistory_Click(object sender, RoutedEventArgs e)
        {
            // as Guest: "Try to convert this to a Guest. If it works, give me the Guest.
            // If not, give me null."
            // Get the reservation selected by the user in the DataGrid
            Guest selectedGuest = dgGuests.SelectedItem as Guest;

            if (selectedGuest == null)
            {
                MessageBox.Show("Please select a guest first to view their stay history.");
                return;
            }
            // Create a new StayHistoryWindow; pass the selected guest to view their stay history
            StayHistoryWindow window = new StayHistoryWindow(selectedGuest);

            window.Show();
        }

        // to refresh list of guests after adding or editing a guest
        private void GuestWindow_Closed(object sender, EventArgs e)
        {
            GuestsPage_Loaded(null, null);
        }
    }
}
