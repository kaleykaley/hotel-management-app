using HotelManagement.WPF.Data.Entities;
using System;
using System.Net.Http;
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

            MessageBoxResult result = MessageBox.Show(
                $"Are you sure you want to save changes to Reservation {reservation.ReservationId}?",
                "Confirm",
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
    }
}
