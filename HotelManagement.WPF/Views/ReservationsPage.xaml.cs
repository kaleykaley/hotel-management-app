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

        private List<HotelManagement.WPF.Data.Entities.Reservation> allReservations = new List<HotelManagement.WPF.Data.Entities.Reservation>();

        private List<HotelManagement.WPF.Data.Entities.Guest> allGuests = new List<HotelManagement.WPF.Data.Entities.Guest>();

        private List<HotelManagement.WPF.Data.Entities.Room> allRooms = new List<HotelManagement.WPF.Data.Entities.Room>();
        public ReservationsPage()
        {
            InitializeComponent();

            client.BaseAddress = new Uri("https://localhost:44380/");

            Loaded += ReservationsPage_Loaded;
        }

        private async void ReservationsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadGuests();
            await LoadRooms();

            HttpResponseMessage response =
                await client.GetAsync("api/Reservations");


            if (response.IsSuccessStatusCode)
            {
                allReservations =
                    await response.Content.ReadAsAsync<List<HotelManagement.WPF.Data.Entities.Reservation>>();

                dgReservations.ItemsSource = allReservations;
            }
            else
            {
                MessageBox.Show("Error loading reservations.");
            }
        }

        private async Task LoadGuests()
        {
            HttpResponseMessage response =
                await client.GetAsync("api/Guests");


            if (response.IsSuccessStatusCode)
            {
                allGuests =
                    await response.Content.ReadAsAsync<List<HotelManagement.WPF.Data.Entities.Guest>>();

                cbGuests.ItemsSource = allGuests;

                cbGuests.DisplayMemberPath = "Name";
                cbGuests.SelectedValuePath = "GuestId";
            }
        }

        private async Task LoadRooms()
        {
            HttpResponseMessage response =
                await client.GetAsync("api/Rooms");


            if (response.IsSuccessStatusCode)
            {
                allRooms =
                    await response.Content.ReadAsAsync<List<HotelManagement.WPF.Data.Entities.Room>>();

                cbRooms.ItemsSource = allRooms;

                cbRooms.DisplayMemberPath = "RoomNumber";
                cbRooms.SelectedValuePath = "RoomId";
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            cbGuests.SelectedIndex = -1;

            cbRooms.SelectedIndex = -1;

            dpCheckIn.SelectedDate = null;

            dpCheckOut.SelectedDate = null;


            dgReservations.ItemsSource = allReservations;
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void AddReservation_Click(object sender, RoutedEventArgs e)
        {
            // Create a new RoomWindow to add new room info
            ReservationWindow window = new ReservationWindow();

            // Open the popup and wait until the user closes it
            window.Show();

            window.Closed += ReservationWindow_Closed; // when this window closes, run RservationWindow_Closed method to reload rooms
        }

        private async void Edit_Click(object sender, RoutedEventArgs e)
        {
            // as Room: "Try to convert this to a Room. If it works, give me the Room.
            // If not, give me null."
            // Get the room selected by the user in the DataGrid
            Reservation selectedReservation = dgReservations.SelectedItem as Reservation;

            if (selectedReservation == null)
            {
                MessageBox.Show("Please select a reservation to edit.");
                return;
            }
            // Create a new ReservationWindow; pass the selected reservation to edit
            ReservationWindow window = new ReservationWindow(selectedReservation);

            // Open the popup and wait until the user closes it
            window.Show();

            window.Closed += ReservationWindow_Closed; // when this window closes, run the ReservationWindow_Closed method to reload reservations
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            Reservation selectedReservation =
                dgReservations.SelectedItem as Reservation;


            if (selectedReservation == null)
            {
                MessageBox.Show("Please select a reservation to delete.");
                return;
            }


            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to delete this reservation?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);


            if (result == MessageBoxResult.No)
                return;



            HttpResponseMessage response =
                await client.DeleteAsync(
                $"api/Reservations/{selectedReservation.ReservationId}");



            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Reservation deleted successfully.");

                ReservationsPage_Loaded(null, null);
            }
            else
            {
                string error =
                    await response.Content.ReadAsStringAsync();

                MessageBox.Show(error);
           
            }
        }

        // to refresh list of rooms after adding or editing a room
        private void ReservationWindow_Closed(object sender, EventArgs e)
        {
            ReservationsPage_Loaded(null, null);
        }
    }
}
