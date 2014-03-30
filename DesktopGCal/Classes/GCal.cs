#region License and Summary
// Copyright (c) 2014 Sean Phillips
// Distributed under the BSD License (see LICENSE.md for full license text)
// 
// GCal.cs
//   Defines a class to handle Google Calendar, based on the v3 API.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Util.Store;
using Google.Apis.Services;

namespace Seanph.Calendar.Helpers
{
    public class GCal
    {
        CalendarService _svc;
        string _secretsFilename;

        public GCal(string clientSecrets)
        {
            _secretsFilename = clientSecrets;
        }

        /// <summary>
        ///     Authorises/logs in a user using OAuth
        /// </summary>
        public bool Auth()
        {
            // Define the service
            IList<string> scopes = new List<string>();
            scopes.Add(CalendarService.Scope.Calendar);

            // Start Auth
            // TODO [Auth]: Extract OAuth login to an internal browser or system rather than launching a web browser
            // TODO [Auth]: Add a timeout. Don't be waiting 20 minutes for auth
            UserCredential cred;
            using (FileStream stream = new FileStream(_secretsFilename, FileMode.Open, 
                FileAccess.Read))
            {
                cred = GoogleWebAuthorizationBroker.AuthorizeAsync(
                            GoogleClientSecrets.Load(stream).Secrets,
                            scopes,
                            "user",
                            CancellationToken.None,
                            new FileDataStore(@"seanph.calendar")
                            ).Result;
            }

            var initializer = new BaseClientService.Initializer
            {
                HttpClientInitializer = cred,
                ApplicationName = "DesktopCal"
            };

            _svc = new CalendarService(initializer);

            // return true if successful Auth, return false if auth failed
            return true;
        }

        /// <summary>
        ///     Gets all events from all calendars from Google Calendar
        /// </summary>
        /// <param name="db">The local CalendarDb to save to</param>
        /// <remarks>
        ///     Should only be run once, when first populating the database
        /// </remarks>
        public void GetAll(CalendarDb db)
        {
            // Get list of all calendars
            IList<CalendarListEntry> calendars = _svc.CalendarList.List().Execute().Items;

            foreach (CalendarListEntry cal in calendars)
            {
                db.AddCalendar(cal);

                // Add calendar events
                EventsResource.ListRequest request = _svc.Events.List(cal.Id);

                foreach (Event calendarEvent in request.Execute().Items)
                    db.AddEvent(new CalEvent(calendarEvent, cal.Id));
            }

            db.UpdateLastUpdated();
        }

        /// <summary>
        ///     Gets all events from Google Calendar updated since lastUpdate
        /// </summary>
        public List<CalEvent> GetUpdated(DateTime lastUpdate)
        {
            // Get list of all calendars
            IList<CalendarListEntry> calendars = _svc.CalendarList.List().Execute().Items;
            var events = new List<CalEvent>();

            foreach (CalendarListEntry cal in calendars)
            {
                EventsResource.ListRequest req = _svc.Events.List(cal.Id);
                req.UpdatedMin = lastUpdate;
                foreach (Event calendarEvent in req.Execute().Items)
                    events.Add(new CalEvent(calendarEvent, cal.Id));
            }
            return events;
        }

        /// <summary>
        ///     Uploads new/updated events to Google Calendar.
        ///     Returns the Google Calendar ID of the uploaded event (used to set ID of local events)
        /// </summary>
        public string UploadEvent(CalEvent ev)
        {
            string str;
            // If the ID ends with @google.com, it's an existing event that just needs
            // updating.
            if (ev.Id.EndsWith("@google.com"))
            {
                EventsResource.GetRequest request = _svc.Events.Get(
                    ev.Calendarid, 
                    ev.Id.Remove(ev.Id.Length - 11, 11));

                Event calendarEvent = request.Execute();

                calendarEvent.Summary = ev.Title;
                calendarEvent.Description = ev.Description;
                calendarEvent.Location = ev.Location;
                calendarEvent.Status = (ev.Status ? "confirmed" : "cancelled");

                if (ev.Allday)
                {
                    calendarEvent.Start.DateTime = null;
                    calendarEvent.Start.Date = ev.Startdate.ToString("yyyy-MM-dd");
                    calendarEvent.End.DateTime = null;
                    calendarEvent.End.Date = ev.Enddate.ToString("yyyy-MM-dd");
                }
                else
                {
                    calendarEvent.Start.Date = null;
                    calendarEvent.Start.DateTime = ev.Startdate;
                    calendarEvent.End.Date = null;
                    calendarEvent.End.DateTime = ev.Enddate;
                }
                EventsResource.UpdateRequest req = _svc.Events.Update(
                    calendarEvent, 
                    ev.Calendarid, 
                    ev.Id.Remove(ev.Id.Length - 11, 11));

                str = req.Execute().ICalUID;
            }
            // If the ID ends with something else (eg @deskcal), it isn't yet a Google
            // event and needs to be uploaded
            else
            {
                var calendarEvent = new Event
                {
                    Start = new EventDateTime(),
                    End = new EventDateTime(),
                    Summary = ev.Title,
                    Description = ev.Description,
                    Location = ev.Location,
                    Status = (ev.Status ? "confirmed" : "cancelled")
                };

                if (ev.Allday)
                {
                    calendarEvent.Start.DateTime = null;
                    calendarEvent.End.DateTime = null;
                    calendarEvent.Start.Date = ev.Startdate.ToString("yyyy-MM-dd");
                    calendarEvent.End.Date = ev.Enddate.ToString("yyyy-MM-dd");
                }
                else
                {
                    calendarEvent.Start.Date = null;
                    calendarEvent.End.Date = null;
                    calendarEvent.Start.DateTime = ev.Startdate;
                    calendarEvent.End.DateTime = ev.Enddate;
                }
                EventsResource.InsertRequest req = _svc.Events.Insert(
                    calendarEvent, 
                    ev.Calendarid);

                str = req.Execute().ICalUID;
            }

            return str;
        }
    }
}