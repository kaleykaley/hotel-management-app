using HotelManagement.WPF.Data.Entities;
using HotelManagement.WPF.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HotelManagement.WPF.Views
{
    /// <summary>
    /// Interaction logic for CheckInPage.xaml
    /// </summary>
    public partial class CheckInOutPage : Page
    {
        private HttpClient client = new HttpClient();

        private List<Reservation> allReservations = new List<Reservation>();

        public CheckInOutPage()
        {
            InitializeComponent();

            client.BaseAddress = new Uri("https://localhost:44380/");

            Loaded += CheckInOutPage_Loaded;
        }


        // Load reservation to DataGrid when the page is loaded
        private async void CheckInOutPage_Loaded(object sender, RoutedEventArgs e)
        {
            HttpResponseMessage response = await client.GetAsync("api/Reservations");

            if (response.IsSuccessStatusCode)
            {
                // Convert JSON returned by the API into a list of Reservation objects
                var reservations = await response.Content.ReadAsAsync<List<Reservation>>();

                // Save complete original list received from the API
                allReservations = reservations;

                // Display all reservations initially in the DataGrid
                dgReservations.ItemsSource = allReservations;
            }
            else
            {
                MessageBox.Show("Error loading reservations.");
            }
        }

        // update room status when a reservation is checked in or checked out
        private async void CheckIn_Click(object sender, RoutedEventArgs e)
        {
            Reservation selectedReservation = dgReservations.SelectedItem as Reservation;

            if (selectedReservation == null)
            {
                MessageBox.Show("Please select a reservation first.");
                return;
            }

            if (selectedReservation.ReservationStatus != "Reserved")
            {
                MessageBox.Show("This reservation is not in a valid state for check-in.");
                return;
            }

            string message = $"Are you sure you want to check in guest: {selectedReservation.GuestName}?";


            MessageBoxResult result = MessageBox.Show(message, "Confirm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.No)
            {
                return;
            }


            HttpResponseMessage response = await client.PutAsync($"api/Reservations/{selectedReservation.ReservationId}/CheckIn", null);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show($"{selectedReservation.GuestName} has been checked in successfully.");

                // Reload reservations from API
                //ReservationsPage_Loaded(null, null);
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();

                MessageBox.Show($"Update failed:\n{error}");
            }

            // Reload reservations from API to show updated status
            CheckInOutPage_Loaded(null, null);

        }

        private async void CheckOut_Click(object sender, RoutedEventArgs e)
        {

            Reservation selectedReservation = dgReservations.SelectedItem as Reservation;

            if (selectedReservation == null)
            {
                MessageBox.Show("Please select a reservation first.");
                return;
            }

            if (selectedReservation.ReservationStatus != "Checked_In")
            {
                MessageBox.Show("This reservation is not in a valid state for check-out.");
                return;
            }

            string message = $"Are you sure you want to check out guest: {selectedReservation.GuestName}?";


            MessageBoxResult result = MessageBox.Show(message, "Confirm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.No)
            {
                return;
            }

            HttpResponseMessage response = await client.PutAsync($"api/Reservations/{selectedReservation.ReservationId}/CheckOut", null);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show($"{selectedReservation.GuestName} has been checked out successfully.");
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();

                MessageBox.Show($"Update failed:\n{error}");
            }
            // Reload reservations from API to show updated status
            CheckInOutPage_Loaded(null, null);
        }
    }
}