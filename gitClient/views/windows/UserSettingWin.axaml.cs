using Avalonia.Controls;
using LibGit2Sharp;
using gitClient.model;

namespace gitClient.views {
  public partial class UserSettingWin : Window {
		public UserSettingWin() {
			InitializeComponent();
		}
		private void BtnCancel_Click(object sender, Avalonia.Interactivity.RoutedEventArgs ev) {
			Close();
		}
		private void BtnSubmit_Click(object sender, Avalonia.Interactivity.RoutedEventArgs ev) {
			ProcInvoker.Run("git", $" config --global user.name \"{UsrNamBox.Text}\"");
			ProcInvoker.Run("git", $" config --global user.email {UsrMailBox.Text}");
			Close();
		}
		public void FirstOpen(Repository r) {
			if (r == null!) return;
			Signature autor = r.Config.BuildSignature(System.DateTimeOffset.Now);
			UsrNamBox.Text = autor.Name;
			UsrMailBox.Text = autor.Email;
		}
	}
}
