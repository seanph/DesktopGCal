#region License and Summary
// Copyright (c) 2014 Sean Phillips
// Distributed under the BSD License (see LICENSE.md for full license text)
//
// CalendarDay.xaml.cs
//   Defines operation for the CalendarDay control, used to display an 
//   individual day's worth of events. Events are displayed using a StackPanel, 
//   which is populated with CalendarEvent controls.
#endregion

using System;
using System.Windows;
using System.Windows.Controls;


namespace Seanph.Calendar.Controls
{
    public partial class CalendarDay : UserControl
    {
        DateTime _date;

        public DateTime Date {
            get { return _date; }
            set
            {
                _date = value;
                lblDay.Text = _date.ToString("ddd");
                lblDate.Text = _date.ToString("dd MMMM");
            }
        }
        
        /// <summary>
        ///     Adds an event to the "day"
        /// </summary>
        /// <param name="start">Start Date of the event</param>
        /// <param name="end">End Date of the event</param>
        /// <param name="info">Event title</param>
        /// <param name="id">Event ID</param>
        /// <param name="fn">Function to be run when the "..." button is clicked</param>
        // TODO [UI]: Add support for event description display
        public void AddEvent(DateTime start, DateTime end, string info, string id, Action<string> fn)
        {
            var cday = new CalendarEvent{
                Start = start, 
                End = end, 
                Id = id, 
                Title = info, 
                Function = fn
            };

            stkPnl.Children.Add(cday);
        }

        // Removes the border from the last CalendarEvent that was added.
        // Since the CalendarDay has its own lower border, the last event having a border
        // ends up looking odd, especially if it doesn't line up with the bottom of the
        // control.
        public void RemoveLastBorder()
        {
            if (stkPnl.Children.Count <= 0) 
                return;

            UIElement lastElement = stkPnl.Children[stkPnl.Children.Count - 1];
            (lastElement as CalendarEvent).RemoveBottomBorder();
            stkPnl.Children[stkPnl.Children.Count - 1] = lastElement;
        }

        public CalendarDay()
        {
            InitializeComponent();
        }
    }
}
