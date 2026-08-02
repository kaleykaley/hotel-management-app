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


        // If no Room is passed when creating the window, automatically use null
        public RoomWindow(Room selectedRoom = null)
        {
            InitializeComponent();

            client.BaseAddress = new Uri("https://localhost:44380/");

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

        // Load the data of the room being edited into the input fields
        private void LoadRoomData()
        {
            txtRoomNumber.Text = room.RoomNumber.ToString();
            txtCapacity.Text = room.Capacity.ToString();
            cbRoomType.Text = room.RoomType;
            txtPrice.Text = room.PricePerNight.ToString();
            txtStatus.Text = room.RoomStatus;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!GetRoomDataFromForm())
            {
                return;
            }

            // Check if the room number already exists when adding a new room
            /*if (!isEditMode)
            {
                HttpResponseMessage check = await client.GetAsync("api/Rooms");

                if (check.IsSuccessStatusCode)
                {
                    var rooms = await check.Content.ReadAsAsync<List<Room>>();

                    if (rooms.Any(r => r.RoomNumber == room.RoomNumber))
                    {
                        MessageBox.Show("A room with this number already exists.");
                        return;
                    }
                }
            }*/

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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // helper to build room object from the form
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

        private void SetMaintenance_Click(object sender, RoutedEventArgs e)
        {
            room.RoomStatus = "Maintenance";
            txtStatus.Text = "Maintenance";
        }

        private void SetAvailable_Click(object sender, RoutedEventArgs e)
        {
            room.RoomStatus = "Available";
            txtStatus.Text = "Available";
        }
    }
}
