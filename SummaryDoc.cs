using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


class SummaryDoc
{
    private DbManager dbm;

    private int[,] checkinStat;
    private int[,] nightsStat;

    public SummaryDoc(DbManager d) {
        dbm = d;
    }

    /* Compute all checkin per month */
    public bool statMonthlyCheckin(int month, int year)
    {
        int monthDays;

        // Valid date inputs
        if (month >= 1 && month <= 12 && year > 2000)
        {
            monthDays   = DateTime.DaysInMonth(year, month);
            checkinStat = new int[DbManager.nationalities.Length, monthDays];

            dbm.dbConnect();

            for (int i = 0; i < DbManager.nationalities.Length; i++)
            {
                //Console.WriteLine(nationalities[i] + "=>");

                for (var j = 1; j <= monthDays; j++)
                {
                    checkinStat[i, j - 1] = dbm.dbGetTotalGuestPerDay(year + "-" + month + "-" + j, DbManager.nationalities[i]); ;
                    //Console.Write("{0, 3}", checkinStat[i, j - 1]);
                }

                //Console.WriteLine();
            }

            dbm.dbDisconnect();

            return true; // valid date

        } // endif: valid date inputs test
        else
            return false; // not valid date
    }

    /* Compute total nights per month */
    public bool statMonthlyNights(int month, int year)
    {
        int monthDays;

        // Valid date inputs
        if (month >= 1 && month <= 12 && year > 2000)
        {
            monthDays  = DateTime.DaysInMonth(year, month);
            nightsStat = new int[DbManager.nationalities.Length, monthDays];

            dbm.dbConnect();

            for (var i = 0; i < DbManager.nationalities.Length; i++)
            {
                //Console.WriteLine(nationalities[i] + "=>");

                for (var j = 1; j <= monthDays; j++)
                {
                    nightsStat[i, j - 1] = dbm.dbGetTotalNightsPerDay(year + "-" + month + "-" + j, DbManager.nationalities[i]); ;
                    //Console.Write("{0, 4}", nightsStat[i, j - 1]);
                }

                //Console.WriteLine();
            }

            dbm.dbDisconnect();

            return true; // valid date

        } // endif: valid date inputs test
        else
            return false; // not valid date      
    }

    public bool genCheckinSummaryFile(int month, int year)
    {
        StreamWriter checkinFile;
        int daysMonth;
        int totalNationality;
        int totalPerDay;
        int bigTotal = 0;

        // fetch statistics data
        if(!statMonthlyCheckin(month, year))
            return false;

        daysMonth = DateTime.DaysInMonth(year, month);

        checkinFile = File.CreateText("Statistiques_checkin(" + month +"-"+ year + ").csv");

        // write title & days of the needed month
        string title = "\"Pays / jours\";";
        string fdata = "";
        //checkinFile.Write("\"Pays / jours\";");

        for (var i = 1; i <= daysMonth; i++)
            //checkinFile.Write("{0};", i);
            title += i + ";";
        title += "\"Total\"\n";
        checkinFile.Write(title);

        //checkinFile.WriteLine("\"Total\"");

        for (var i = 0; i < DbManager.nationalities.Length; i++)
        {
            totalNationality = 0;
            //checkinFile.Write("\""+nationalities[i]+"\";");
            fdata = "\"" + DbManager.nationalities[i] + "\";";

            for (var j = 1; j <= daysMonth; j++)
            {
                //checkinFile.Write("{0};", checkinStat[i, j - 1] == 0 ? "" : "" + checkinStat[i, j - 1]);
                fdata += checkinStat[i, j - 1] == 0 ? ";" : checkinStat[i, j - 1] + ";";
                totalNationality += checkinStat[i, j - 1];
            }

            fdata += totalNationality == 0 ? "\n" : totalNationality+"\n";
            checkinFile.Write(fdata);
            //checkinFile.WriteLine("{0}", totalNationality == 0 ? "" : "" + totalNationality);
            
        }

        checkinFile.Write("\"Total\";");
        //fdata += "\"Total\";";
        //checkinFile.Write(fdata);

        for (var j = 1; j <= daysMonth; j++)
        {
            totalPerDay = 0;

            for (var i = 0; i < DbManager.nationalities.Length; i++)
                totalPerDay += checkinStat[i, j - 1];

            bigTotal += totalPerDay;

            checkinFile.Write("{0};", totalPerDay == 0 ? "" : "" + totalPerDay);
        }

        checkinFile.Write("{0}", bigTotal == 0 ? "" : "" + bigTotal);

        checkinFile.Close();

        return true;
    }

    public bool genNightsSummaryFile(int month, int year)
    {
        StreamWriter nightsFile;
        int daysMonth;
        int totalNationality;
        int totalPerDay;
        int bigTotal = 0;
        int usedRooms;
        int totalUsedRooms = 0;

        // fetch statistics data
        if (!statMonthlyNights(month, year))
            return false;

        daysMonth = DateTime.DaysInMonth(year, month);

        nightsFile = File.CreateText("Statistiques_nuitées(" + month + "-" + year + ").csv");

        // write days of the needed month
        nightsFile.Write("\"Pays / jours\";");
        for (var i = 1; i <= daysMonth; i++)
            nightsFile.Write("{0};", i);

        nightsFile.WriteLine("\"Total\"");

        for (var i = 0; i < DbManager.nationalities.Length; i++)
        {
            totalNationality = 0;
            nightsFile.Write("\"" + DbManager.nationalities[i] + "\";");

            for (var j = 1; j <= daysMonth; j++)
            {
                nightsFile.Write("{0};", nightsStat[i, j - 1] == 0 ? "" : "" + nightsStat[i, j - 1]);
                totalNationality += nightsStat[i, j - 1];
            }

            nightsFile.WriteLine("{0}", totalNationality == 0 ? "" : "" + totalNationality);
        }

        nightsFile.Write("\"Total\";");

        // compute totals of nights for every day in the month
        for (var j = 1; j <= daysMonth; j++)
        {
            totalPerDay = 0;

            for (var i = 0; i < DbManager.nationalities.Length; i++)
                totalPerDay += nightsStat[i, j - 1];

            bigTotal += totalPerDay;

            nightsFile.Write("{0};", totalPerDay == 0 ? "" : "" + totalPerDay);
        }

        nightsFile.WriteLine("{0}", bigTotal == 0 ? "" : "" + bigTotal);

        nightsFile.Write("\"Total de chambres occupées\";");

        // compute total busy rooms per day for a month
        for (var j = 1; j <= daysMonth; j++)
        {
            usedRooms = dbm.dbTotalUsedRooms(year + "-" + month + "-" + j);
            nightsFile.Write("{0};", usedRooms == 0 ? "" : "" + usedRooms);
            totalUsedRooms += usedRooms;
        }

        nightsFile.WriteLine("{0}", totalUsedRooms == 0 ? "" : "" + totalUsedRooms);

        nightsFile.Close();

        return true;
    }
}
