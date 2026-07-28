using HotelManagement.WPF.Data.Entities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace HotelManagement.WPF.Windows
{
    /// <summary>
    /// Interaction logic for InvoiceWindow.xaml
    /// </summary>
    public partial class InvoiceWindow : Window
    {
        private HttpClient client = new HttpClient();

        private Invoice invoice;

        private List<Reservation> allReservations = new List<Reservation>(); // not needed?

        public InvoiceWindow()
        {
            InitializeComponent();

            dpIssueDate.SelectedDate = DateTime.Today;
            client.BaseAddress = new Uri("https://localhost:44380/");

            LoadReservations();
        }

        private async void LoadReservations()
        {
            HttpResponseMessage response = await client.GetAsync("api/Reservations");

            if (response.IsSuccessStatusCode)
            {
                // Convert JSON returned by the API into a list of Reservation objects
                var reservations = await response.Content.ReadAsAsync<List<Reservation>>();

                // Save complete original list received from the API
                allReservations = reservations;

                // Display all reservations initially in the combobox
                cbReservations.ItemsSource = reservations;
                cbReservations.DisplayMemberPath = "DisplayName";
                cbReservations.SelectedValuePath = "ReservationId";
            }
            else
            {
                MessageBox.Show("Error loading reservations.");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void GenerateInvoice_Click(object sender, RoutedEventArgs e)
        {
            if (!GetInvoiceDataFromForm())
            {
                return;
            }

            string message = $"Are you sure you want to generate this invoice?";


            MessageBoxResult result = MessageBox.Show(message, "Confirm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.No)
            {
                return;
            }

            HttpResponseMessage response = await client.PostAsJsonAsync("api/Invoices", invoice);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show($"New invoice generated successfully."); ;
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();

                MessageBox.Show($"Generating invoice failed:\n{error}");
            }

            this.Close();
        }


        // Helper to build invoice object from the form
        private bool GetInvoiceDataFromForm()
        {
            invoice = new Invoice();

            if (cbReservations.SelectedValue == null)
            {
                MessageBox.Show("Please select a reservation.");
                return false;
            }

            invoice.ReservationId = (int)cbReservations.SelectedValue;

            if (dpIssueDate.SelectedDate == null)
            {
                MessageBox.Show("Please select an issue date.");
                return false;
            }

            invoice.IssueDate = dpIssueDate.SelectedDate.Value;

            // New invoices start unpaid
            invoice.InvoiceStatus = "Unpaid";

            return true;
        }

        // auto-fill form fields when a reservation is selected
        private void cbReservations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Reservation selectedReservation = cbReservations.SelectedItem as Reservation;

            if (selectedReservation == null)
            {
                return;
            }

            txtGuestName.Text = selectedReservation.GuestName;

            txtRoom.Text = selectedReservation.RoomNumber.ToString();

            decimal roomCharge =
                selectedReservation.NumberOfNights *
                selectedReservation.PricePerNight;

            txtRoomCharge.Text = roomCharge.ToString("C");

            txtTotalDue.Text = roomCharge.ToString("C");
            // ADD EXTRA SERVICES CHARGES


        }
    }
}
