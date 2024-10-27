using Avalonia.Controls;
using LibGit2Sharp;
using gitClient.model;
using System.Text.Json;
using System.IO;

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
			//	GitCred gitCred = new GitCred();
			//	gitCred.GitName = GhUsrNamBox.Text.Trim();
			//	gitCred.GitPassword = string.Empty;
			//	var jFile = JsonSerializer.Serialize(gitCred);
			//	File.WriteAllText(("cred.json"), jFile);
			Close();
		}
		public void FirstOpen(Repository r) {
			if (r != null) {
				Signature Autor = r.Config.BuildSignature(System.DateTimeOffset.Now);
				UsrNamBox.Text = Autor.Name;
				UsrMailBox.Text = Autor.Email;	
			}

			//if(File.Exists(@"cred.json")) {
			//	var cred = JsonSerializer.Deserialize<GitCred>(File.ReadAllText(@"cred.json")) ?? new GitCred();
			//	GhUsrNamBox.Text = cred.GitName;
			//	//GhPWBox.Text = cred.GitPassword;
			//}
		}
	}
}
