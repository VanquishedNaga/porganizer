﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Data.Sqlite;
using Windows.UI.Xaml.Media.Imaging;

namespace DataAccessLibrary
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

        public static List<DatabaseVideoFile> GetVideoList()
        {
            List<DatabaseVideoFile> entries = new List<DatabaseVideoFile>();

            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT Path from FILE", db);

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

        public static ObservableCollection<Performer> GetPerformerList()
        {
            ObservableCollection<Performer> entries = new ObservableCollection<Performer>();

            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT Name, DateOfBirth, Ethnicity from PERFORMER", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

                    Performer temp = new Performer
                    {
                        Name = query.GetString(0),
                        Ethnicity = textInfo.ToTitleCase(query.GetString(2).ToLower())
                    };

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

    public class DatabaseVideoFile
    {
        public string path;
        int rating;
        int actress;

    }

    public abstract class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Performer : BindableBase
    {
        public Performer()
        {
            string profilePicPlaceholderPath = "ms-appx:///Assets/StoreLogo.scale-400.png";
            ProfilePic = new BitmapImage(new Uri(profilePicPlaceholderPath));
        }

        private string name;
        public string Name
        {
            get { return this.name; }
            set { this.SetProperty(ref this.name, value); }
        }

        private DateTime? dateOfBirth;
        public DateTime? DateOfBirth
        {
            get { return this.dateOfBirth; }
            set { this.SetProperty(ref this.dateOfBirth, value); }
        }

        private string ethnicity;
        public string Ethnicity
        {
            get { return this.ethnicity; }
            set { this.SetProperty(ref this.ethnicity, value); }
        }

        private BitmapImage profilePic;
        public BitmapImage ProfilePic
        {
            get { return this.profilePic; }
            set { this.SetProperty(ref this.profilePic, value); }
        }
    }
}
