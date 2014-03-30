#region License and Summary
// Copyright (c) 2014 Sean Phillips
// Distributed under the BSD License (see LICENSE.md for full license text)
// 
// CalEvent.cs
//   Defines a class to manage an individual Calendar Event.
#endregion

using System;
using System.Data;
using System.Globalization;
using Google.Apis.Calendar.v3.Data;
using Seanph.DataTools;

namespace Seanph.Calendar.Helpers
{
    public class CalEvent : IComparable<CalEvent>
    {
        public string Id { get; private set; }
        public string Calendarid;
        public string Title;
        public string Description;
        public string Location;
        public bool Allday;
        public DateTime Startdate;
        public DateTime Enddate;
        public bool Status;

        public CalEvent()
        {
            Calendarid = "";
            Id = "";
            Title = "";
            Description = "";
            Location = "";
            Allday = false;
            Startdate = DateTime.Now;
            Enddate = DateTime.Now;
            Status = true;
        }
        
        /// <summary>
        ///     Loads a CalEvent from a DataRow (read in from the CalendarDB)
        /// </summary>
        public CalEvent(DataRow d)
        {
            Calendarid = (string)d.ItemArray[0];
            Id = (string)d.ItemArray[1];
            Title = (string)d.ItemArray[2];
            Description = (string)d.ItemArray[3];
            Location = (string)d.ItemArray[4];
            Allday = Convert.ToBoolean(d.ItemArray[5]);
            Startdate = STime.Unix2DT(Convert.ToUInt32(d.ItemArray[6]));
            Enddate = STime.Unix2DT(Convert.ToUInt32(d.ItemArray[7]));
            Status = Convert.ToBoolean(d.ItemArray[8]);
        }

        /// <summary>
        ///     Loads a CalEvent from a Google Calendar Event object
        /// </summary>
        public CalEvent(Event d, string calendar)
        {
            Calendarid = calendar;
            Id = d.ICalUID;
            Title = d.Summary;
            Description = d.Description;
            Location = d.Location;
            Status = (d.Status == "confirmed");

            if (d.Start == null)
            {
                Startdate = DateTime.Now;
                Enddate = DateTime.Now;
                Allday = false;
            }
            else
            {
                // Handle all-day events
                if (d.Start.Date == null)
                {
                    Startdate = d.Start.DateTime.GetValueOrDefault(DateTime.Now);
                    Enddate = d.End.DateTime.GetValueOrDefault(DateTime.Now);
                    Allday = false;
                }
                else
                {
                    Startdate = DateTime.ParseExact(
                        d.Start.Date, 
                        "yyyy-MM-dd", 
                        CultureInfo.InvariantCulture, 
                        DateTimeStyles.None);

                    Enddate = DateTime.ParseExact(
                        d.Start.Date, 
                        "yyyy-MM-dd", 
                        CultureInfo.InvariantCulture, 
                        DateTimeStyles.None);

                    Allday = true;
                }
            }
        }

        /// <summary>
        ///     Used to generate a temporary random ID for local, unsync'd events
        /// </summary>
        public void GenId()
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random prng = new Random((int)DateTime.Now.Ticks);
            string rndstr = "";
            for (int i = 0; i < 8; i++)
                rndstr += alphabet[prng.Next(alphabet.Length)];

            Id = string.Format("{0}@deskcal", rndstr);
        }

        // To implement IComparable
        public int CompareTo(CalEvent obj)
        {
            if (Startdate < obj.Startdate)
                return -1;

            if (Startdate == obj.Startdate)
                return 0;

            if (Startdate > obj.Startdate)
                return 1;

            return 0;
        }
    }
}