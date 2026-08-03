using HotelManagement.WPF.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private List<ReservationExtraService> extraServices = new List<ReservationExtraService>();


        // If no Reservation is passed when creating the window, use null
        public ReservationWindow(Reservation selectedReservation = null)
        {
            InitializeComponent();

            client.BaseAddress = new Uri("http://hotelmanagement2026.somee.com/");

            reservation = selectedReservation;

            if (reservation != null)
            {
                // EDIT MODE
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

        /// <summary>
        /// Load the form with the data of the reservation being edited
        /// </summary>
        private void LoadReservationData()
        {
            cbGuests.SelectedValue = reservation.GuestId;

            cbRooms.SelectedValue = reservation.RoomId;

            dpCheckIn.SelectedDate = reservation.CheckInDate;

            dpCheckOut.SelectedDate = reservation.CheckOutDate;

            txtNumberOfGuests.Text = reservation.NumberOfGuests.ToString();
        }

        /// <summary>
        /// Add a new reservation or update an existing one
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    this.Close();
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
                    MessageBox.Show($"New reservation added successfully.");
                    this.Close();
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();

                    MessageBox.Show($"Adding failed:\n{error}");
                }
            }
        }

        /// <summary>
        /// Close the window without saving changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Build a reservation object from the form data
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
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

            if (reservation.CheckInDate.Date < DateTime.Today)
            {
                MessageBox.Show("Check-in date cannot be in the past.");
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

            reservation.ExtraServices = extraServices.Where(s => s.IsSelected).ToList();

            foreach (var service in reservation.ExtraServices)
            {
                if (service.Quantity <= 0)
                {
                    MessageBox.Show("Extra service quantity must be greater than zero.");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Load guests, rooms, and extra services when the window is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ReservationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadGuests();
            await LoadRooms();
            await LoadExtraServices();

            if (isEditMode)
            {
                LoadReservationData();
                LoadSelectedExtraServices();
            }
        }

        /// <summary>
        /// Populate the guest ComboBox with data from the API
        /// </summary>
        /// <returns></returns>
        private async Task LoadGuests()
        {
            HttpResponseMessage response = await client.GetAsync("api/Guests");

            if (response.IsSuccessStatusCode)
            {
                guests = await response.Content.ReadAsAsync<List<Guest>>();

                cbGuests.ItemsSource = guests;

                cbGuests.DisplayMemberPath = "Name";  // what the user sees

                cbGuests.SelectedValuePath = "GuestId";  // what gets stored
            }
            else
            {
                MessageBox.Show("Error loading guests.");
            }
        }

        /// <summary>
        /// Populate the room ComboBox with data from the API
        /// </summary>
        /// <returns></returns>
        private async Task LoadRooms()
        {
            HttpResponseMessage response = await client.GetAsync("api/Rooms");

            if (response.IsSuccessStatusCode)
            {
                rooms = await response.Content.ReadAsAsync<List<Room>>();

                if (!rooms.Any())
                {
                    MessageBox.Show("No rooms available.");
                }

                // Filter unavailable rooms when creating a reservation
                if (!isEditMode)
                {
                    rooms = rooms
                        .Where(r => r.RoomStatus != "Maintenance" &&
                                    r.RoomStatus != "Occupied")
                        .ToList();
                }

                cbRooms.ItemsSource = rooms;

                cbRooms.DisplayMemberPath = "RoomNumber";
                cbRooms.SelectedValuePath = "RoomId";
            }
            else
            {
                MessageBox.Show("Error loading rooms.");
            }
        }

        /// <summary>
        /// Populate the extra services ItemsControl with data from the API
        /// </summary>
        /// <returns></returns>
        private async Task LoadExtraServices()
        {
            HttpResponseMessage response = await client.GetAsync("api/ExtraServices");

            if (response.IsSuccessStatusCode)
            {
                List<ExtraService> services =
                    await response.Content.ReadAsAsync<List<ExtraService>>();

                extraServices = services.Select(s => new ReservationExtraService
                {
                    ExtraServiceId = s.ExtraServiceId,
                    ServiceName = s.Name,
                    Price = s.Price,
                    Quantity = 1
                })
                .ToList();

                icExtraServices.ItemsSource = extraServices;
            }
            else
            {
                MessageBox.Show("Error loading extra services.");
            }
        }

        /// <summary>
        /// Load the selected extra services for the reservation being edited
        /// </summary>
        private void LoadSelectedExtraServices()
        {
            if (reservation.ExtraServices == null)
            {
                return;
            }

            foreach (var service in extraServices)
            {
                var existing = reservation.ExtraServices
                    .FirstOrDefault(s => s.ExtraServiceId == service.ExtraServiceId);

                if (existing != null)
                {
                    service.IsSelected = true;
                    service.Quantity = existing.Quantity;
                }
            }

            icExtraServices.Items.Refresh();
        }
    }
}
