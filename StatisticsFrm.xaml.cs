using System;
using System.Windows;

namespace Hotel_Manager
{
    /// <summary>
    /// Interaction logic for StatisticsFrm.xaml
    /// </summary>
    public partial class StatisticsFrm : Window
    {
        private SummaryDoc sm;

        public StatisticsFrm(DbManager dbMan)
        {
            int startYear = 2010;

            int yLength = (DateTime.Now.Year - startYear) + 1;
            int[] years = new int[yLength];

            for (int i = 0; i < yLength; i++)
                years[i] = startYear + i;

            InitializeComponent();

            yearLst.ItemsSource = years;

            // set default value to this year & month
            monthsLst.SelectedIndex = DateTime.Now.Month  - 1;
            yearLst.SelectedIndex   = yearLst.Items.Count - 1;

            // set generating the two types of stats as
            // the default operation
            optionStatLst.SelectedIndex = 2;

            sm = new SummaryDoc(dbMan);

        }

        private void generateFilesBtn_Click(object sender, RoutedEventArgs e)
        {
            switch(optionStatLst.SelectedIndex){

                case 0:
                    sm.genCheckinSummaryFile(Convert.ToInt16(monthsLst.Text), Convert.ToInt16(yearLst.Text));
                break;

                case 1:
                    sm.genNightsSummaryFile(Convert.ToInt16(monthsLst.Text), Convert.ToInt16(yearLst.Text));
                break;

                case 2:
                    sm.genNightsSummaryFile(Convert.ToInt16(monthsLst.Text), Convert.ToInt16(yearLst.Text));
                    sm.genCheckinSummaryFile(Convert.ToInt16(monthsLst.Text), Convert.ToInt16(yearLst.Text));                   
                break;
            }

            MessageBox.Show("Statistiques du " + monthsLst.Text + "-" + yearLst.Text + " générés.",
                            "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void cancelCheckoutBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
