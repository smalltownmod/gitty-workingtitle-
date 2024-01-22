using Avalonia.Controls;
using Avalonia.Interactivity;
using LibGit2Sharp;
using static MsBox.Avalonia.MessageBoxManager;
using System;
using static gitClient.MainWindow;

namespace gitClient.views {
  public partial class CreateBranchWin : Window {

		public CreateBranchWin() {
			InitializeComponent();
		}
		private void BtnCreateBranch(object sender, RoutedEventArgs ev) {
			try {
				Repo.CreateBranch(NewBranch.Text);
				Commands.Checkout(Repo, Repo.Branches[NewBranch.Text]);
				Close();
			}
			catch (Exception e) {
      GetMessageBoxStandard("Error", e.Message).ShowAsync();
      }
		}
		private void BtnCancel(object sender, RoutedEventArgs e) {
			Close();
		}
	}
}
