using HotelManagement.WPF.Data.Entities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace HotelManagement.WPF.Windows
{
    /// <summary>
    /// Interaction logic for ReservationWindow.xaml
    /// </summary>
    public partial class ReservationWindow : Window
    {
        private Reservation reservation;
        private bool isEditMode;

        private HttpClient client = new HttpClient(); // bridge to API

        private List<Guest> guests = new List<Guest>();
        private List<Room> rooms = new List<Room>();


        // If no Reservation is passed when creating the window, automatically use null
        public ReservationWindow(Reservation selectedReservation = null)
        {
            InitializeComponent();

            client.BaseAddress = new Uri("https://localhost:44380/");

            reservation = selectedReservation;

            if (reservation != null)
            {
                // EDIT MODE
                reservation = selectedReservation;
                isEditMode = true;

                txtTitle.Text = "Edit Reservation";

                LoadReservationData();
            }
            else
            {
                // ADD MODE

                reservation = new Reservation();
                isEditMode = false;

                txtTitle.Text = "Add Reservation";
            }
            Loaded += ReservationWindow_Loaded;
        }


        private void LoadReservationData()
        {
            cbGuests.SelectedValue = reservation.GuestId;

            cbRooms.SelectedValue = reservation.RoomId;

            dpCheckIn.SelectedDate = reservation.CheckInDate;

            dpCheckOut.SelectedDate = reservation.CheckOutDate;

            txtNumberOfGuests.Text = reservation.NumberOfGuests.ToString();

            cbStatus.Text = reservation.ReservationStatus;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!GetReservationDataFromForm())
            {
                return;
            }

            string message;

            if (isEditMode)
            {
                message = $"Are you sure you want to save changes to Reservation {reservation.ReservationId}?";
            }
            else
            {
                message = "Are you sure you want to add this reservation?";
            }

            MessageBoxResult result = MessageBox.Show(message, "Confirm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.No)
            {
                return;
            }

            if (isEditMode)
            {
                HttpResponseMessage response = await client.PutAsJsonAsync($"api/Reservations/{reservation.ReservationId}", reservation);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Reservation {reservation.ReservationId} updated successfully.");

                    // Reload reservations from API
                    //ReservationsPage_Loaded(null, null);
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();

                    MessageBox.Show($"Update failed:\n{error}");
                }
            }
            else // is Add Mode
            {
                HttpResponseMessage response = await client.PostAsJsonAsync("api/Reservations", reservation);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Reservation {reservation.ReservationId} added successfully."); ;
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();

                    MessageBox.Show($"Adding failed:\n{error}");
                }
            }
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Helper to build reservation object from the form
        private bool GetReservationDataFromForm()
        {
            if (cbGuests.SelectedValue == null)
            {
                MessageBox.Show("Please select a guest.");
                return false;
            }

            if (cbRooms.SelectedValue == null)
            {
                MessageBox.Show("Please select a room.");
                return false;
            }


            reservation.GuestId = (int)cbGuests.SelectedValue;
            reservation.RoomId = (int)cbRooms.SelectedValue;

            if (dpCheckIn.SelectedDate == null)
            {
                MessageBox.Show("Please select a check-in date.");
                return false;
            }

            reservation.CheckInDate = dpCheckIn.SelectedDate.Value;

            if (dpCheckOut.SelectedDate == null)
            {
                MessageBox.Show("Please select a check-out date.");
                return false;
            }

            reservation.CheckOutDate = dpCheckOut.SelectedDate.Value;

            if (reservation.CheckOutDate <= reservation.CheckInDate)
            {
                MessageBox.Show("Check-out date must be after check-in date.");
                return false;
            }

            int numberOfGuests;

            if (int.TryParse(txtNumberOfGuests.Text, out numberOfGuests))
            {
                reservation.NumberOfGuests = numberOfGuests;
            }
            else
            {
                MessageBox.Show("Please enter a valid number of guests.");
                return false;
            }

            reservation.ReservationStatus = cbStatus.Text;

            return true;
        }

        private async void ReservationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadGuests();
            await LoadRooms();

            if (isEditMode)
            {
                LoadReservationData();
            }
        }

        private async Task LoadGuests()
        {
            HttpResponseMessage response = await client.GetAsync("api/Guests");

            if (response.IsSuccessStatusCode)
            {
                guests = await response.Content.ReadAsAsync<List<Guest>>();

                cbGuests.ItemsSource = guests;

                // what the user sees
                cbGuests.DisplayMemberPath = "Name";

                // what gets stored
                cbGuests.SelectedValuePath = "GuestId";
            }
            else
            {
                MessageBox.Show("Error loading guests.");
            }
        }

        private async Task LoadRooms()
        {
            HttpResponseMessage response = await client.GetAsync("api/Rooms");

            if (response.IsSuccessStatusCode)
            {
                rooms = await response.Content.ReadAsAsync<List<Room>>();

                cbRooms.ItemsSource = rooms;

                cbRooms.DisplayMemberPath = "RoomNumber";

                cbRooms.SelectedValuePath = "RoomId";
            }
            else
            {
                MessageBox.Show("Error loading rooms.");
            }
        }
    }


}
