using Avalonia.Controls;
using LibGit2Sharp;
using static MsBox.Avalonia.MessageBoxManager;
using System;
using System.Linq;
using MsBox.Avalonia.Enums;

namespace gitClient.views {
  public partial class SwitchBranchWin : Window {
    public SwitchBranchWin() {
      InitializeComponent();
      try {
        ComBranchList.ItemsSource =
          MainWindow.Repo!.Branches.Where(b => !b.IsCurrentRepositoryHead && !b.IsRemote).ToList();
        ComBranchList.SelectedIndex = 0;
      }
      catch {
        //
      }
    }

    private void BtnCancel(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
      Close();
    }

    private void BtnSwitch(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
      try {
        var branchname = ComBranchList.SelectedItem!.ToString();
        var branch = MainWindow.Repo!.Branches[branchname];
        Commands.Checkout(MainWindow.Repo, branch);
        MainWindow.FetchAll(MainWindow.Repo);
      }
      catch (Exception ex) {
        GetMessageBoxStandard("Error!", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error,
          WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
      }

      Close();
    }
  }
}