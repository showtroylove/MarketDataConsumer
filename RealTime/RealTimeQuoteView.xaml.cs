using System.Windows;

namespace TroyStevens.Market.Client
{
    public partial class RealTimeQuoteView : Window
    {
        public RealTimeQuoteView()
        {
            InitializeComponent();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            OnEndSessionEvent(e);
        }

        /// <summary>
        /// This function signals the VM, in a loosely coupled way (via interface), so it may close gracefully.
        /// </summary>
        /// <param name="e"></param>
        private void OnEndSessionEvent(System.ComponentModel.CancelEventArgs e)
        {
            if (null == DataContext || !(DataContext is INotifyViewClosing))
                return;

            INotifyViewClosing cmd = (INotifyViewClosing)DataContext;
            if (cmd.OnViewClosing.CanExecute(null))
                cmd.OnViewClosing.Execute(e);
        }
    }
      
}
