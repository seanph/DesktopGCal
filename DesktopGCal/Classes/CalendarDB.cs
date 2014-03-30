#region License and Summary
// Copyright (c) 2014 Sean Phillips
// Distributed under the BSD License (see LICENSE.md for full license text)
// 
// CalendarDB.cs
//   Defines a class to manage a locally stored SQLite calendar database.
#endregion

using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using Google.Apis.Calendar.v3.Data;
using Seanph.DataTools;

namespace Seanph.Calendar.Helpers
{
    public class CalendarDb
    {
        SqLiteDb _calendarSqLite;

        public CalendarDb(string fn)
        {
            if (File.Exists(fn))
                _calendarSqLite = new SqLiteDb(fn);
            else
            {
                _calendarSqLite = new SqLiteDb(fn);
                CreateDb();
            }
        }

        void CreateDb()
        {
            // TODO [LocalDB]: Clear the DB before initializing it (?)
            const string sql = @"CREATE TABLE [CalBase] (
                        [LastSync]  INTEGER NULL
                        );

                        CREATE TABLE [Calendars] (
                        [IsPrimary] BOOLEAN NULL,
                        [ID]        TEXT NULL,
                        [Title]     TEXT NULL,
                        [Color]     TEXT NULL
                        );
                                
                        CREATE TABLE [Events] (
                        [CalendarID]  TEXT NULL,
                        [ID]          TEXT NULL,
                        [Title]       TEXT NULL,
                        [Description] TEXT NULL,
                        [Location]    TEXT NULL,
                        [AllDay]      BOOLAN NULL,
                        [StartDate]   INTEGER NULL,
                        [EndDate]     INTEGER NULL,
                        [Status]      BOOLEAN NULL,
                        [Updated]     BOOLEAN NULL
                        );";
            _calendarSqLite.NondataQuery(sql);
        }
        /// <summary>
        ///     Adds a calendar to the local database from a Google CalendarListEntry object
        /// </summary>
        public void AddCalendar(CalendarListEntry cal)
        {
            _calendarSqLite.NondataQuery(
                string.Format(
                    "INSERT INTO [Calendars] VALUES({0},\"{1}\",\"{2}\",\"{3}\");",
                    (cal.Primary.GetValueOrDefault(false) ? 1 : 0),
                    cal.Id,
                    cal.Summary,
                    cal.ColorId));
        }
        /// <summary>
        ///     Adds an event to the local database from a CalEvent object
        /// </summary>
        public void AddEvent(CalEvent c)
        {
            if (c.Id == null)
            {
                // error
            }
            else
            {
                if (c.Id == "")
                {
                    do
                        c.GenId();
                    while (_calendarSqLite.DataQuery(string.Format("SELECT * FROM [Events] WHERE id=\"{0}\"", c.Id)).Rows.Count > 0);
                }
                _calendarSqLite.NondataQuery(
                    string.Format(
                        "INSERT INTO [Events] VALUES(\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",{5},{6},{7},{8},{9});",
                        c.Calendarid,
                        c.Id,
                        c.Title,
                        c.Description,
                        c.Location,
                        (c.Allday ? 1 : 0),
                        STime.DT2Unix(c.Startdate),
                        STime.DT2Unix(c.Enddate),
                        (c.Status ? 1 : 0),
                        (c.Id.EndsWith("@deskcal") ? 1 : 0)));
            }
        }
        /// <summary>
        ///     Updates the "Last Sync" date to now
        /// </summary>
        public void UpdateLastUpdated()
        {
            _calendarSqLite.NondataQuery(
                string.Format(
                    "INSERT INTO [CalBase] VALUES({0});",
                    STime.DT2Unix(DateTime.Now)));
        }
        /// <summary>
        ///     Gets when the last sync occurred as a DateTime
        /// </summary>
        public DateTime GetLastUpdated()
        {
            return STime.Unix2DT(
                Convert.ToUInt32(
                    _calendarSqLite.DataQuery("SELECT [LastSync] FROM [CalBase]").Rows[0].ItemArray[0]));
        }

        /// <summary>
        ///     Loads all events from all calendars from the local database
        /// </summary>
        public List<CalCalendar> LoadAll()
        {
            var calendars = new List<CalCalendar>();
            DataTable calData = _calendarSqLite.DataQuery("SELECT * FROM [Calendars]");

            foreach (DataRow d in calData.Rows)
                calendars.Add(new CalCalendar(d));

            foreach (CalCalendar cal in calendars)
            {
                DataTable eventData = _calendarSqLite.DataQuery(
                    string.Format(
                        "SELECT * FROM [Events] WHERE CalendarID=\"{0}\";", 
                        cal.Id));

                cal.Events = new List<CalEvent>();

                foreach (DataRow d in eventData.Rows)
                    cal.Events.Add(new CalEvent(d));
            }

            return calendars;
        }
        /// <summary>
        /// Loads all events from all calendars within a period of time from the local database
        /// </summary>
        public List<CalCalendar> LoadPeriod(DateTime from, DateTime until)
        {
            var calendars = new List<CalCalendar>();
            DataTable calData = _calendarSqLite.DataQuery("SELECT * FROM [Calendars]");

            foreach (DataRow d in calData.Rows)
                calendars.Add(new CalCalendar(d));

            foreach (CalCalendar cal in calendars)
            {
                DataTable eventData = _calendarSqLite.DataQuery(
                    string.Format(
                        "SELECT * FROM [Events] WHERE CalendarID=\"{0}\" AND startdate>={1} AND enddate<={2};", 
                        cal.Id, 
                        STime.DT2Unix(from), 
                        STime.DT2Unix(until)));

                cal.Events = new List<CalEvent>();

                foreach (DataRow d in eventData.Rows)
                    cal.Events.Add(new CalEvent(d));
            }

            return calendars;
        }
        /// <summary>    
        ///     Loads all events from all calendars from a certain date from the local database
        /// </summary>
        public List<CalCalendar> LoadDate(DateTime date)
        {
            var calendars = new List<CalCalendar>();
            DataTable calData = _calendarSqLite.DataQuery("SELECT * FROM [Calendars]");

            foreach (DataRow d in calData.Rows)
                calendars.Add(new CalCalendar(d));

            foreach (CalCalendar cal in calendars)
            {
                DataTable eventData = _calendarSqLite.DataQuery(
                    string.Format(
                        "SELECT * FROM [Events] WHERE CalendarID=\"{0}\" AND startdate>{1} AND startdate<{2};", 
                        cal.Id, 
                        STime.DT2Unix(date), 
                        STime.DT2Unix(date.AddDays(1))));

                cal.Events = new List<CalEvent>();

                foreach (DataRow d in eventData.Rows)
                    cal.Events.Add(new CalEvent(d));
            }

            return calendars;
        }
        /// <summary>
        ///     Loads all events from an individual calendar from the local database
        /// </summary>
        public CalCalendar LoadCalendar(string id)
        {
            DataTable calData = _calendarSqLite.DataQuery(string.Format("SELECT * FROM [Calendars] WHERE id=\"{0}\"", id));

            CalCalendar calendar = null;
            foreach (DataRow d in calData.Rows)
                calendar = new CalCalendar(d);

            if (calendar != null)
            {
                DataTable eventData = _calendarSqLite.DataQuery(
                    string.Format(
                        "SELECT * FROM [Events] WHERE CalendarID=\"{0}\";", 
                        id));

                calendar.Events = new List<CalEvent>();

                foreach (DataRow d in eventData.Rows)
                    calendar.Events.Add(new CalEvent(d));
            }

            return calendar;
        }

        /// <summary>
        ///     Loads an individual event from the local database
        /// </summary>
        public CalEvent LoadEvent(string id)
        {
            DataTable eventData = _calendarSqLite.DataQuery(
                string.Format(
                    "SELECT * FROM [Events] WHERE id=\"{0}\";", 
                    id));

            return new CalEvent(eventData.Rows[0]);
        }

        /// <summary>
        ///     Updates an individual event in the local database
        /// </summary>
        public void UpdateEvent(CalEvent calEv, bool localUpdate)
        {
            _calendarSqLite.NondataQuery(
                string.Format(
                    "UPDATE [Events] SET Title=\"{1}\", Description=\"{2}\", Location=\"{3}\", StartDate={4}, EndDate={5}, Status={6}, Updated={7} WHERE ID=\"{0}\";",
                    calEv.Id,
                    calEv.Title,
                    calEv.Description,
                    calEv.Location,
                    STime.DT2Unix(calEv.Startdate),
                    STime.DT2Unix(calEv.Enddate),
                    (calEv.Status ? 1 : 0),
                    (localUpdate ? 1 : 0)));
        }

        /// <summary>
        ///     Removes an individual event from the local database
        /// </summary>
        // TODO [LocalDB]: Add a function to fully delete an individual event from the local DB
        public void RemoveEvent(string id)
        {
            _calendarSqLite.NondataQuery(
                string.Format(
                    "UPDATE [Events] SET Status=0, Updated=1 WHERE ID=\"{0}\";",
                    id));
        }
        /// <summary>
        ///     Loads all events that have IDs that match a list of other events, AND ALSO
        ///     that have been updated since the last sync
        /// </summary>
        /// <remarks>
        ///     Used to find clashing events
        /// </remarks>
        public List<CalEvent> LoadUpdatedEvents(List<CalEvent> searchevents)
        {
            string sqlquery = "SELECT * FROM [Events] WHERE ID IN ( ";
            foreach (CalEvent c in searchevents)
                sqlquery += string.Format("\"{0}\",", c.Id);
            sqlquery = sqlquery.Remove(sqlquery.Length - 1) + ") AND Updated=1;";

            DataTable updatedItems = _calendarSqLite.DataQuery(sqlquery);
            List<CalEvent> cals = new List<CalEvent>();
            foreach (DataRow d in updatedItems.Rows)
                cals.Add(new CalEvent(d));

            return cals;
        }

        /// <summary>
        ///     Loads all events that have IDs that match a list of other events
        /// </summary>
        public List<CalEvent> LoadEvents(List<CalEvent> searchevents)
        {
            string sqlquery = "SELECT * FROM [Events] WHERE ID IN ( ";
            foreach (CalEvent c in searchevents)
                sqlquery += string.Format("\"{0}\",", c.Id);
            sqlquery = sqlquery.Remove(sqlquery.Length - 1) + ");";

            DataTable dataItems = _calendarSqLite.DataQuery(sqlquery);
            List<CalEvent> cals = new List<CalEvent>();
            foreach (DataRow d in dataItems.Rows)
                cals.Add(new CalEvent(d));

            return cals;
        }

        /// <summary>
        ///     Loads all events that have been updated locally
        /// </summary>
        public List<CalEvent> LoadUpdatedEvents()
        {
            const string sqlquery = "SELECT * FROM [Events] WHERE Updated=1;";

            DataTable dataItems = _calendarSqLite.DataQuery(sqlquery);
            List<CalEvent> cals = new List<CalEvent>();
            foreach (DataRow d in dataItems.Rows)
                cals.Add(new CalEvent(d));

            return cals;
        }

        /// <summary>
        ///     Changes the ID of an event
        /// </summary>
        /// <remarks>
        ///     Used to fix IDs when local events are sync'd with Google Calendar
        /// </remarks>
        public void UpdateId(CalEvent c, string newId)
        {
            _calendarSqLite.NondataQuery(
                string.Format(
                    "UPDATE [Events] SET id=\"{0}\" WHERE id=\"{1}\";", 
                    newId, 
                    c.Id));
        }

        /// <summary>
        ///     Loads a list of all calendars stored in the local DB
        /// </summary>
        public List<CalCalendar> LoadCalendars()
        {
            const string sqlquery = "SELECT * FROM [Calendars];";
            DataTable dataItems = _calendarSqLite.DataQuery(sqlquery);

            var cals = new List<CalCalendar>();
            foreach (DataRow d in dataItems.Rows)
                cals.Add(new CalCalendar(d));

            return cals;
        }
    }
}