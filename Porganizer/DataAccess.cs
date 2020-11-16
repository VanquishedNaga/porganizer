using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Microsoft.Data.Sqlite;

namespace Porganizer
{
    public static class DataAccess
    {
        // Create the database tables if they don't already exist.
        public static void InitializeDatabase()
        {
            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();

                String tableCommand =
                    "CREATE TABLE IF NOT EXISTS SERIES (" +
                    "   SeriesId        INTEGER         PRIMARY KEY, " +
                    "   Name            NVARCHAR(50)    NOT NULL        UNIQUE" +
                    ");" +
                    "CREATE TABLE IF NOT EXISTS FILE (" +
                    "   FileId          INTEGER         PRIMARY KEY, " +
                    "   Path            NVARCHAR(2048)  NOT NULL        UNIQUE," +
                    "   ImportDate      DATE            NOT NULL," +
                    "   Size            INTEGER         NOT NULL," +
                    "   Ranking         INTEGER," +
                    "   ReleaseDate     DATE," +
                    "   SeriesId        INTEGER," +
                    "   CONSTRAINT SeriFK FOREIGN KEY (SeriesId)" +
                    "       REFERENCES SERIES (SeriesId)" +
                    ");" +
                    "CREATE TABLE IF NOT EXISTS PERFORMER (" +
                    "   PerformerId     INTEGER         PRIMARY KEY, " +
                    "   Name            NVARCHAR(50)    NOT NULL        UNIQUE," +
                    "   DateOfBirth     DATE," +
                    "   Ranking         INTEGER," +
                    "   Ethnicity       NVARCHAR(25)    NOT NULL" +
                    "       CHECK (Ethnicity IN ('JAPANESE', 'AMERICAN'))" +
                    ");" +
                    "CREATE TABLE IF NOT EXISTS PERFORMANCE (" +
                    "   FileId          INTEGER," +
                    "   PerformerId     INTEGER," +
                    "   CONSTRAINT FileFK FOREIGN KEY (FileId)" +
                    "       REFERENCES FILE (FileId)," +
                    "   CONSTRAINT PerfFK FOREIGN KEY (PerformerId)" +
                    "       REFERENCES PERFORMER (PerformerId)," +
                    "   UNIQUE (FileId, PerformerId)" +
                    ");";

                SqliteCommand createTable = new SqliteCommand(tableCommand, db);

                createTable.ExecuteReader();

                db.Close();
            }
        }

        public static void AddFile(string path, int size)
        {
            if (!IsFileInDatabase(path))
            {
                using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
                {
                    db.Open();

                    SqliteCommand insertCommand = new SqliteCommand();
                    insertCommand.Connection = db;

                    // Use parameterized query to prevent SQL injection attacks
                    insertCommand.CommandText =
                        "INSERT INTO FILE (Path, ImportDate, Size)" +
                        "   VALUES (@Path, CURRENT_DATE, @Size);";
                    insertCommand.Parameters.AddWithValue("@Path", path);
                    insertCommand.Parameters.AddWithValue("@Size", size);

                    insertCommand.ExecuteReader();

                    db.Close();
                }
            }
        }

