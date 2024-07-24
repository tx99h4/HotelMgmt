using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Hotel_Manager
{
    /// <summary>
    /// Interaction logic for addCheckoutFrm.xaml
    /// </summary>
    public partial class addCheckoutFrm : Window
    {
        private DbManager dbm;
        private GestionHotelFrm parent;

        public addCheckoutFrm(GestionHotelFrm parentWindow)
        {
            InitializeComponent();
            parent = parentWindow;

            dbm = parent.dbm;

            // set busy rooms list
            busyRoomLst.ItemsSource = dbm.dbBusyRooms();
            busyRoomLst.SelectedIndex = 0;

            checkoutdateDtp.SelectedDate = DateTime.Now;

        }

        private void cancelCheckoutBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void savecheckoutBtn_Click(object sender, RoutedEventArgs e)
        {
            // update checkout date and set free the room
            dbm.dbUpdateCheckoutDate(Convert.ToInt32(busyRoomLst.Text), checkoutdateDtp.Text);

            MessageBox.Show("Checkout sauvegardé.", "Notification");

            // refresh busy rooms list
            busyRoomLst.ItemsSource = dbm.dbBusyRooms();
            busyRoomLst.SelectedIndex = 0;

            // enable adding new client when hotel is partially full
            if (dbm.dbFreeRooms() != null)
                parent.newcustomerBtn.IsEnabled = true;

            // disable checkout when hotel is empty
            if (busyRoomLst.Items.Count == 0){
                parent.addCheckoutBtn.IsEnabled = false;
                this.Close();
            }
        }
    }
}
