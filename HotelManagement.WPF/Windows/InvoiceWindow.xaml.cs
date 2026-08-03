using HotelManagement.WPF.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public InvoiceWindow()
        {
            InitializeComponent();

            dpIssueDate.SelectedDate = DateTime.Today;
            client.BaseAddress = new Uri("http://hotelmanagement2026.somee.com/");

            LoadReservationsWithoutInvoice();
        }

        /// <summary>
        /// Populates combobox with reservations that do not have an invoice
        /// </summary>
        private async void LoadReservationsWithoutInvoice()
        {
            HttpResponseMessage response = await client.GetAsync("api/Reservations/WithoutInvoice");

            if (response.IsSuccessStatusCode)
            {
                // Convert JSON returned by the API into a list of Reservation objects
                var reservations = await response.Content.ReadAsAsync<List<Reservation>>();

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

        /// <summary>
        /// Closes the window without saving any changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Generates a new invoice for the selected reservation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Populates the invoice object with data from the form fields
        /// </summary>
        /// <returns>True if the data is valid, false otherwise</returns>
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

        /// <summary>
        /// Auto-fills the form fields when a reservation is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            // ADD EXTRA SERVICES CHARGES

            decimal extraServicesCharge = 0;

            if (selectedReservation.ExtraServices != null)
            {
                foreach (ReservationExtraService service in selectedReservation.ExtraServices)
                {
                    extraServicesCharge += service.Price * service.Quantity;
                }
            }

            txtExtraServices.Text = extraServicesCharge.ToString("C");

            txtTotalDue.Text = (roomCharge + extraServicesCharge).ToString("C");
        }
    }
}
