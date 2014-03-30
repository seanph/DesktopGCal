#region License and Summary
// Copyright (c) 2014 Sean Phillips
// Distributed under the BSD License (see LICENSE.md for full license text)
// 
// MainWindow.xaml.cs
//   Defines the UI execution for the Calendar application.
#endregion
#region Overall To-Do List
// Used to manage to-do items that aren't localised in code

// TODO [_overall]: More error handling
// TODO [_overall]: Add threading, especially for Calendar Sync and initial setup
// TODO [_overall]: Build Add/Edit event form
// TODO [_overall]: Allow user to sync/display only certain Calendars
//      Also, have this be done seamlessly (without clearing the events list if a new calendar is added)
// TODO [UI]: Better support for all-day events/events that spread over one day
// TODO [UI]: Extract the Scroller/StackPanel to a CalendarControl so that's all we need to interface with
// TODO [UI]: Allow events/days (weekend!) to be coloured
// TODO [xFutureVersion]: Support for Tasks (and Google Tasks API)
// TODO [xFutureVersion]: Add support to parse and manage event descriptions as tasks
//      Eg. a description "[]Do this[x]And this[x]And that[]And that" should output checkBoxes
// TODO [xFutureVersion]: Move the CalendarDB out of the local directory/allow browsing for DBs.
#endregion

using System;
using System.Collections.Generic;
using System.Windows;

using Seanph.Calendar.Helpers;
using Seanph.Calendar.Controls;
using System.IO;

namespace Seanph.Calendar
{
    public partial class MainWindow : Window
    {
        GCal _googleCalendar;
        CalendarDb _db;
        GSyncMgr _syncmgr;
        DateTime _lastLoad;

        public MainWindow()
        {
            InitializeComponent();
        }

        // TODO [UI]: Show more info about the event when "..." button is clicked
        public void MoreButtonClick(string id)
        {
            MessageBox.Show(id);
        }

        private void btnFirstCheck_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists("client_secrets.json"))
            {
                _googleCalendar = new GCal("client_secrets.json");
                _db = new CalendarDb("calendar.db");
                btnAuth.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("Error! Client secrets not found.");
            }
        }

        private void btnAuth_Click(object sender, RoutedEventArgs e)
        {
            if (_googleCalendar.Auth())
            {
                MessageBox.Show("Logged in!");
                btnGetAllFromGCal.IsEnabled = true;
                btnSync.IsEnabled = true;
            }
            else
                MessageBox.Show("Auth failed!");
        }

        private void btnUpdateLastUpdated_Click(object sender, RoutedEventArgs e)
        {
            _db.UpdateLastUpdated();
        }

        private void btnGetAllFromGCal_Click(object sender, RoutedEventArgs e)
        {
            _googleCalendar.GetAll(_db);
        }

        private void btnLoadDB_Click(object sender, RoutedEventArgs e)
        {
            if (_lastLoad.Year == 1)
                _lastLoad = DateTime.Now;

            for (int i = 0; i <= 5; i++)
            {
                var cday = new CalendarDay {Date = _lastLoad};
                List<CalCalendar> cals = _db.LoadDate(_lastLoad);

                foreach (CalCalendar cal in cals)
                {
                    cal.Events.Sort();
                    foreach (CalEvent c in cal.Events)
                        cday.AddEvent(c.Startdate, c.Enddate, c.Title, c.Id, MoreButtonClick);
                }

                stackpanel1.Children.Add(cday);
                _lastLoad = _lastLoad.AddDays(1);
            }
        }

        private void btnLoadMore_Click(object sender, RoutedEventArgs e)
        {
            btnLoadDB_Click(sender, e);
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnSync_Click(object sender, RoutedEventArgs e)
        {
            _syncmgr = new GSyncMgr(_googleCalendar, _db);
            _syncmgr.Sync();
        }
    }
}
