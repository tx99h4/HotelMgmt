using System;
using System.Data;
using System.Data.OleDb;
using System.Collections.Generic;

/*
 * CAUTION: All operations dealing with DB
 * are not handling exceptions
 */

public class DbManager
{
    private OleDbConnection dbConn;
    private readonly String strAccessConn;

    public static readonly string[] nationalities = {"France"  , "Espagne", "Royaume-Uni",
                                               "Allemagne" , "Italie", "Suède",
                                               "Danemark", "Finlande", "Norvège",
                                               "Autriche" , "Portugal", "Hollande",
                                               "Belgique" , "Suisse", "Hongrie",
                                               "Russie", "Pologne", "USA",
                                               "Canada"  , "Japon", "Algérie",
                                               "Tunisie", "Libye", "Mauritanie",
                                               "Arabie Saoudite"  , "Égypte", "Syrie",
                                               "E.A.U.", "Autres pays arabes", "Afrique",
                                               "R.M.E." , "Autres", "Maroc",
                                               "Résidents étrangers"
                                              };

    public DbManager(string dbFolder)
    {
        strAccessConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + dbFolder +
                        "Persist Security Info=False";
    }

    public void dbConnect()
    {
        dbConn = new OleDbConnection(strAccessConn);
        dbConn.Open();
    }

    public void dbNewGuest(List<string> record)
    {
        // pre condition: choosen room is free
        string strAddRequest = "UPDATE Room SET busy=TRUE WHERE id_room=" + record[0];
        OleDbCommand addCmd = new OleDbCommand(strAddRequest, dbConn);
        addCmd.ExecuteNonQuery();

        strAddRequest = "INSERT INTO Guest(no_room,name,givenname,nationality," +
                        "birthdate,idtype,idnumber,checkindate) VALUES(" +
                                record[0] + ",'" +
                                record[1] + "','" +
                                record[2] + "','" +
                                record[3] + "','" +
                                record[4] + "','" +
                                record[5] + "','" +
                                record[6] + "','" +
                                record[7] + "')";

        addCmd.CommandText = strAddRequest;
        addCmd.ExecuteNonQuery();

    }

    // pre condition: guest to remove exist
    public void dbRemoveGuest(List<string> record)
    {
        DataSet ds = new DataSet();
        
        string strFindRequest   = "SELECT id_guest FROM Guest WHERE no_room=" + record[0] +
                                  " AND name='"+ record[1] + "'" +
                                  " AND givenname='" + record[2] + "'" +
                                  " AND nationality='" + record[3] + "'" +
                                  " AND birthdate=CDATE('" + record[4] +"')"+
                                  " AND idtype='" + record[5] + "'" +
                                  " AND idnumber='" + record[6] + "'" +
                                  " AND checkindate=CDATE('" + record[7] + "')";
        
        string strRemoveRequest = "DELETE FROM Guest WHERE id_guest=";

        OleDbCommand removeCmd = new OleDbCommand(strFindRequest, dbConn);
        OleDbDataAdapter getAvailableAdapter = new OleDbDataAdapter(removeCmd);

        getAvailableAdapter.Fill(ds);

        // guest don't exist
        if (ds.Tables[0].Rows.Count == 0)
            return;

        removeCmd.CommandText = strRemoveRequest + ds.Tables[0].Rows[0][0];
        removeCmd.ExecuteNonQuery();

    }

    public List<int> dbFindGuest(List<string> record)
    {
        DataSet ds = new DataSet();
        List<int> foundGuests = new List<int>();

        // TODO -> add BETWEEN criterion 
        string strFindRequest = "SELECT id_guest FROM Guest,Room WHERE id_room=no_room"+
                                (record[0].Equals("") ? "" : " AND no_room=" + record[0]) +
                                (record[1].Equals("") ? "" : " AND name LIKE '" + record[1] + "%'") +
                                (record[2].Equals("") ? "" : " AND givenname LIKE '" + record[2] + "%'") +
                                (record[3].Equals("") ? "" : " AND nationality='" + record[3] + "'") +
                                (record[4].Equals("") ? "" : " AND birthdate=CDATE('" + record[4] + "')") +
                                (record[5].Equals("") ? "" : " AND idtype='" + record[5] + "'") +
                                (record[6].Equals("") ? "" : " AND idnumber='" + record[6] + "'") +
                                (record[7].Equals("") ? "" : " AND checkindate=CDATE('" + record[7] + "')") +
                                (record[8].Equals("") ? "" : " AND checkoutdate=CDATE('" + record[8] + "')") +
                                (record[9].Equals("") ? "" : " AND busy=" + record[9]);

        OleDbCommand findCmd = new OleDbCommand(strFindRequest, dbConn);
        OleDbDataAdapter findAdapter = new OleDbDataAdapter(findCmd);
        findAdapter.Fill(ds);

        if (ds.Tables[0].Rows.Count != 0)
        {
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                foundGuests.Add(Convert.ToInt16(row[0].ToString()));
            }
        }

