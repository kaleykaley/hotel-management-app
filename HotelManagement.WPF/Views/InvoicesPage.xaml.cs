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

            client.BaseAddress = new Uri("http://hotelmanagement2026.somee.com/");

            Loaded += InvoicesPage_Loaded;
        }

        /// <summary>
        /// Loads the list of invoices from the API when the page is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void InvoicesPage_Loaded(object sender, RoutedEventArgs e)
        {
            HttpResponseMessage response = await client.GetAsync("api/Invoices");

            if (response.IsSuccessStatusCode)
            {
                // Convert JSON returned by the API into a list of Reservation objects
                var invoices = await response.Content.ReadAsAsync<List<Invoice>>();

                // Save complete original list received from the API
                allInvoices = invoices;

                // Display all invoices in the DataGrid
                dgInvoices.ItemsSource = allInvoices;
            }
            else
            {
                MessageBox.Show("Error loading invoices.");
            }
        }

        /// <summary>
        /// Opens a new InvoiceWindow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateInvoice_Click(object sender, RoutedEventArgs e)
        {
            // Create a new InvoiceWindow to add new room info
            InvoiceWindow window = new InvoiceWindow();

            // Open the popup and wait until the user closes it
            window.Show();

            // reload invoices when the window closes
            window.Closed += RefreshInvoices; 
        }

        /// <summary>
        /// Opens a new PaymentWindow with the selected invoice
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegisterPayment_Click(object sender, RoutedEventArgs e)
        {
            // as Invoice: "Try to convert this to an Invoice. If it works, give me the Invoice.
            // If not, give me null."
            // Get the invoice selected by the user in the DataGrid
            Invoice selectedInvoice = dgInvoices.SelectedItem as Invoice;

            if (selectedInvoice == null)
            {
                MessageBox.Show("Please select an invoice in order to register a payment.");
                return;
            }

            if (selectedInvoice.InvoiceStatus == "Paid")
            {
                MessageBox.Show("This invoice is already paid.");
                return;
            }

            // Create a new PaymentWindow; pass the selected Invoice to edit
            PaymentWindow window = new PaymentWindow(selectedInvoice);

            window.Show();

            // reload invoices when the window closes
            window.Closed += RefreshInvoices; 
        }

        /// <summary>
        /// Reloads invoices after a related window is closed
        /// </summary>
        private void RefreshInvoices(object sender, EventArgs e)
        {
            InvoicesPage_Loaded(null, null);
        }
    }
}
