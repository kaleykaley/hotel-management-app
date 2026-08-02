using HotelManagement.WPF.Data.Entities;
using System;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Windows;

namespace HotelManagement.WPF.Windows
{
    /// <summary>
    /// Interaction logic for GuestWindow.xaml
    /// </summary>
    public partial class GuestWindow : Window
    {
        private Guest guest;
        private bool isEditMode;

        private HttpClient client = new HttpClient(); // bridge to API

        // If no guest is passed when creating the window, automatically use null
        public GuestWindow(Guest selectedGuest = null)
        {
            InitializeComponent();
            client.BaseAddress = new Uri("https://localhost:44380/");
            guest = selectedGuest;
            if (guest != null)
            {
                // EDIT MODE
                guest = selectedGuest;
                isEditMode = true;
                txtTitle.Text = "Edit Guest";
                LoadGuestData();
            }
            else
            {
                // ADD MODE
                guest = new Guest();
                isEditMode = false;
                txtTitle.Text = "Add Guest";
            }
        }

        // Load the data of the guest being edited into the input fields
        private void LoadGuestData()
        {
            txtGuestName.Text = guest.Name;
            txtPhoneContact.Text = guest.PhoneContact;
            txtEmail.Text = guest.Email;
            cbDocumentType.Text = guest.DocumentType;
            txtDocumentNumber.Text = guest.DocumentNumber;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!GetGuestDataFromForm())
            {
                return;
            }

            string message;

            if (isEditMode)
            {
                message = $"Are you sure you want to save changes to Guest {guest.Name}?";
            }
            else
            {
                message = "Are you sure you want to add this guest?";
            }

            MessageBoxResult result = MessageBox.Show(message, "Confirm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.No)
            {
                return;
            }

            if (isEditMode)
            {
                HttpResponseMessage response = await client.PutAsJsonAsync($"api/Guests/{guest.GuestId}", guest);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Guest {guest.GuestId} updated successfully.");
                    this.Close();

                    // Reload guests from API
                    //ReservationsPage_Loaded(null, null);
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();

                    MessageBox.Show(error);
                }
            }
            else // is Add Mode
            {
                HttpResponseMessage response = await client.PostAsJsonAsync("api/Guests", guest);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"New guest added successfully.");
                    this.Close();
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();

                    MessageBox.Show(error);
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // helper to build guest object from the form
        private bool GetGuestDataFromForm()
        {
            if (string.IsNullOrEmpty(txtGuestName.Text))
            {
                MessageBox.Show("Please enter the guest's name.");
                return false;
            }

            guest.Name = txtGuestName.Text.Trim();
            guest.PhoneContact = txtPhoneContact.Text.Trim();
            guest.Email = txtEmail.Text.Trim();
            // if user selects nothing, return null
            guest.DocumentType = cbDocumentType.Text;
            guest.DocumentNumber = txtDocumentNumber.Text.Trim();

            return true;
        }
    }
}
