using System;
using System.Windows;
using System.Windows.Shapes;
using System.IO;

namespace Hotel_Manager
{
    /// <summary>
    /// Interaction logic for GestionHotelFrm.xaml
    /// </summary>
    public partial class GestionHotelFrm : Window
    {
        private string dbFolder = Directory.GetCurrentDirectory() + @"\hotelDB.mdb;";

        public readonly DbManager dbm;
        
        public GestionHotelFrm()
        {
            InitializeComponent();

            try
            {
                dbm = new DbManager(dbFolder);
                dbm.dbConnect();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Base de données introuvable!", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Environment.Exit(-1);
            }

            // disable adding new guests when the hotel is completely full
            if (dbm.dbFreeRooms() == null){
                newcustomerBtn.IsEnabled = false;
                MessageBox.Show("Hotel complet.", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // disable checkout input when the hotel is completely empty
            if (dbm.dbBusyRooms() == null)
                addCheckoutBtn.IsEnabled = false;
        }

        // Add new clients informations
        private void newcustomerBtn_Click(object sender, RoutedEventArgs e)
        {
            newClientFrm addClientfrm = new newClientFrm(this);
            addClientfrm.ShowDialog();
        }

        // Add checkout
        private void addCheckoutBtn_Click(object sender, RoutedEventArgs e)
        {
            addCheckoutFrm addCheckoutfrm = new addCheckoutFrm(this);
            addCheckoutfrm.ShowDialog();
        }

        // show statistics of stay per day
        private void nightStatisticBtn_Click(object sender, RoutedEventArgs e)
        {
            StatisticsFrm nightStatisticsFrm = new StatisticsFrm(dbm);
            nightStatisticsFrm.ShowDialog();
        }

        // show statistics of leaving per day
        private void statisticCheckoutBtn_Click(object sender, RoutedEventArgs e)
        {
            modifyFrm modifyFrm = new modifyFrm(this);
            modifyFrm.ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            dbm.dbDisconnect();
        }

    }
}
