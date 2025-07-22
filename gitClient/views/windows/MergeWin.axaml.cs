using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using gitClient.model;

namespace gitClient.views;

public partial class MergeWin : Window {
  public MergeWin() {
    InitializeComponent();
    MergeBlock.Text =
      $"Merge selected branch into {MainWindow.Repo!.Branches.First(b => b.IsCurrentRepositoryHead).FriendlyName}";
    try {
      ComBranchList.ItemsSource =
        MainWindow.Repo.Branches.Where(b => !b.IsCurrentRepositoryHead && !b.IsRemote).ToList();
      ComBranchList.SelectedIndex = 0;
    }
    catch {
      //
    }
  }

  private void BtnMerge(object? sender, RoutedEventArgs e) {
    try {
      Directory.SetCurrentDirectory(MainWindow.Repo!.Info.WorkingDirectory);
      var branchname = ComBranchList.SelectedItem!.ToString();
      var branch = MainWindow.Repo.Branches[branchname].FriendlyName;
      ProcInvoker.Run("git", $" merge {branch} --no-commit");
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