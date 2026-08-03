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
        private HttpClient client = new HttpClient(); // bridge to API

        // Stores all rooms received from API
        private List<Room> allRooms = new List<Room>();
        public RoomsPage()
        {
            InitializeComponent();

            // Base address of Web API
            client.BaseAddress = new Uri("http://hotelmanagement2026.somee.com/");

            // Load rooms automatically after page finishes loading
            Loaded += RoomsPage_Loaded;
        }

        /// <summary>
        /// Loads the rooms from the API when the page is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RoomsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadRooms();
        }

        /// <summary>
        /// Filters and displays the rooms
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidatePriceFilters(out decimal minPrice, out decimal maxPrice))
                return;

            // Start with all rooms
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

        /// <summary>
        /// Resets the filters and displays all rooms
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Opens a new RoomWindow to add a new room
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddRoom_Click(object sender, RoutedEventArgs e)
        {
            RoomWindow window = new RoomWindow();
            window.Show();

            // Reload list of rooms when window closes
            window.Closed += RoomWindow_Closed;
        }

        /// <summary>
        /// Opens a new RoomWindow to edit the selected room
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            // Get the room selected by the user in the DataGrid
            Room selectedRoom = dgRooms.SelectedItem as Room;

            if (selectedRoom == null)
            {
                MessageBox.Show("Please select a room to edit.");
                return;
            }

            RoomWindow window = new RoomWindow(selectedRoom);
            window.Show();

            // Reload list of rooms when window closes
            window.Closed += RoomWindow_Closed;
        }

        /// <summary>
        /// Reloads the list of rooms when the RoomWindow is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RoomWindow_Closed(object sender, EventArgs e)
        {
            await LoadRooms();
        }

        /// <summary>
        /// Deletes the selected room
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
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

            HttpResponseMessage response = await client.DeleteAsync($"api/Rooms/{selectedRoom.RoomId}");

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show($"Room {selectedRoom.RoomNumber} deleted successfully.");

                // Reload rooms from API
                await LoadRooms();
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

        /// <summary>
        /// Validates the price filters
        /// </summary>
        /// <param name="minPrice"></param>
        /// <param name="maxPrice"></param>
        /// /// <returns>True if the filters are valid, otherwise false</returns>
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
