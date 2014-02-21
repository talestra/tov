using System.Diagnostics;
using System.Windows;

namespace TalesOfVesperiaFrontendWPF
{
    /// <summary>
    /// Interaction logic for AboutForm.xaml
    /// </summary>
    public partial class AboutForm
    {
        public AboutForm()
        {
            InitializeComponent();
            this.GlassBackground(0, 0, 0, 40);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Label_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("http://www.tales-tra.com");
        }
    }
}
