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
)  ;
        CREATE TABLE IF NOT EXISTS userTasks (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            discordID INTEGER NOT NULL,
            channelID INTEGER NOT NULL,
            name TEXT NOT NULL,
            deadlineUtc TEXT NOT NULL,
            reminderUtc TEXT NOT NULL,
            reminderSent INTEGER NOT NULL DEFAULT 0
            );
CREATE TABLE IF NOT EXISTS grades (
    discordID INTEGER NOT NULL,
    hash TEXT NOT NULL,
    PRIMARY KEY (discordID, hash)
    
)";
        
        createTable.ExecuteNonQuery();
    }

    public void InsertUsersData(ulong discordID, string authToken, string authTokenSecret)
    {
        lock (connection)
        {
            var insertData = connection.CreateCommand();
            insertData.CommandText =
                "INSERT OR REPLACE INTO usersInfo (discordID, authToken, authTokenSecret) VALUES (@discordID, @authToken, @authTokenSecret)";
            insertData.Parameters.AddWithValue("@discordID", discordID);
            insertData.Parameters.AddWithValue("@authToken", authToken);
            insertData.Parameters.AddWithValue("@authTokenSecret", authTokenSecret);
            insertData.ExecuteNonQuery();
        }
    }

    public void InsertTimetableData(ulong discordID, string name, string start_time, string end_time,
        string room_number, string building_name, string group_number)
    {
        lock (connection)
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
    }

    public void InsertUsersRequest(ulong discordID, string requestToken, string requestTokenSecret)
    {
        lock (connection)
        {
            var insertData = connection.CreateCommand();
            insertData.CommandText =
                "INSERT OR REPLACE INTO usersRequest (discordID, requestToken, requestTokenSecret) VALUES (@discordID, @requestToken, @requestTokenSecret)";
            insertData.Parameters.AddWithValue("@discordID", discordID);
            insertData.Parameters.AddWithValue("@requestToken", requestToken);
            insertData.Parameters.AddWithValue("@requestTokenSecret", requestTokenSecret);
            insertData.ExecuteNonQuery();
        }
    }

    public (string? requestToken, string? requestTokenSecret) SelectUsersRequest(ulong discordID)
    {
        lock (connection)
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
    }
    public (string? authToken, string? authTokenSecret) SelectUsersData(ulong discordID)
    {
        lock (connection)
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

    public void InsertUserTasks(UserTask task)
    {
        lock (connection)
        {
            var command = connection.CreateCommand();
            command.CommandText =
                "INSERT INTO userTasks (discordID, channelID, name, deadlineUtc, reminderUtc) VALUES (@discordID, @channelID, @name, @deadlineUtc, @reminderUtc)";
            command.Parameters.AddWithValue("@discordID", (long)task.UserId);
            command.Parameters.AddWithValue("@channelID", (long)task.ChannelId);
            command.Parameters.AddWithValue("@name", task.Name);
            command.Parameters.AddWithValue("@deadlineUtc", task.Deadline.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"));
            command.Parameters.AddWithValue("@reminderUtc", task.ReminderTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"));
            command.ExecuteNonQuery();
        }
    }

    public List<UserTask> SelectUserTasks()
    {
        lock (connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = 
                "SELECT id, discordID, channelID, name, deadlineUtc, reminderUtc FROM userTasks " +
                "WHERE reminderSent = 0 AND reminderUtc <= @now";
            command.Parameters.AddWithValue("@now", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"));
            var list = new List<UserTask>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new UserTask
                {
                    Id = reader.GetInt64(0),
                    UserId = (ulong)reader.GetInt64(1),
                    ChannelId = (ulong)reader.GetInt64(2),
                    Name = reader.GetString(3),
                    Deadline = DateTime.Parse(reader.GetString(4), null, System.Globalization.DateTimeStyles.RoundtripKind),
                    ReminderTime = DateTime.Parse(reader.GetString(5), null, System.Globalization.DateTimeStyles.RoundtripKind)
                });
            }
            return list;
        }
    }
    public void MarkTaskSent(long id)
    {
        lock (connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = "UPDATE userTasks SET reminderSent = 1 WHERE id = @id";
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }
    }

    public List<ulong> SelectAllUsers() //potrzebne do ocen, sprawdza jaki user jest w bazie i komu wyslac wiadomosc
    {
        lock (connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT discordID FROM usersInfo";
            var list = new List<ulong>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add((ulong)reader.GetInt64(0));
            }
            return list;
        }
    }

    public bool CheckIfUserHasGrade(ulong discordID)
    {
        lock (connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT 1 FROM grades WHERE discordID = @discordID LIMIT 1";
            command.Parameters.AddWithValue("@discordID", (long)discordID);
            return command.ExecuteScalar() != null;
        }
    }

    public bool MarkGradeSeen(ulong discordID, string hash) //wpisuje do tabeli grades ocene jesli istnieje
    {
        lock (connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = "INSERT OR IGNORE INTO grades (discordID, hash) VALUES (@discordID, @hash)";
            command.Parameters.AddWithValue("@discordID", (long)discordID);
            command.Parameters.AddWithValue("@hash", hash);
            return command.ExecuteNonQuery() > 0;
        }
    }
    public bool IsGradeSeen(ulong discordID, string hash)
    {
        lock (connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT 1 FROM grades WHERE discordID = @discordID AND hash = @hash";
            command.Parameters.AddWithValue("@discordID", (long)discordID);
            command.Parameters.AddWithValue("@hash", hash);
            return command.ExecuteScalar() != null;
        }
    }
    
}