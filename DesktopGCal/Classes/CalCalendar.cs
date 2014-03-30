#region License and Summary
// Copyright (c) 2014 Sean Phillips
// Distributed under the BSD License (see LICENSE.md for full license text)
// 
// CalCalendar.cs
//   Defines a class to handle individual calendars (sets of events)
#endregion

using System.Data;
using System.Collections.Generic;

namespace Seanph.Calendar.Helpers
{
    public class CalCalendar
    {
        public List<CalEvent> Events;
        public string Id;
        public string Title;
        public string Color;

        public CalCalendar(DataRow d)
        {
            Id = (string)d.ItemArray[1];
            Title = (string)d.ItemArray[2];
            Color = (string)d.ItemArray[3];
        }
        /// <summary>
        ///     Loads all events for this calendar from the DB
        /// </summary>
        public List<CalEvent> GetEvents(SqLiteDb sqlite)
        {
            DataTable data = sqlite.DataQuery(
                string.Format(
                    "SELECT * FROM [Events] WHERE CalendarID=\"{0}\";", 
                    Id));

            if (Events == null)
                Events = new List<CalEvent>();
            else
                Events.Clear();

            foreach (DataRow d in data.Rows)
                Events.Add(new CalEvent(d));

            return Events;
        }
    }
}