        return foundGuests;
    }

    // pre-condition: idguest at least one element inside
    public DataSet dbGetGuest(List<int> idguest)
    {
        DataSet ds = new DataSet();
        string strSelectRequest;
        string expandedList = "";

        OleDbCommand selectCmd = new OleDbCommand("", dbConn);
        OleDbDataAdapter selectAdapter;

        for (var i = 0; i < idguest.Count-1; i++)
            expandedList += idguest[i] + ",";

        expandedList += idguest[idguest.Count - 1];

        strSelectRequest = "SELECT no_room,name,givenname,nationality,FORMAT(birthdate,\"Short Date\")"+
                           ",idtype,idnumber,FORMAT(checkindate,\"Short Date\"),"+
                           "FORMAT(checkoutdate,\"Short Date\") " +
                           "FROM Guest WHERE id_guest IN (" + expandedList + ") "+
                           "ORDER BY no_room";

        selectCmd.CommandText = strSelectRequest;
        selectAdapter = new OleDbDataAdapter(selectCmd);
        selectAdapter.Fill(ds);

        return ds;
    }

    // pre condition: roomID unique
    public void dbSetRoomStat(string room_id, bool stat)
    {
        string strRequest;
        OleDbCommand updateCmd = new OleDbCommand("", dbConn);

        strRequest = "UPDATE Room SET busy="+
                     (stat ? "TRUE" : "FALSE") + " WHERE id_room=" + room_id;
        
        updateCmd.CommandText = strRequest;
        updateCmd.ExecuteNonQuery();
    }

    // pre condition: a unique guest record exist
    public void dbUpdateGuest(int guest_id,  List<string> record, List<string> newRecord)
    {
        string strUpdateRequest;
        OleDbCommand updateCmd;
        OleDbDataAdapter updateAdapter;
        DataSet ds = new DataSet();

        // not updating twice the same information
        if(record.Equals(newRecord))
            return;

        updateCmd = new OleDbCommand("", dbConn);

        // room changed ?
        if (record[0] != newRecord[0])
        {
            // check if the room is busy during that date
            // WARNING: MUST COVER ALL OVERLAPS of checkin&checkout !!
            strUpdateRequest = "SELECT id_guest FROM Guest WHERE no_room=" + newRecord[0] +
                               "  AND (checkindate  >= #" + newRecord[7] +
                               "# AND (checkoutdate <= #" + newRecord[8] +
                               "# OR checkoutdate IS NULL))";

            updateCmd.CommandText = strUpdateRequest;
            updateAdapter = new OleDbDataAdapter(updateCmd);
            updateAdapter.Fill(ds);

            if (ds.Tables[0].Rows.Count != 0)
            {
                // room must be reserved for at least the guest to update
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    if (row.ToString().Equals(guest_id.ToString()))
                        break;
                }

                return; // stop process otherwise
            }

            // OK? then ->
            // set busy the new one
            dbSetRoomStat(newRecord[0], true);

            // free former room
            dbSetRoomStat(record[0], false);
        }

        // update guest informations
        strUpdateRequest = "UPDATE Guest SET no_room=" + newRecord[0] +
                            (record[1].Equals(newRecord[1]) ? "" : " ,name='" + newRecord[1] + "'") +
                            (record[2].Equals(newRecord[2]) ? "" : " ,givenname='" + newRecord[2] + "'") +
                            (record[3].Equals(newRecord[3]) ? "" : " ,nationality='" + newRecord[3] + "'") +
                            (record[4].Equals(newRecord[4]) ? "" : " ,birthdate=CDATE('" + newRecord[4] + "')") +
                            (record[5].Equals(newRecord[5]) ? "" : " ,idtype='" + newRecord[5] + "'") +
                            (record[6].Equals(newRecord[6]) ? "" : " ,idnumber='" + newRecord[6] + "'") +
                            (record[7].Equals(newRecord[7]) ? "" : " ,checkindate=CDATE('" + newRecord[7] + "')") +
                            (record[8].Equals(newRecord[8]) ? "" : (newRecord[8] == "" ? ", checkoutdate=NULL" : " ,checkoutdate=CDATE('" + newRecord[8] + "')") ) +
                            " WHERE id_guest=" + guest_id;

        updateCmd.CommandText = strUpdateRequest;
        updateCmd.ExecuteNonQuery();

        // set room to free if checkout date is modified
        if (!(record[8].Equals(newRecord[8]) || newRecord[8] == ""))
        {
            strUpdateRequest = "UPDATE Room SET busy=FALSE WHERE id_room=" + newRecord[0];
            updateCmd.CommandText = strUpdateRequest;
            updateCmd.ExecuteNonQuery();
        }
    }

    /* get total free rooms in hotel */
    public List<string> dbFreeRooms()
    {
        DataSet ds = new DataSet();
        List<string> freeRooms = new List<string>();

        OleDbCommand getFreeCmd = new OleDbCommand("SELECT id_room FROM Room WHERE busy=FALSE", dbConn);
        OleDbDataAdapter getFreeAdapter = new OleDbDataAdapter(getFreeCmd);

        getFreeAdapter.Fill(ds);

        // Return nothing when all rooms are full
        if (ds.Tables[0].Rows.Count == 0)
            return null;

        foreach (DataRow row in ds.Tables[0].Rows)
        {
            freeRooms.Add(row[0].ToString());
        }

        return freeRooms;

    }

    /* get total busy rooms in hotel */
    public List<string> dbBusyRooms()
    {
        DataSet ds = new DataSet();
        List<string> BusyRooms = new List<string>();

        OleDbCommand getBusyCmd = new OleDbCommand("SELECT id_room FROM Room WHERE busy=TRUE", dbConn);
        OleDbDataAdapter getBusyAdapter = new OleDbDataAdapter(getBusyCmd);

        getBusyAdapter.Fill(ds);

        // Return nothing when all rooms are full
        if (ds.Tables[0].Rows.Count == 0)
            return null;

        foreach (DataRow row in ds.Tables[0].Rows)
        {
            BusyRooms.Add(row[0].ToString());
        }

        return BusyRooms;

    }

    public const int room_already_free = -1;
    public const int checkout_ok = 0;

    /* Insert leaving date then free the room */
    public int dbUpdateCheckoutDate(int id_room, string checkoutDate)
    {
        DataSet ds = new DataSet();

        // check if room is busy
        string strCheckBusy = "SELECT busy FROM Room WHERE id_room=" + id_room;
        OleDbCommand checkBusyRequest = new OleDbCommand(strCheckBusy, dbConn);
        OleDbDataAdapter busyAdapter  = new OleDbDataAdapter(checkBusyRequest);
        busyAdapter.Fill(ds);

        if (ds.Tables[0].Rows[0][0].ToString().ToLower().Equals("false"))
            return room_already_free;

        // update guest checkout date
        string strCheckoutRequest = "UPDATE Guest SET checkoutdate='" + checkoutDate +
                                    "' WHERE no_room=" + id_room + " AND checkoutdate IS NULL";

        OleDbCommand updateCheckoutCmd = new OleDbCommand(strCheckoutRequest, dbConn);
        updateCheckoutCmd.ExecuteNonQuery();

        // set room to free
        string strFreeRequest = "UPDATE Room SET busy=FALSE WHERE id_room=" + id_room;
        OleDbCommand updateFreeCmd = new OleDbCommand(strFreeRequest, dbConn);
        updateFreeCmd.ExecuteNonQuery();

        return checkout_ok;
    }

    /* Get number of customers of certain date */
    public int dbGetTotalGuestPerDay(string date, string nationality)
    {
        DataSet ds = new DataSet();

        string strTotalGuests = "SELECT COUNT(id_guest) FROM Guest WHERE " +
                                    "checkindate = CDATE('" + date + "') " +
                                    "AND nationality='" + nationality + "'";
        //string strTotalGuests = "SELECT checkindate,COUNT(id_guest) FROM Guest WHERE " +
        //                        "nationality='" + nationality + "' AND " +
        //                        "checkindate BETWEEN #" + date.Substring(0, 8) +
        //                        "01# AND #" + date + "# " +
        //                        " GROUP BY checkindate ORDER BY checkindate";

        OleDbCommand getTotalGuestRequest  = new OleDbCommand(strTotalGuests, dbConn);
        OleDbDataAdapter totalGuestAdapter = new OleDbDataAdapter(getTotalGuestRequest);
        totalGuestAdapter.Fill(ds);

        // if no match found
        if (ds.Tables[0].Rows.Count == 0 || ds.Tables[0].Rows[0][0].ToString() == "")
            return 0;
        else
            return Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());
    }

    /* Get number of nights of certain date */
    public int dbGetTotalNightsPerDay(string date, string nationality)
    {
        DataSet ds = new DataSet();

        string strTotalNights = "SELECT SUM(DATEDIFF(\"d\",checkindate,IIF(ISNULL(checkoutdate), NOW(), checkoutdate))) FROM Guest WHERE nationality = '" +
                                    nationality + "' AND checkindate = CDATE('" + date + "')";

        OleDbCommand getTotalNightsRequest  = new OleDbCommand(strTotalNights, dbConn);
        OleDbDataAdapter totalNightsAdapter = new OleDbDataAdapter(getTotalNightsRequest);
        totalNightsAdapter.Fill(ds);

        // if no match found
        if (ds.Tables[0].Rows.Count == 0 || ds.Tables[0].Rows[0][0].ToString() == "")
            return 0;
        else
            return Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());
    }

    /* returns number of total used rooms for a certain day */
    public int dbTotalUsedRooms(string checkindate)
    {
        DataSet ds = new DataSet();

        string strTotalRoomsMonth = "SELECT COUNT(no_room) FROM Guest WHERE checkindate = CDATE('" + checkindate + "')";
        OleDbCommand getTotalRoomsRequest = new OleDbCommand(strTotalRoomsMonth, dbConn);
        OleDbDataAdapter totalRoomsAdapter = new OleDbDataAdapter(getTotalRoomsRequest);

        totalRoomsAdapter.Fill(ds);

        // if no match found
        if (ds.Tables[0].Rows.Count == 0 || ds.Tables[0].Rows[0][0].ToString() == "")
            return 0;
        else
            return Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());
    }

    /* disconnect from the DB */
    public void dbDisconnect()
    {
        dbConn.Close();
    }
}
