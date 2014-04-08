#region License and Summary
// Copyright (c) 2014 Sean Phillips
// Distributed under the BSD License (see LICENSE.md for full license text)
//
// PlaceHolderTextBox.xaml.cs
//   Defines operation for the PlaceHolderTextBox control, used to display a
//   textbox which supports placeholder text.
#endregion

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Seanph.Calendar.Controls
{
    /// <summary>
    /// Interaction logic for PlaceHolderTextBox.xaml
    /// </summary>
    public partial class PlaceHolderTextBox : TextBox
    {
        public PlaceHolderTextBox()
        {
            InitializeComponent();
        }

        public string PlaceHolderText { get; set; }
        bool _placeholder = true;

        private void txtInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_placeholder)
            {
                txtInput.Text = "";
                txtInput.Foreground = SystemColors.ControlTextBrush;
            }
        }

        private void txtInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtInput.Text == "")
            {
                txtInput.Text = PlaceHolderText;
                txtInput.Foreground = Brushes.LightGray;
                _placeholder = true;
            }
            else
            {
                _placeholder = false;
            }
        }

        private void txtInput_Loaded(object sender, RoutedEventArgs e)
        {

            txtInput_LostFocus(null, null);
        }
    }
}
