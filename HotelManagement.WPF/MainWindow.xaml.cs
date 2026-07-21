using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HotelManagement.WPF.Views;

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
            // Navigate to the DashboardPage when the application starts
            MainFrame.Navigate(new DashboardPage());
        }
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DashboardPage());
        }

        private void Guests_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new GuestsPage());
        }

        private void Rooms_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new RoomsPage());
        }

        private void Reservations_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ReservationsPage());
        }



        private void CheckIn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CheckInPage());
        }

        private void CheckOut_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CheckOutPage());
        }

        private void Invoices_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new InvoicesPage());
        }
    }
}
