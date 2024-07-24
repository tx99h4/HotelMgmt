using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Linq;

namespace Hotel_Manager
{
    /// <summary>
    /// Interaction logic for newClientFrm.xaml
    /// </summary>
    public partial class newClientFrm : Window
    {
        private GestionHotelFrm parent;
        private DbManager dbm;

        private bool onlyFreeRooms = true;

        public newClientFrm(GestionHotelFrm parentWindow)
        {
            InitializeComponent();
            parent = parentWindow;

            dbm = parent.dbm;

            checkindateDtp.SelectedDate = DateTime.Now;

            idtypeLst.SelectedIndex = 0;
            nationalityLst.ItemsSource = DbManager.nationalities;
            nationalityLst.SelectedIndex = 32; // morocco by default

            // set free rooms list
            roomLst.ItemsSource = dbm.dbFreeRooms();
            roomLst.SelectedIndex = 0;

            freeRoomChk.IsChecked = true;

        }

        // close the add client form
        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void saveclientBtn_Click(object sender, RoutedEventArgs e)
        {
            List<string> newGuest = new List<string>();
            List<string> freeRooms;
            List<string> busyRooms;

            // avoid empty data input
            if (!(nameTxt.Text.Equals("") || givenameTxt.Text.Equals("") ||
                birthdateDtp.Text.Equals("") || idnumberTxt.Text.Equals("") ||
                checkindateDtp.Text.Equals("")))
            {
                // insert client to database
                newGuest.Add(roomLst.Text);
                newGuest.Add(nameTxt.Text);
                newGuest.Add(givenameTxt.Text);
                newGuest.Add(nationalityLst.Text);
                newGuest.Add(birthdateDtp.Text);
                newGuest.Add(idtypeLst.Text);
                newGuest.Add(idnumberTxt.Text);
                newGuest.Add(checkindateDtp.Text);

                dbm.dbNewGuest(newGuest);

                freeRooms = dbm.dbFreeRooms();
                busyRooms = dbm.dbBusyRooms();

                // refresh free rooms list
                if (onlyFreeRooms && freeRooms != null) //not full
                    roomLst.ItemsSource = freeRooms;
                else
                {
                    if (freeRooms == null) // full
                        roomLst.ItemsSource = busyRooms;
                    else if (busyRooms == null) // empty
                        roomLst.ItemsSource = freeRooms;
                    else // partially full
                        roomLst.ItemsSource = busyRooms.Concat(freeRooms);
                }

                roomLst.SelectedIndex = 0;

                MessageBox.Show("Client sauvegardé.", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);

                // clear fields for new entry
                nameTxt.Text = "";
                givenameTxt.Text = "";
                birthdateDtp.Text = "";
                idnumberTxt.Text = "";

                // enable checking out
                parent.addCheckoutBtn.IsEnabled = true;

                // disable input new clients when hotel is full
                if (roomLst.Items.Count == 0)
                {
                    parent.newcustomerBtn.IsEnabled = false;
                    this.Close();
                }
            }
        }

        private void freeRoomChk_Checked(object sender, RoutedEventArgs e)
        {
            List<string> freeRooms = dbm.dbFreeRooms();

            onlyFreeRooms = true;
            roomnumberLbl.Content = "Chambre libre";

            if (freeRooms == null) // full
            {
                onlyFreeRooms = false;
                roomnumberLbl.Content = "Chambre";

                MessageBox.Show("Les chambres sont toutes occupées!", "Notification",
                                 MessageBoxButton.OK, MessageBoxImage.Information);

                roomLst.ItemsSource = dbm.dbBusyRooms();
            }
            else
                roomLst.ItemsSource = dbm.dbFreeRooms();

            roomLst.SelectedIndex = 0;

        }

        private void freeRoomChk_Unchecked(object sender, RoutedEventArgs e)
        {
            List<string> freeRooms = dbm.dbFreeRooms();
            List<string> busyRooms = dbm.dbBusyRooms();

            roomnumberLbl.Content = "Chambre";
            onlyFreeRooms = false;

            if (busyRooms != null && freeRooms != null) // partially full
                roomLst.ItemsSource = busyRooms.Concat(freeRooms);
            else if (busyRooms != null && freeRooms == null)
            { //completely full
                MessageBox.Show("Les chambres sont toutes occupées!", "Notification",
                                 MessageBoxButton.OK, MessageBoxImage.Information);
                roomLst.ItemsSource = busyRooms;
            }
            else // completely free
                roomLst.ItemsSource = freeRooms;

            roomLst.SelectedIndex = 0;
        }
    }
}
