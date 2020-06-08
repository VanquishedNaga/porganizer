using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Data.Sqlite;

namespace DataAccessLibrary
{
    public static class DataAccess
    {
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
                    "   Size            INTEGER," +
                    "   Ranking         INTEGER," +
                    "   ReleaseDate     DATE," +
                    "   ImportDate      DATE            NOT NULL," +
                    "   SeriesId        INTEGER," +
                    "   CONSTRAINT SeriFK FOREIGN KEY (SeriesId)" +
                    "       REFERENCES SERIES (SeriesId)" +
                    ");" +
                    "CREATE TABLE IF NOT EXISTS PERFORMER (" +
                    "   PerformerId     INTEGER         PRIMARY KEY, " +
                    "   Name            NVARCHAR(50)    NOT NULL        UNIQUE," +
                    "   DateOfBirth     INTEGER," +
                    "   Ranking         INTEGER" +
                    ");" +
                    "CREATE TABLE IF NOT EXISTS PERFOMANCE (" +
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

        public static void InitializeDatabaseOld()
        {
            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();

                String tableCommand = "CREATE TABLE IF NOT " +
                    "EXISTS MyTable (Primary_Key INTEGER PRIMARY KEY, " +
                    "Text_Entry NVARCHAR(2048) NULL)";

                SqliteCommand createTable = new SqliteCommand(tableCommand, db);

                createTable.ExecuteReader();

                db.Close();
            }
        }

        public static void AddFile(string path)
        {
            if (!IsInDatabase(path))
            {
                using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
                {
                    db.Open();

                    SqliteCommand insertCommand = new SqliteCommand();
                    insertCommand.Connection = db;

                    // Use parameterized query to prevent SQL injection attacks
                    insertCommand.CommandText = "INSERT INTO MyTable VALUES (NULL, @Entry);";
                    insertCommand.Parameters.AddWithValue("@Entry", path);

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

                SqliteCommand deleteCommand = new SqliteCommand("DELETE FROM MyTable WHERE Text_Entry=\"" + path + "\"", db);
                deleteCommand.ExecuteReader();
            }
        }

        public static bool IsInDatabase(string path)
        {
            bool isInDatabase = false;

            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();

                using (SqliteCommand selectCommand = new SqliteCommand("SELECT EXISTS(SELECT 1 FROM MyTable WHERE Text_Entry=\"" + path + "\")", db))
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

        public static List<String> GetData()
        {
            List<String> entries = new List<string>();

            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT Text_Entry from MyTable", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    entries.Add(query.GetString(0));
                }

                db.Close();
            }

            return entries;
        }

        public static List<DatabaseVideoFile> GetVideoList()
        {
            List<DatabaseVideoFile> entries = new List<DatabaseVideoFile>();

            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT Text_Entry from MyTable", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    DatabaseVideoFile temp = new DatabaseVideoFile
                    {
                        path = query.GetString(0)
                    };
                    entries.Add(temp);
                }

                db.Close();
            }

            return entries;
        }
    }

    public class DatabaseVideoFile
    {
        public string path;
        int rating;
        int actress;

    }
}
