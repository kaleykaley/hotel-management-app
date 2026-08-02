using HotelManagement.WPF.Data.Entities;
using HotelManagement.WPF.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http; // microsoft's client for communicating with web services
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HotelManagement.WPF.Views
{
    /// <summary>
    /// Interaction logic for RoomsPage.xaml
    /// </summary>
    public partial class RoomsPage : Page
    {
        // HttpClient: to send requests to our Web API
        // create one object and reuse it while this page is open.
        private HttpClient client = new HttpClient(); // bridge to API

        // Stores all rooms received from API.
        // Filter this list instead of requesting the API every time
        private List<Room> allRooms = new List<Room>();
        public RoomsPage()
        {
            InitializeComponent();

            // Base address of Web API
            // Every request will start with this URL
            client.BaseAddress = new Uri("https://localhost:44380/");

            // Load rooms automatically; runs after page finishes loading
            Loaded += RoomsPage_Loaded;
        }

        private async void RoomsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadRooms();
        }

        // implement filters through api?
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            /// FIX SEARCH 
            if (!ValidatePriceFilters(out decimal minPrice, out decimal maxPrice))
                return;

            // Start with all reservations
            IEnumerable<Room> filtered = allRooms;

            // Filter by room type if one was selected
            string roomType = (cbRoomType.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (roomType != "All")
            {
                filtered = filtered.Where(r => r.RoomType == roomType);
            }

            // Filter by room status if one was selected
            string statusType = (cbStatus.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (statusType != "All")
            {
                filtered = filtered.Where(r => r.RoomStatus == statusType);
            }

            // Filter by min and/or max price if selected
            filtered = filtered.Where(r => r.PricePerNight >= minPrice && r.PricePerNight <= maxPrice);

            // Display filtered results
            dgRooms.ItemsSource = filtered.ToList();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            // Reset ComboBoxes
            cbRoomType.SelectedIndex = 0;
            cbStatus.SelectedIndex = 0;

            // Reset price fields
            txtMinPrice.Text = "0";
            txtMaxPrice.Text = "9999";

            // Show all rooms again
            dgRooms.ItemsSource = allRooms;
        }

        private async void AddRoom_Click(object sender, RoutedEventArgs e)
        {
            // Create a new RoomWindow to add new room info
            RoomWindow window = new RoomWindow();

            // Open the popup and wait until the user closes it
            window.Show();

            window.Closed += RoomWindow_Closed; // when this window closes, run the RoomWindow_Closed method to reload rooms
        }

        private async void Edit_Click(object sender, RoutedEventArgs e)
        {
            // as Room: "Try to convert this to a Room. If it works, give me the Room.
            // If not, give me null."
            // Get the room selected by the user in the DataGrid
            Room selectedRoom = dgRooms.SelectedItem as Room;

            if (selectedRoom == null)
            {
                MessageBox.Show("Please select a room to edit.");
                return;
            }

            // Create a new RoomWindow; pass the selected room to edit
            RoomWindow window = new RoomWindow(selectedRoom);

            // Open the popup and wait until the user closes it
            window.Show();

            window.Closed += RoomWindow_Closed; // when this window closes, run the RoomWindow_Closed method to reload rooms
        }

        // to refresh list of rooms after adding or editing a room
        private async void RoomWindow_Closed(object sender, EventArgs e)
        {
            await LoadRooms();

            //RoomsPage_Loaded(null, null);
        }

        // async because it must wait for the server (API) to respond
        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            // as Room: "Try to convert this to a Room. If it works, give me the Room.
            // If not, give me null."
            // Get the room selected
            Room selectedRoom = dgRooms.SelectedItem as Room;

            if (selectedRoom == null)
            {
                MessageBox.Show("Please select a room to delete.");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Are you sure you want to delete Room {selectedRoom.RoomNumber}?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.No)
            {
                return;
            }
            //DELETE api/Rooms/{id}
            HttpResponseMessage response = await client.DeleteAsync($"api/Rooms/{selectedRoom.RoomId}");

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show($"Room {selectedRoom.RoomNumber} deleted successfully.");

                // Reload rooms from API
                await LoadRooms();
                //RoomsPage_Loaded(null, null);
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();

                MessageBox.Show(error);
            }
        }

        /// <summary>
        /// Reloads the list of rooms from the API and updates the DataGrid
        /// </summary>
        /// <returns></returns>
        private async Task LoadRooms()
        {
            HttpResponseMessage response = await client.GetAsync("api/Rooms");

            if (response.IsSuccessStatusCode)
            {
                allRooms = await response.Content.ReadAsAsync<List<Room>>();
                dgRooms.ItemsSource = allRooms;
            }
            else
            {
                MessageBox.Show("Error loading rooms.");
            }
        }



        private bool ValidatePriceFilters(out decimal minPrice, out decimal maxPrice)
        {
            minPrice = 0;
            maxPrice = 9999;

            if (!decimal.TryParse(txtMinPrice.Text, out minPrice) || minPrice < 0)
            {
                MessageBox.Show("Minimum price must be a number greater than or equal to 0.");
                return false;
            }

            if (!decimal.TryParse(txtMaxPrice.Text, out maxPrice) || maxPrice < 0)
            {
                MessageBox.Show("Maximum price must be a number greater than or equal to 0.");
                return false;
            }

            if (minPrice > maxPrice)
            {
                MessageBox.Show("Minimum price cannot be greater than maximum price.");
                return false;
            }

            return true;
        }
    }
}
