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

                lbActivities.ItemsSource = dashboard.Activities;
            }
            else
            {
                MessageBox.Show("Error loading dashboard.");
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            txtCurrentDate.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy");
            txtCurrentTime.Text = DateTime.Now.ToString("HH:mm:ss");
        }
    }
}

