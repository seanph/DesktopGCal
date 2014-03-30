#region License and Summary
// Copyright (c) 2014 Sean Phillips
// Distributed under the BSD License (see LICENSE.md for full license text)
// 
// CalendarEvent.xaml.cs
//   Defines operation for the CalendarEvent control, which is used to display individual events.
//   Built to be used directly by the CalendarDay control and not an individual control.
#endregion

using System;
using System.Windows;
using System.Windows.Controls;

namespace Seanph.Calendar.Controls
{
    public partial class CalendarEvent : UserControl
    {
        DateTime _startDate;
        DateTime _endDate;
        string _data;
        Action<string> _fn;
        public string Id;

        public DateTime Start
        {
            get { return _startDate; }
            set
            {
                _startDate = value;
                lblStartTime.Text = _startDate.ToString("hh:mm");
            }
        }

        public DateTime End
        {
            get { return _endDate; }
            set
            {
                _endDate = value;
                lblEndTime.Text = _endDate.ToString("hh:mm");
            }
        }

        public string Title
        {
            get { return _data; }
            set
            {
                _data = value;
                lblInfo.Text = _data;
            }
        }

        // The function property pertains to the function to be run when the "..." or "more" button is
        // pressed. This allows for an application using the control to choose what's done. 
        // Takes the event ID as a parameter.
        public Action<string> Function
        {
            set { _fn = value; }
        }

        // Removes the bottom border from the control
        // Used by CalendarDay control just to make things look cleaner
        public void RemoveBottomBorder()
        {
            Thickness t = new Thickness(0);
            brdr.BorderThickness = t;
        }

        public CalendarEvent()
        {
            InitializeComponent();
        }

        // TODO [UI]: Replace the event "..." button with a prettier control
        private void btnMore_Click(object sender, RoutedEventArgs e)
        {
            if (_fn != null)
                _fn(Id);
        }
    }
}
