#region License and Summary
// Copyright (c) 2014 Sean Phillips
// Distributed under the BSD License (see LICENSE.md for full license text)
//
// AddEditEvent.xaml.cs
//   Defines operation for the AddEditEvent form, which allows the editing and
//   addition of new events to the local calendar database.
#endregion


using System.Windows;
using Xceed.Wpf.Toolkit;
    
namespace Seanph.Calendar
{
    public partial class AddEditEvent : Window
    {
        // This needs to be implemented next.

        // "Add Mode"
        public AddEditEvent()
        {
            InitializeComponent();
        }

        // "Edit Mode"
        public AddEditEvent(string eventDetails)
        {
            InitializeComponent();
            txtTitle.Text = eventDetails;
        }
    }
}
