using HotelManagement.WPF.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace HotelManagement.WPF.Views
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
        private HttpClient client = new HttpClient(); // bridge to API

        private DispatcherTimer timer = new DispatcherTimer();


        public DashboardPage()
        {
            InitializeComponent();

            // Base address of Web API
            // Every request will start with this URL
            client.BaseAddress = new Uri("https://localhost:44380/");

            // Load rooms automatically; runs after page finishes loading
            Loaded += DashboardPage_Loaded;

            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }


        private async void DashboardPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await LoadDashboard();

            //await LoadGuestCount();
            //await LoadAvailableRooms();
            //await LoadTodayCheckIns();
            //await LoadPendingInvoices();

        }

        private async Task LoadDashboard()
        {
            HttpResponseMessage response = await client.GetAsync("api/Dashboard");

            if (response.IsSuccessStatusCode)
            {
                Dashboard dashboard = await response.Content.ReadAsAsync<Dashboard>();

                txtTotalGuests.Text = dashboard.TotalGuests.ToString();
                txtAvailableRooms.Text = dashboard.AvailableRooms.ToString();
                txtActiveReservations.Text = dashboard.ActiveReservations.ToString();
                txtUnpaidInvoices.Text = dashboard.UnpaidInvoices.ToString();
            }
            else
            {
                MessageBox.Show("Error loading dashboard.");
            }
        }

        /*
        private async Task LoadPendingInvoices()
        {
            HttpResponseMessage response = await client.GetAsync("api/Invoices");

            if (response.IsSuccessStatusCode)
            {
                // make a new GET for just available rooms?
                List<Invoice> invoices = await response.Content.ReadAsAsync<List<Invoice>>();

                int pendingInvoices = invoices.Count(i => i.InvoiceStatus == "Unpaid");

                txtPendingInvoices.Text = pendingInvoices.ToString();
            }
            else
            {
                MessageBox.Show("Error loading invoices.");
            }
        }

        /*private async Task LoadTodayCheckIns()
        {
            HttpResponseMessage response = await client.GetAsync("api/Reservations");

            if (response.IsSuccessStatusCode)
            {
                // make a new GET for just today checked-in reservations?
                List<Reservation> reservations = await response.Content.ReadAsAsync<List<Reservation>>();

                // display reserversation that were checked in today nicely or just count? 
            }
            else
            {
                MessageBox.Show("Error loading invoices.");
            }
        }



        private async Task LoadGuestCount()
        {
            // the HTTP response (200, 404, etc) is stored in the variable response
            HttpResponseMessage response = await client.GetAsync("api/Guests");

            // successfull connection to api, continue to reques json
            if (response.IsSuccessStatusCode)
            {
                // JSON (from API) is inside response.Content
                // converts json to List of Guests
                List<Guest> guests = await response.Content.ReadAsAsync<List<Guest>>();

                txtTotalGuests.Text = guests.Count.ToString();
            }
            else
            {
                MessageBox.Show("Error loading guests.");
            }
        }

        private async Task LoadAvailableRooms()
        {
            HttpResponseMessage response = await client.GetAsync("api/Rooms");

            if (response.IsSuccessStatusCode)
            {
                // make a new GET for just available rooms?
                List<Room> rooms = await response.Content.ReadAsAsync<List<Room>>();

                int availableRooms = rooms.Count(r => r.RoomStatus == "Available");

                txtAvailableRooms.Text = availableRooms.ToString();
            }
            else
            {
                MessageBox.Show("Error loading rooms.");
            }
        }*/

        private void Timer_Tick(object sender, EventArgs e)
        {
            txtCurrentDate.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy");
            txtCurrentTime.Text = DateTime.Now.ToString("HH:mm:ss");
        }
    }
}

