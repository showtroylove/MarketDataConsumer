using System.Windows;
using System.Windows.Input;

namespace TroyStevens.Market.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class RealTimeQuoteMobileView : Window
    {
        public RealTimeQuoteMobileView()
        {
            InitializeComponent();
        }

        #region Code Behind
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void OnPowerOff(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            if (RealTimeApp.Visibility == System.Windows.Visibility.Hidden)
            {
                RealTimeApp.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                RealTimeApp.Visibility = System.Windows.Visibility.Hidden;
            }
        } 
        #endregion
    }
}
