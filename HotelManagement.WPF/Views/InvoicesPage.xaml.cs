using HotelManagement.WPF.Data.Entities;
using HotelManagement.WPF.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HotelManagement.WPF.Views
{
    /// <summary>
    /// Interaction logic for InvoicesPage.xaml
    /// </summary>
    public partial class InvoicesPage : Page
    {
        private HttpClient client = new HttpClient();

        private List<Invoice> allInvoices = new List<Invoice>();

        public InvoicesPage()
        {
            InitializeComponent();

            client.BaseAddress = new Uri("https://localhost:44380/");

            Loaded += InvoicesPage_Loaded;
        }

        private async void InvoicesPage_Loaded(object sender, RoutedEventArgs e)
        {
            HttpResponseMessage response = await client.GetAsync("api/Invoices");

            if (response.IsSuccessStatusCode)
            {
                // Convert JSON returned by the API into a list of Reservation objects
                var invoices = await response.Content.ReadAsAsync<List<Invoice>>();

                // Save complete original list received from the API
                allInvoices = invoices;

                // Display all reservations initially in the DataGrid
                dgReservations.ItemsSource = allInvoices;
            }
            else
            {
                MessageBox.Show("Error loading invoices.");
            }
        }

        private void GenerateInvoice_Click(object sender, RoutedEventArgs e)
        {
            // Create a new InvoiceWindow to add new room info
            InvoiceWindow window = new InvoiceWindow();

            // Open the popup and wait until the user closes it
            window.Show();

            window.Closed += InvoiceWindow_Closed; // when this window closes, run RservationWindow_Closed method to reload rooms

        }

        private void RegisterPayment_Click(object sender, RoutedEventArgs e)
        {
            // as Reservation: "Try to convert this to a Reservation. If it works, give me the Reservation.
            // If not, give me null."
            // Get the reservation selected by the user in the DataGrid
            Reservation selectedReservation = dgReservations.SelectedItem as Reservation;

            if (selectedReservation == null)
            {
                MessageBox.Show("Please select a reservation to edit.");
                return;
            }
            // Create a new ReservationWindow; pass the selected reservation to edit
            ReservationWindow window = new ReservationWindow(selectedReservation);

            window.Show();

            window.Closed += InvoiceWindow_Closed; // when this window closes, run the InvoiceWindow_Closed method to reload invoices

        }


        // to refresh list of rooms after adding or editing a room
        private void InvoiceWindow_Closed(object sender, EventArgs e)
        {
            InvoicesPage_Loaded(null, null);
        }
    }
}
