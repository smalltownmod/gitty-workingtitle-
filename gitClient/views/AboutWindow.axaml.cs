using Avalonia.Controls;
using Avalonia.Interactivity;

namespace gitClient.views {
	public partial class AboutWindow : Window {
		public AboutWindow() {
			InitializeComponent();
		}
		public void BtnOk(object sender, RoutedEventArgs e) {
			Close();
		}
	}
}
