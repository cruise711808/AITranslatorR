using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AITranslator.View.UserControls
{
    /// <summary>
    /// UserControl_LogsView.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl_LogsView : UserControl
    {
        public UserControl_LogsView()
        {
            InitializeComponent();
        }

        public void ShowInit()
        {
            logViewInit.Visibility = Visibility.Visible;
            logViewApi.Visibility = Visibility.Collapsed;
        }

        public void ShowApi()
        {
            logViewInit.Visibility = Visibility.Collapsed;
            logViewApi.Visibility = Visibility.Visible;
        }

        public void ViewToItem()
        {
            logViewApi.ScrollToBottom();
        }
    }
}
