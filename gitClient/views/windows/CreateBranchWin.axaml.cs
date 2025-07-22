using Avalonia.Controls;
using Avalonia.Interactivity;
using LibGit2Sharp;
using static MsBox.Avalonia.MessageBoxManager;
using System;
using MsBox.Avalonia.Enums;
using static gitClient.MainWindow;

namespace gitClient.views {
  public partial class CreateBranchWin : Window {
    public CreateBranchWin() {
      InitializeComponent();
    }

    private void BtnCreateBranch(object sender, RoutedEventArgs ev) {
      try {
        Repo.CreateBranch(NewBranch.Text);
        Commands.Checkout(Repo, Repo!.Branches[NewBranch.Text]);
        Close();
      }
      catch (Exception ex) {
        GetMessageBoxStandard("Error!", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error,
          WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
      }
    }

    private void BtnCancel(object sender, RoutedEventArgs e) {
      Close();
    }
  }
}