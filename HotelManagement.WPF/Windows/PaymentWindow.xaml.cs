using HotelManagement.WPF.Data.Entities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace HotelManagement.WPF.Windows
{
    /// <summary>
    /// Interaction logic for PaymentWindow.xaml
    /// </summary>
    public partial class PaymentWindow : Window
    {
        private Invoice invoice;

        private HttpClient client = new HttpClient(); // bridge to API

        // If no Invoice is passed when creating the window, use null
        public PaymentWindow(Invoice selectedInvoice = null)
        {
            InitializeComponent();

            client.BaseAddress = new Uri("http://hotelmanagement2026.somee.com/");

            invoice = selectedInvoice;

            if (invoice == null)
            {
                MessageBox.Show("No invoice was selected.");
                Close();
                return;
            }

            LoadPaymentData();
        }

        /// <summary>
        /// Load form with data from selected invoice
        /// </summary>
        private void LoadPaymentData()
        {
            txtInvoiceId.Text = invoice.InvoiceId.ToString();

            txtGuestName.Text = invoice.GuestName;

            txtRoomNumber.Text = invoice.RoomNumber.ToString();

            txtTotalDue.Text = invoice.TotalDue.ToString("C");

            dpPaymentDate.SelectedDate = DateTime.Today;

            cbPaymentMethod.SelectedIndex = 0; // default to Cash
        }

        /// <summary>
        /// Create a Payment object and send it to the API
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RegisterPayment_Click(object sender, RoutedEventArgs e)
        {
            if (cbPaymentMethod.SelectedItem == null)
            {
                MessageBox.Show("Please select a payment method.");
                return;
            }

            Payment payment = GetPaymentFromForm();

            MessageBoxResult result = MessageBox.Show(
                $"Register a payment of {invoice.TotalDue:C} for Invoice {invoice.InvoiceId}?",
                "Confirm Payment",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
                return;

            HttpResponseMessage response =
                await client.PostAsJsonAsync("api/Payments", payment);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Payment registered and invoice marked as Paid.");
                this.Close();
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();
                MessageBox.Show(error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Builds a Payment object from the form data
        /// </summary>
        /// <returns>Payment</returns>
        private Payment GetPaymentFromForm()
        {
            return new Payment
            {
                InvoiceId = invoice.InvoiceId,
                AmountPaid = invoice.TotalDue,
                PaymentType = cbPaymentMethod.Text,
                // Use selected payment date, or today's date if none was selected
                PaymentDate = dpPaymentDate.SelectedDate ?? DateTime.Today
            };
        }
    }
}
