using System.Runtime.InteropServices.JavaScript;
using Microsoft.Data.Sqlite;

namespace ProjektOI;
// narazie request tokeny zapisywane są w bazie danych, tam trzymane i nnie usuwane
// one aktywne są i tak tylko pol godziny, ale latwiej bylo narazie tak zrobic
// pozniej pomysli sieco zrobic zeby ich nie trzymac, albo usuwac je moze z bazy po uzyciu
public class DatabaseManager
{
    private SqliteConnection connection;

    public DatabaseManager()
    {
        connection = new SqliteConnection("Data Source=database.db;");
        connection.Open();
    }

    public void CreateDatabase()
    {

        var createTable = connection.CreateCommand();
        createTable.CommandText =
            @"CREATE TABLE IF NOT EXISTS usersInfo (
    discordID INTEGER PRIMARY KEY,
    authToken TEXT NOT NULL,
    authTokenSecret TEXT NOT NULL
 );
               CREATE TABLE IF NOT EXISTS timetable (
    discordID INTEGER PRIMARY KEY,
    name TEXT,
    start_time TEXT NOT NULL,
    end_time TEXT NOT NULL,
    room_number TEXT,
    building_name TEXT,
    group_number TEXT
);
CREATE TABLE IF NOT EXISTS usersRequest (
    discordID INTEGER PRIMARY KEY,
    requestToken TEXT NOT NULL,
    requestTokenSecret TEXT NOT NULL
)";
        createTable.ExecuteNonQuery();
    }

    public void InsertUsersData(ulong discordID, string authToken, string authTokenSecret)
    {
        var insertData = connection.CreateCommand();
        insertData.CommandText =
            "INSERT INTO usersInfo (discordID, authToken, authTokenSecret) VALUES (@discordID, @authToken, @authTokenSecret)";
        insertData.Parameters.AddWithValue("@discordID", discordID);
        insertData.Parameters.AddWithValue("@authToken", authToken);
        insertData.Parameters.AddWithValue("@authTokenSecret", authTokenSecret);
        insertData.ExecuteNonQuery();
    }

    public void InsertTimetableData(ulong discordID, string name, string start_time, string end_time,
        string room_number, string building_name, string group_number)
    {
        var insertData = connection.CreateCommand();
        insertData.CommandText =
            "INSERT INTO timetable (discordID, name, start_time, end_time, room_number, building_name, group_number) VALUES (@discordID, @name, @start_time, @end_time, @room_number, @building_name, @group_number)";
        insertData.Parameters.AddWithValue("@discordID", discordID);
        insertData.Parameters.AddWithValue("@name", name);
        insertData.Parameters.AddWithValue("@start_time", start_time);
        insertData.Parameters.AddWithValue("@end_time", end_time);
        insertData.Parameters.AddWithValue("@room_number", room_number);
        insertData.Parameters.AddWithValue("@building_name", building_name);
        insertData.Parameters.AddWithValue("@group_number", group_number);
        insertData.ExecuteNonQuery();
    }

    public void InsertUsersRequest(ulong discordID, string requestToken, string requestTokenSecret)
    {
        var insertData = connection.CreateCommand();
        insertData.CommandText =
            "INSERT INTO usersRequest (discordID, requestToken, requestTokenSecret) VALUES (@discordID, @requestToken, @requestTokenSecret)";
        insertData.Parameters.AddWithValue("@discordID", discordID);
        insertData.Parameters.AddWithValue("@requestToken", requestToken);
        insertData.Parameters.AddWithValue("@requestTokenSecret", requestTokenSecret);
        insertData.ExecuteNonQuery();
    }

    public (string? requestToken, string? requestTokenSecret) SelectUsersRequest(ulong discordID)
    {
        var selectData = connection.CreateCommand();
        selectData.CommandText =
            "SELECT requestToken, requestTokenSecret FROM usersRequest WHERE discordID = @discordID";
        selectData.Parameters.AddWithValue("@discordID", discordID);
        var reader = selectData.ExecuteReader();
        string requestToken = null;
        string requestTokenSecret = null;
        while (reader.Read())
        {
            requestToken = reader.GetString(0);
            requestTokenSecret = reader.GetString(1);
        }
        return (requestToken, requestTokenSecret);
    }
    public (string? authToken, string? authTokenSecret) SelectUsersData(ulong discordID)
    {
        var selectData = connection.CreateCommand();
        selectData.CommandText =
            "SELECT authToken, authTokenSecret FROM usersInfo WHERE discordID = @discordID";
        selectData.Parameters.AddWithValue("@discordID", discordID);
        var reader = selectData.ExecuteReader();
        string authToken = null;
        string authTokenSecret = null;
        while (reader.Read())
        {
            authToken = reader.GetString(0);
            authTokenSecret = reader.GetString(1);
        }
        return (authToken, authTokenSecret);
    }
    
}