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
    /// Interaction logic for ReservationsPage.xaml
    /// </summary>
    public partial class ReservationsPage : Page
    {
        private HttpClient client = new HttpClient();

        private List<Reservation> allReservations = new List<Reservation>();

        private List<Guest> allGuests = new List<Guest>();

        private List<Room> allRooms = new List<Room>();

        public ReservationsPage()
        {
            InitializeComponent();

            client.BaseAddress = new Uri("http://hotelmanagement2026.somee.com/");

            Loaded += ReservationsPage_Loaded;
        }

        /// <summary>
        /// Load all reservations from the API when the page is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ReservationsPage_Loaded(object sender, RoutedEventArgs e)
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

            await LoadGuests();
            await LoadRooms();
        }

        /// <summary>
        /// Populate the guest ComboBox with all guests from the API
        /// </summary>
        /// <returns></returns>
        private async Task LoadGuests()
        {
            HttpResponseMessage response = await client.GetAsync("api/Guests");

            if (response.IsSuccessStatusCode)
            {
                allGuests =
                    await response.Content.ReadAsAsync<List<HotelManagement.WPF.Data.Entities.Guest>>();

                cbGuests.ItemsSource = allGuests;

                cbGuests.DisplayMemberPath = "Name";
                cbGuests.SelectedValuePath = "GuestId";
            }
            else
            {
                MessageBox.Show("Error loading reservations.");

            }
        }

        /// <summary>
        /// Populate the room ComboBox with all rooms from the API
        /// </summary>
        /// <returns>Task</returns>
        private async Task LoadRooms()
        {
            HttpResponseMessage response = await client.GetAsync("api/Rooms");

            if (response.IsSuccessStatusCode)
            {
                allRooms =
                    await response.Content.ReadAsAsync<List<HotelManagement.WPF.Data.Entities.Room>>();

                cbRooms.ItemsSource = allRooms;

                cbRooms.DisplayMemberPath = "RoomNumber";
                cbRooms.SelectedValuePath = "RoomId";
            }
            else
            {
                MessageBox.Show("Error loading rooms.");
            }
        }

        /// <summary>
        /// Clear all search filters and display the complete list of reservations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            cbGuests.SelectedIndex = -1;
            cbRooms.SelectedIndex = -1;
            dpCheckIn.SelectedDate = null;
            dpCheckOut.SelectedDate = null;
            dgReservations.ItemsSource = allReservations;
        }

        /// <summary>
        /// Filter the reservations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            // Validate date range
            if (dpCheckIn.SelectedDate != null && dpCheckOut.SelectedDate != null)
            {
                DateTime checkIn = dpCheckIn.SelectedDate.Value;
                DateTime checkOut = dpCheckOut.SelectedDate.Value;

                if (checkOut < checkIn)
                {
                    MessageBox.Show("Check-out date cannot be before check-in date.");
                    return;
                }
            }

            // Start with all reservations
            IEnumerable<Reservation> filtered = allReservations;

            // Filter by guest if one was selected
            if (cbGuests.SelectedValue != null)
            {
                int guestId = (int)cbGuests.SelectedValue;

                filtered = filtered.Where(r =>r.GuestId == guestId);
            }

            // Filter by room if one was selected
            if (cbRooms.SelectedValue != null)
            {
                int roomId = (int)cbRooms.SelectedValue;

                filtered = filtered.Where(r => r.RoomId == roomId);
            }

            // Filter by check-in date if selected
            if (dpCheckIn.SelectedDate != null)
            {
                DateTime checkIn = dpCheckIn.SelectedDate.Value;

                filtered = filtered.Where(r => r.CheckInDate.Date == checkIn.Date);
            }

            // Filter by check-out date if selected
            if (dpCheckOut.SelectedDate != null)
            {
                DateTime checkOut = dpCheckOut.SelectedDate.Value;

                filtered = filtered.Where(r => r.CheckOutDate.Date == checkOut.Date);
            }

            // Display filtered results
            dgReservations.ItemsSource = filtered.ToList();
        }

        /// <summary>
        /// Open a new ReservationWindow to add a new reservation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddReservation_Click(object sender, RoutedEventArgs e)
        {
            // Create a new ReservationWindow to add a reservation
            ReservationWindow window = new ReservationWindow();

            window.Show();

            // Reload the reservations when the window is closed
            window.Closed += ReservationWindow_Closed;
        }

        /// <summary>
        /// Open a new ReservationWindow to edit the selected reservation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            // as Reservation: convert this to a Reservation object, or null if it cannot be converted
            // Get the reservation selected by the user in the DataGrid
            Reservation selectedReservation = dgReservations.SelectedItem as Reservation;

            if (selectedReservation == null)
            {
                MessageBox.Show("Please select a reservation to edit.");
                return;
            }

            if (selectedReservation.ReservationStatus == "Cancelled")
            {
                MessageBox.Show("Cancelled reservations cannot be edited.");
                return;
            }
            // Create a new ReservationWindow; pass the selected reservation to edit
            ReservationWindow window = new ReservationWindow(selectedReservation);

            window.Show();

            // Reload the reservations when the window is closed
            window.Closed += ReservationWindow_Closed;
        }

        /// <summary>
        /// Cancel a reservation 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Reservation selectedReservation = dgReservations.SelectedItem as Reservation;

            if (selectedReservation == null)
            {
                MessageBox.Show("Please select a reservation to cancel.");
                return;
            }

            if (selectedReservation.ReservationStatus == "Cancelled")
            {
                MessageBox.Show("This reservation is already cancelled.");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to cancel this reservation?",
                "Confirm Cancellation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.No)
            {
                return;
            }

            HttpResponseMessage response =
                await client.PutAsync($"api/Reservations/{selectedReservation.ReservationId}/Cancel", null);


            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Reservation cancelled successfully.");

                ReservationsPage_Loaded(null, null);
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();

                MessageBox.Show(error);
            }
        }

        /// <summary>
        /// Reload the reservations when the ReservationWindow is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReservationWindow_Closed(object sender, EventArgs e)
        {
            ReservationsPage_Loaded(null, null);
        }
    }
}