        public static void RemoveFileFromDatabase(string path)
        {
            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();

                SqliteCommand deleteCommand = new SqliteCommand("DELETE FROM FILE WHERE Path=\"" + path + "\"", db);
                deleteCommand.ExecuteReader();
            }
        }

        public static bool IsFileInDatabase(string path)
        {
            bool isInDatabase = false;

            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();

                using (SqliteCommand selectCommand = new SqliteCommand("SELECT EXISTS(SELECT 1 FROM FILE WHERE Path=\"" + path + "\")", db))
                {
                    // Investigate why this does not work.
                    //selectCommand.Parameters.AddWithValue("@Path", path);

                    using (SqliteDataReader query = selectCommand.ExecuteReader())
                    {
                        query.Read();
                        isInDatabase = query.GetBoolean(0);
                    }
                }
            }

            return isInDatabase;
        }

        public static List<VideoFile> GetVideoList()
        {
            List<VideoFile> entries = new List<VideoFile>();

            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT FileId, Path from FILE", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    VideoFile temp = new VideoFile(query.GetInt16(0), query.GetString(1));
                    entries.Add(temp);
                }

                db.Close();
            }

            return entries;
        }

        public static void AddPerformer(string name, DateTimeOffset? date, string ethnicity)
        {
            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                Object tempDate = DBNull.Value;

                if (date.HasValue)
                {
                    tempDate = date.Value.Date;
                }

                // Use parameterized query to prevent SQL injection attacks
                insertCommand.CommandText =
                    "INSERT INTO PERFORMER (Name, DateOfBirth, Ethnicity)" +
                    "   VALUES (@Name, @Date, @Ethnicity);";
                insertCommand.Parameters.AddWithValue("@Name", name);
                insertCommand.Parameters.AddWithValue("@Date", tempDate);
                insertCommand.Parameters.AddWithValue("@Ethnicity", ethnicity.ToUpper());

                insertCommand.ExecuteReader();

                db.Close();
            }
        }

        public static void UpdatePerformer(string oldName, string newName, DateTimeOffset? date, string ethnicity)
        {
            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();

                SqliteCommand updateCommand = new SqliteCommand();
                updateCommand.Connection = db;

                Object tempDate = DBNull.Value;

                if (date.HasValue)
                {
                    tempDate = date.Value.Date;
                }

                // Use parameterized query to prevent SQL injection attacks
                updateCommand.CommandText =
                    "UPDATE PERFORMER " +
                    "   SET Name = @NewName, " +
                    "       DateOfBirth = @Date, " +
                    "       Ethnicity = @Ethnicity" +
                    "   WHERE Name = @OldName;";
                updateCommand.Parameters.AddWithValue("@NewName", newName);
                updateCommand.Parameters.AddWithValue("@Date", tempDate);
                updateCommand.Parameters.AddWithValue("@Ethnicity", ethnicity.ToUpper());
                updateCommand.Parameters.AddWithValue("@OldName", oldName);

                updateCommand.ExecuteReader();

                db.Close();
            }
        }

        public static void DeletePerformer(string name)
        {
            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();

                SqliteCommand deleteCommand = new SqliteCommand();
                deleteCommand.Connection = db;

                // Use parameterized query to prevent SQL injection attacks
                deleteCommand.CommandText =
                    "DELETE FROM PERFORMER " +
                    "   WHERE Name = @Name;";
                deleteCommand.Parameters.AddWithValue("@Name", name);

                deleteCommand.ExecuteReader();

                db.Close();
            }
        }

        public static ObservableCollection<Performer> GetPerformerList()
        {
            ObservableCollection <Performer> entries = new ObservableCollection<Performer>();

            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand(
                    "SELECT Name, DateOfBirth, Ethnicity, PerformerId from PERFORMER " +
                    "   ORDER BY Name", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

                    Performer temp = new Performer(query.GetInt16(3), query.GetString(0), textInfo.ToTitleCase(query.GetString(2).ToLower()));

                    if (!query.IsDBNull(1))
                    {
                        temp.DateOfBirth = query.GetDateTime(1);
                    }
                    else
                    {
                        temp.DateOfBirth = null;
                    }

                    entries.Add(temp);
                }

                db.Close();
            }

            return entries;
        }

        public static void AddPerformerToFile(int fileId, int performerId)
        {
            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                // Use parameterized query to prevent SQL injection attacks
                insertCommand.CommandText =
                    "INSERT INTO PERFORMANCE (FileId, PerformerId)" +
                    "   VALUES (@FileId, @PerformerId);";
                insertCommand.Parameters.AddWithValue("@FileId", fileId);
                insertCommand.Parameters.AddWithValue("@PerformerId", performerId);

                try
                {
                    insertCommand.ExecuteReader();
                }
                catch (Exception)
                {

                }

                db.Close();
            }
        }

        public static ObservableCollection<Performer> GetFilePerformers(int fileId)
        {
            ObservableCollection<Performer> entries = new ObservableCollection<Performer>();

            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand (
                    "SELECT Name, DateOfBirth, Ethnicity, PerformerId FROM PERFORMER " +
                    "   WHERE EXISTS(SELECT PerformerId FROM PERFORMANCE " +
                    "                WHERE FileId = @FileId AND PERFORMER.PerformerId = PERFORMANCE.PerformerId) " +
                    "   ORDER BY Name", db);
                selectCommand.Parameters.AddWithValue("@FileId", fileId);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

                    Performer temp = new Performer(query.GetInt16(3), query.GetString(0), textInfo.ToTitleCase(query.GetString(2).ToLower()));

                    if (!query.IsDBNull(1))
                    {
                        temp.DateOfBirth = query.GetDateTime(1);
                    }
                    else
                    {
                        temp.DateOfBirth = null;
                    }

                    entries.Add(temp);
                }

                db.Close();
            }

            return entries;
        }
    }
}
