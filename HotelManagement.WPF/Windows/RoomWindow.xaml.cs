using HotelManagement.WPF.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Windows;

namespace HotelManagement.WPF.Windows
{
    /// <summary>
    /// Interaction logic for RoomWindow.xaml
    /// </summary>
    public partial class RoomWindow : Window
    {
        private Room room;
        private bool isEditMode;

        private HttpClient client = new HttpClient(); // bridge to API


        // If no Room is provided, the window opens in Add mode
        public RoomWindow(Room selectedRoom = null)
        {
            InitializeComponent();

            client.BaseAddress = new Uri("http://hotelmanagement2026.somee.com/");

            room = selectedRoom;

            if (room != null)
            {
                // EDIT MODE
                room = selectedRoom;
                isEditMode = true;

                txtTitle.Text = "Edit Room";

                txtRoomNumber.IsEnabled = false;

                btnMaintenance.Visibility = Visibility.Visible;
                btnAvailable.Visibility = Visibility.Visible;

                LoadRoomData();
            }
            else
            {
                // ADD MODE
                room = new Room();
                isEditMode = false;

                txtTitle.Text = "Add Room";
                txtStatus.Text = "Available";

                btnMaintenance.Visibility = Visibility.Collapsed;
                btnAvailable.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// /// Loads the selected room data into the input fields
        /// </summary>
        private void LoadRoomData()
        {
            txtRoomNumber.Text = room.RoomNumber.ToString();
            txtCapacity.Text = room.Capacity.ToString();
            cbRoomType.Text = room.RoomType;
            txtPrice.Text = room.PricePerNight.ToString();
            txtStatus.Text = room.RoomStatus;
        }

        /// <summary>
        /// Adds a new room or updates an existing room
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!GetRoomDataFromForm())
            {
                return;
            }

            string message;

            if (isEditMode)
            {
                message = $"Are you sure you want to save changes to Room {room.RoomNumber}?";
            }
            else
            {
                message = "Are you sure you want to add this room?";
            }

            MessageBoxResult result = MessageBox.Show(message,"Confirm", MessageBoxButton.YesNo,MessageBoxImage.Warning);

            if (result == MessageBoxResult.No)
            {
                return;
            }

            if (isEditMode)
            {
                HttpResponseMessage response = await client.PutAsJsonAsync($"api/Rooms/{room.RoomId}", room);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Room {room.RoomNumber} updated successfully.");
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
                HttpResponseMessage response = await client.PostAsJsonAsync("api/Rooms", room);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Room {room.RoomNumber} added successfully.");
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
        /// Closes the window without saving changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Builds a Room object from the form inputs
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        private bool GetRoomDataFromForm()
        {

            int roomNumber;

            if (int.TryParse(txtRoomNumber.Text, out roomNumber))
            {
                room.RoomNumber = roomNumber;
            }
            else
            {
                MessageBox.Show("Please enter a valid room number.");
                return false;
            }
            int capacity;

            if (int.TryParse(txtCapacity.Text, out capacity))
            {
                if (capacity > 10)
                {
                    MessageBox.Show("Room capacity cannot exceed 10 guests.");
                    return false;
                }
                room.Capacity = capacity;
            }
            else
            {
                MessageBox.Show("Please enter a valid capacity.");
                return false;
            }
            room.RoomType = cbRoomType.Text;

            decimal price;

            if (decimal.TryParse(txtPrice.Text, out price))
            {
                room.PricePerNight = price;
            }
            else
            {
                MessageBox.Show("Please enter a valid price.");
                return false;
            }

            // New rooms always start available
            if (!isEditMode)
            {
                room.RoomStatus = "Available";
            }
            else
            {
                room.RoomStatus = txtStatus.Text;
            }

            return true;
        }

        /// <summary>
        /// Sets the room status to "Maintenance"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetMaintenance_Click(object sender, RoutedEventArgs e)
        {
            room.RoomStatus = "Maintenance";
            txtStatus.Text = "Maintenance";
        }

        /// <summary>
        /// Sets the room status to "Available"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetAvailable_Click(object sender, RoutedEventArgs e)
        {
            room.RoomStatus = "Available";
            txtStatus.Text = "Available";
        }
    }
}
