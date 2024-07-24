using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;

namespace Hotel_Manager
{
    /// <summary>
    /// Interaction logic for modifyFrm.xaml
    /// </summary>
    /// 

    public partial class modifyFrm : Window
    {
        private DbManager dbm;
        private GestionHotelFrm parent;
        private DataSet ds;

        public modifyFrm(GestionHotelFrm parentWindow)
        {
            InitializeComponent();
            parent = parentWindow;

            dbm = parent.dbm;

            idtypeLst.SelectedIndex = 0;
            nationalityLst.ItemsSource = DbManager.nationalities;
            nationalityLst.SelectedIndex = 32; // "morocco" by default

            // set busy rooms list
            roomLst.ItemsSource = dbm.dbBusyRooms();
            roomLst.SelectedIndex = 0;

        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void findBtn_Click(object sender, RoutedEventArgs e)
        {
            List<string> pattern = new List<string>();
            List<int> idguest = new List<int>();

            pattern.Add(roomLst.Text.Equals("indifférent") ? "" : roomLst.Text);
            pattern.Add(nameTxt.Text);
            pattern.Add(givenameTxt.Text);
            pattern.Add(allroomChk.IsChecked == true ? "" : nationalityLst.Text);
            pattern.Add(birthdateDtp.Text);
            pattern.Add(allroomChk.IsChecked == true ? "" : idtypeLst.Text);
            pattern.Add(allroomChk.IsChecked == true ? "" : idnumberTxt.Text);
            pattern.Add(checkindateDtp.Text);
            pattern.Add(checkoutdateDtp.Text);

            pattern.Add(allroomChk.IsChecked == true ? "" : "TRUE");

            // avoid empty room input
            if (roomLst.Text != "")
                idguest = dbm.dbFindGuest(pattern);

            if (idguest.Count == 0)
            {
                MessageBox.Show("Client introuvable !", "Notification", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                ds = null;
                clientdataGrd.DataContext = ds;
            }
            else
            {
                ds = dbm.dbGetGuest(idguest);

                //clientdataGrd.ItemsSource = ds.Tables["Table"].DefaultView;
                clientdataGrd.DataContext = ds.Tables["Table"].DefaultView;

                if (clientdataGrd.Columns.Count != 0)
                {
                    clientdataGrd.Columns[0].Header = "Chambre";
                    clientdataGrd.Columns[1].Header = "Nom";
                    clientdataGrd.Columns[2].Header = "Prénom";
                    clientdataGrd.Columns[3].Header = "Nationalité";
                    clientdataGrd.Columns[4].Header = "Date de naissance";

                    clientdataGrd.Columns[5].Header = "Pièce d'identité";
                    clientdataGrd.Columns[6].Header = "No. de pièce";
                    clientdataGrd.Columns[7].Header = "Date d'arrivée";
                    clientdataGrd.Columns[8].Header = "Date de départ";
                }
            }
        }

        private ContextMenu updateCtx;
        private List<string> selectedguest;

        private void clientdataGrd_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid _DataGrid = sender as DataGrid;
            DataRowView selectedRow = _DataGrid.SelectedItem as DataRowView;

            Button updateBtn = new Button() { Content = "Modifier" };
            MenuItem deleteLbl = new MenuItem() { Header = "Supprimer" };

            List<string> idtypeLst = new List<string>{
                                     "CIN",
                                     "Passport",
                                     "Titre de séjours" };

            int countCells = _DataGrid.SelectedCells.Count;

            updateCtx = new ContextMenu();
            selectedguest = new List<string>();

            if (selectedRow != null)
            {
                for (var i = 0; i < countCells; i++)
                    selectedguest.Add(selectedRow.Row[i].ToString());

                // don't set to FALSE
                // cannot find rooms which is busy now and used before
                selectedguest.Add(selectedRow.Row[7].ToString().Equals("") ? "TRUE" : "");

                updateCtx.Items.Add(new TextBox() { Text = selectedguest[0] });
                updateCtx.Items.Add(new TextBox() { Text = selectedguest[1] });
                updateCtx.Items.Add(new TextBox() { Text = selectedguest[2] });

                updateCtx.Items.Add(new ComboBox()
                {
                    ItemsSource = DbManager.nationalities,
                    SelectedIndex = Array.IndexOf(DbManager.nationalities, selectedguest[3])
                });

                updateCtx.Items.Add(new DatePicker() { Text = selectedguest[4] });
                updateCtx.Items.Add(new ComboBox() { SelectedIndex = Array.IndexOf(idtypeLst.ToArray(), selectedguest[5]), ItemsSource = idtypeLst });
                updateCtx.Items.Add(new TextBox() { Text = selectedguest[6] });
                updateCtx.Items.Add(new DatePicker() { Text = selectedguest[7] });
                updateCtx.Items.Add(new DatePicker() { Text = selectedguest[8] });

                // bind & add modify to event handler
                updateBtn.Click += new RoutedEventHandler(updateBtn_Click);
                updateCtx.Items.Add(updateBtn);

                // bind & add remove to event handler
                updateCtx.Items.Add(deleteLbl);
                deleteLbl.Click += new RoutedEventHandler(deleteLbl_Click);

            }
            else
            {
                updateBtn.Click -= new RoutedEventHandler(updateBtn_Click);
                deleteLbl.Click -= new RoutedEventHandler(deleteLbl_Click);
            }

        }

        void deleteLbl_Click(object sender, RoutedEventArgs e)
        {
            if (!(selectedguest[4].Equals("") || selectedguest[7].Equals(""))
                && selectedguest.Count > 0)
            {
                if (MessageBox.Show("Voulez-vous supprimer ce client ?",
                                   "Question",
                                   MessageBoxButton.YesNo,
                                   MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    dbm.dbRemoveGuest(selectedguest);
            }
        }

        void updateBtn_Click(object sender, RoutedEventArgs e)
        {
            clientdataGrd.SelectionChanged -=
                new SelectionChangedEventHandler(clientdataGrd_SelectionChanged);

            List<int> guest_id = dbm.dbFindGuest(selectedguest);
            List<string> updatedInfo = new List<string>();

            updatedInfo.Add(((TextBox)updateCtx.Items[0]).Text);
            updatedInfo.Add(((TextBox)updateCtx.Items[1]).Text);
            updatedInfo.Add(((TextBox)updateCtx.Items[2]).Text);
            updatedInfo.Add(((ComboBox)updateCtx.Items[3]).Text);
            updatedInfo.Add(((DatePicker)updateCtx.Items[4]).Text);
            updatedInfo.Add(((ComboBox)updateCtx.Items[5]).Text);
            updatedInfo.Add(((TextBox)updateCtx.Items[6]).Text);
            updatedInfo.Add(((DatePicker)updateCtx.Items[7]).Text);

            string checkoutdate = ((DatePicker)updateCtx.Items[8]).Text;
            updatedInfo.Add(checkoutdate);
            updatedInfo.Add(checkoutdate.Equals("") ? "TRUE" : "FALSE");

            dbm.dbUpdateGuest(guest_id[0], selectedguest, updatedInfo);

            MessageBox.Show("Modifications sauvegardées.", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);

            updateCtx.IsOpen = false;

            clientdataGrd.SelectionChanged +=
                new SelectionChangedEventHandler(clientdataGrd_SelectionChanged);
        }

        private void clientdataGrd_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DependencyObject depObj = (DependencyObject)e.OriginalSource;

            while (
                (depObj != null) &&
                !(depObj is DataGridCell) &&
                !(depObj is DataGridColumn))
            {
                depObj = VisualTreeHelper.GetParent(depObj);
            }

            if (depObj == null)
                return;

            if (depObj is DataGridCell)
            {
                while ((depObj != null) && !(depObj is DataGridRow))
                {
                    depObj = VisualTreeHelper.GetParent(depObj);
                }

                DataGridRow dgRow = depObj as DataGridRow;
                dgRow.ContextMenu = updateCtx;
            }
        }

        private void allroomChk_Checked(object sender, RoutedEventArgs e)
        {
            List<string> busyRooms = dbm.dbBusyRooms();
            List<string> freeRooms = dbm.dbFreeRooms();
            List<string> anyItem = new List<string>() { "indifférent" };

            if (busyRooms != null && freeRooms != null) // partially full
                roomLst.ItemsSource = busyRooms.Concat(freeRooms).Concat(anyItem);
            else if (busyRooms != null && freeRooms == null) //completely full
                roomLst.ItemsSource = busyRooms.Concat(anyItem);
            else // completely free
                roomLst.ItemsSource = freeRooms.Concat(anyItem);

            roomLst.SelectedIndex = roomLst.Items.Count - 1;

            roomnumberLbl.Content = "Chambre";
            nationalityLst.IsEnabled = false;
            idtypeLst.IsEnabled = false;
            idnumberTxt.IsEnabled = false;
        }

        private void allroomChk_Unchecked(object sender, RoutedEventArgs e)
        {
            List<string> emptyLst = new List<string>();
            emptyLst.Add("");

            List<string> busyRooms = dbm.dbBusyRooms();

            if (busyRooms != null)
            {
                roomLst.ItemsSource = busyRooms;
                roomLst.SelectedIndex = 0;
            }
            else
            {   // completely free
                roomLst.ItemsSource = emptyLst;
                roomLst.SelectedIndex = -1;
            }

            roomnumberLbl.Content = "Chambre occupée";
            nationalityLst.IsEnabled = true;
            idtypeLst.IsEnabled = true;
            idnumberTxt.IsEnabled = true;
        }
    }
}
