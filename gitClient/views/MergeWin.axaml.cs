using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using gitClient.model;
using LibGit2Sharp;

namespace gitClient.views;

public partial class MergeWin : Window {
  public MergeWin() {
    InitializeComponent();
    MergeBlock.Text =
      $"Merge selected branch into {MainWindow.Repo.Branches.Where(b => b.IsCurrentRepositoryHead).First().FriendlyName}";
    try {
      ComBranchList.ItemsSource =
        MainWindow.Repo.Branches.Where(b => !b.IsCurrentRepositoryHead && !b.IsRemote).ToList();
      ComBranchList.SelectedIndex = 0;
    }
    catch { }
  }

  private void BtnMerge(object? sender, RoutedEventArgs e) {
    // throw new System.NotImplementedException();
    try {
      Directory.SetCurrentDirectory(MainWindow.Repo.Info.WorkingDirectory);
      var branchname = ComBranchList.SelectedItem.ToString();
      var branch = MainWindow.Repo.Branches[branchname].FriendlyName;
      ProcInvoker.Run("git", $" merge {branch} --no-commit");
      MainWindow.FetchAll(MainWindow.Repo);
      Directory.SetCurrentDirectory(MainWindow.Oldpath);
      Close();
    }
    catch { }
  }

  private void BtnCancel(object? sender, RoutedEventArgs e) {
    Close();
  }
}