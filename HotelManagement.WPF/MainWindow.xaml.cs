using HotelManagement.WPF.Views;
using System.Windows;

namespace HotelManagement.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Display the DashboardPage when the application starts
            MainFrame.Navigate(new DashboardPage());
        }

        /// <summary>
        /// Navigates to the DashboardPage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DashboardPage());
        }

        /// <summary>
        /// Navigates to the GuestsPage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Guests_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new GuestsPage());
        }

        /// <summary>
        /// Navigates to the RoomsPage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Rooms_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new RoomsPage());
        }

        /// <summary>
        /// Navigates to the ReservationsPage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Reservations_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ReservationsPage());
        }

        /// <summary>
        /// Navigates to the CheckInOutPage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrontDesk_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CheckInOutPage());
        }

        /// <summary>
        /// Navigates to the InvoicesPage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Invoices_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new InvoicesPage());
        }
    }
}
