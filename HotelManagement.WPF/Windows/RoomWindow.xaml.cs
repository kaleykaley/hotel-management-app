using HotelManagement.WPF.Data.Entities;
using System;
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

                LoadRoomData();
            }
            else
            {
                // ADD MODE
                room = new Room();
                isEditMode = false;

                txtTitle.Text = "Add Room";
            }
        }

        // Load the data of the room being edited into the input fields
        private void LoadRoomData()
        {
            txtRoomNumber.Text = room.RoomNumber.ToString();
            txtCapacity.Text = room.Capacity.ToString();
            cbRoomType.Text = room.RoomType;
            cbStatus.Text = room.RoomStatus;
            txtPrice.Text = room.PricePerNight.ToString();
        }

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

            MessageBoxResult result = MessageBox.Show(message,"Confirm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

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

                    // Reload rooms from API
                    //RoomsPage_Loaded(null, null);
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
                    MessageBox.Show($"Room {room.RoomNumber} added successfully."); ;
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
                room.Capacity = capacity;
            }
            else
            {
                MessageBox.Show("Please enter a valid capacity.");
                return false;
            }
            room.RoomType = cbRoomType.Text;
            room.RoomStatus = cbStatus.Text;

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
            return true;
        }
    }
}
