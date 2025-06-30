using System.Windows;

namespace LogcatViewer
{
    public partial class ApkDetailWindow : Window
    {
        public ApkDetailWindow(string apkDetailText)
        {
            InitializeComponent();
            ApkDetailTextBlock.Text = apkDetailText;
        }
    }
}