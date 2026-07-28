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
        //private Reservation reservation;
        private Invoice invoice;

        private HttpClient client = new HttpClient(); // bridge to API


        // If no Invoice is passed when creating the window, automatically use null
        public PaymentWindow(Invoice selectedInvoice = null)
        {
            InitializeComponent();

            client.BaseAddress = new Uri("https://localhost:44380/");

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
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();
                MessageBox.Show(error);
            }
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Helper to build payment object from the form
        private Payment GetPaymentFromForm()
        {
            return new Payment
            {
                InvoiceId = invoice.InvoiceId,
                AmountPaid = invoice.TotalDue,
                PaymentType = cbPaymentMethod.Text,
                // ?? - If the value on the left is not null, use it. else, use value on the right
                PaymentDate = dpPaymentDate.SelectedDate ?? DateTime.Today
            };
        }
    }
}
