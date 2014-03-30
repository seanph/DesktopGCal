#region License and Summary
// Copyright (c) 2014 Sean Phillips
// Distributed under the BSD License (see LICENSE.md for full license text)
// 
// GSyncMgr.cs
//   Defines a synchronisation class to sync between a local SQLite database 
//   (as defined in CalendarDB.cs), and Google Calendar using the v3 API.
#endregion

using System;
using System.Threading;
using System.Collections.Generic;

namespace Seanph.Calendar.Helpers
{
    public class GSyncMgr
    {
        GCal _googleCalendar;
        CalendarDb _localCalendar;

        public GSyncMgr(GCal google, CalendarDb local)
        {
            _googleCalendar = google;
            _localCalendar = local;
        }

        public void Sync()
        {
            // Get last sync time
            DateTime syncdt = _localCalendar.GetLastUpdated();

            // Get updated events from Google
            List<CalEvent> googleEvents = _googleCalendar.GetUpdated(syncdt);

            List<CalEvent> clashingEvents = _localCalendar.LoadUpdatedEvents(googleEvents);
            foreach (CalEvent ev in clashingEvents)
            {
                CalEvent gv = googleEvents.Find(x => x.Id == ev.Id);

                // TODO [Sync]: Ask user whether to use local or Google event when clash occurs. 
                // (Currently assuming Google event)

                _localCalendar.UpdateEvent(gv, false);
                googleEvents.Remove(gv);
            }

            // Update remaining local items
            List<CalEvent> updatedEvents = _localCalendar.LoadEvents(googleEvents);
            foreach (CalEvent ev in updatedEvents)
            {
                CalEvent gv = googleEvents.Find(x => x.Id == ev.Id);
                _localCalendar.UpdateEvent(gv, false);
                googleEvents.Remove(gv);
            }

            // Insert new events into local database
            foreach (CalEvent ev in googleEvents)
                _localCalendar.AddEvent(ev);

            // Upload new local events
            List<CalEvent> updated = _localCalendar.LoadUpdatedEvents();
            foreach (CalEvent ev in updated)
            {
                // Upload the event and fix its ID to the Google Calendar ID
                string id = _googleCalendar.UploadEvent(ev);
                _localCalendar.UpdateId(ev, id);
                // To keep under 5 request/sec limit
                Thread.Sleep(250);
            }
        }
    }
}