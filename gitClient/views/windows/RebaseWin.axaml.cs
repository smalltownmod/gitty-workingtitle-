using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using gitClient.model;

namespace gitClient.views;

public partial class RebaseWin : Window {
  public RebaseWin() {
    InitializeComponent();
    MergeBlock.Text =
      $"Rebase selected branch onto {MainWindow.Repo!.Branches.First(b => b.IsCurrentRepositoryHead).FriendlyName}";

    try {
      ComBranchList.ItemsSource =
        MainWindow.Repo.Branches.Where(b => !b.IsCurrentRepositoryHead).ToList();
      ComBranchList.SelectedIndex = 0;
    }
    catch {
      //
    }
  }

  private void BtnRebase(object? sender, RoutedEventArgs e) {
    try {
      Directory.SetCurrentDirectory(MainWindow.Repo!.Info.WorkingDirectory);
      var branchname = ComBranchList.SelectedItem!.ToString();
      var branch = MainWindow.Repo.Branches[branchname].FriendlyName;
      ProcInvoker.Run("git", $" rebase {branch}");
      MainWindow.FetchAll(MainWindow.Repo);
      Directory.SetCurrentDirectory(MainWindow.Oldpath!);
      Close();
    }
    catch {
      //
    }
  }

  private void BtnCancel(object? sender, RoutedEventArgs e) {
    Close();
  }
}