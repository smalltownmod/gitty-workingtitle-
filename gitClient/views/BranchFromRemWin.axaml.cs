using Avalonia.Controls;
using gitClient.model;
using LibGit2Sharp;
using System;
using System.IO;
using System.Linq;
using MsBox.Avalonia.Enums;
using static MsBox.Avalonia.MessageBoxManager;
using static gitClient.MainWindow;

namespace gitClient.views {
  public partial class BranchFromRemWin : Window {
    public BranchFromRemWin() {
      InitializeComponent();
      try {
        ComBranchList.ItemsSource = Repo.Branches.Where(b => b.IsRemote).ToList();
        ComBranchList.SelectedIndex = 0;
      }
      catch {
      }
    }

    private void BtnPull(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
      var newBranch = ComBranchList.SelectedItem.ToString().Split("/").Last();
      try {
        Directory.SetCurrentDirectory(Repo.Info.WorkingDirectory);
        ProcInvoker.Run("git", $" pull origin {newBranch}");
        Directory.SetCurrentDirectory(Oldpath);
        Commands.Checkout(Repo, newBranch);

        Close();
      }
      catch (Exception ex) {
        GetMessageBoxStandard("error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error,
          WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
      }
    }

    private void BtnCancel(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
      Close();
    }
  }
}