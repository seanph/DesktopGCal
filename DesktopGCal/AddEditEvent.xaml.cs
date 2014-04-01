using System.Windows;
using System.Windows.Media;

namespace Seanph.Calendar
{
    public partial class AddEditEvent : Window
    {
        public AddEditEvent()
        {
            InitializeComponent();
        }

        //----
        // "Placeholder text" textbox code
        // TODO [UI]: Extract placeholder text textbox code to a control
        bool _placeholder = true;
        private void txtTitle_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_placeholder)
            {
                txtTitle.Text = "";
                txtTitle.Foreground = SystemColors.ControlTextBrush;
            }
        }

        private void txtTitle_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtTitle.Text == "")
            {
                txtTitle.Text = "Event Title";
                txtTitle.Foreground = Brushes.LightGray;
                _placeholder = true;
            }
            else
            {
                _placeholder = false;
            }
        }
        //----

        // TODO [UI]: Build a DateTime picker component
        // For time, maybe use a UpDown style component that goes in 15 minute increments
        // Obviously, has to allow user to manually enter times too.
        // Pair this with a Calendar control for date.
    }
}
