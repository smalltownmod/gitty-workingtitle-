using Avalonia.Controls;
using Avalonia.Interactivity;
using gitClient.model;
using LibGit2Sharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace gitClient.views {
  public partial class FileState : UserControl {
    public List<ItemToCommit> Items { get; set; }

    public FileState() {
      InitializeComponent();
    }

    public void RefreshFileState(Repository repo) {
      if (Supercheck.IsChecked == false)
        Items = repo.RetrieveStatus(new StatusOptions()).Where(i => i.State != FileStatus.Ignored).Select(r => {
          return new ItemToCommit(false, r.State.ToString(), r.FilePath);
        }).ToList();
      else 
        Items = repo.RetrieveStatus(new StatusOptions()).Where(i => i.State != FileStatus.Ignored).Select(r => {
          return new ItemToCommit(true, r.State.ToString(), r.FilePath);
        }).ToList();
      dgvFileState.ItemsSource = Items;
    }


    private void Add_OnClick(object? sender, RoutedEventArgs e) {
      Directory.SetCurrentDirectory(MainWindow.Repo.Info.WorkingDirectory);
      foreach (var item in Items)
        if (item.Checked)
          ProcInvoker.Run("git", $" add {item.Path}");
      Directory.SetCurrentDirectory(MainWindow.Oldpath);
    }

    private void Remove_OnClick(object? sender, RoutedEventArgs e) {
      Directory.SetCurrentDirectory(MainWindow.Repo.Info.WorkingDirectory);
      foreach (var item in Items)
        if (item.Checked)
          ProcInvoker.Run("git", $" restore --staged {item.Path}");
      Directory.SetCurrentDirectory(MainWindow.Oldpath);
    }

    private void Commit_OnClick(object? sender, RoutedEventArgs e) {
      var comwin = new CommitWin();
      comwin.ShowDialog((VisualRoot as Window)!);
    }

    private void Supercheck_change(object? sender, RoutedEventArgs e) {
    RefreshFileState(MainWindow.Repo);
     }

    private void Revert_OnCLick(object? sender, RoutedEventArgs e) {
      Directory.SetCurrentDirectory(MainWindow.Repo.Info.WorkingDirectory);
      foreach (var item in Items) {
        if (item.Checked) {
          ProcInvoker.Run("git", $"restore --staged {item.Path}");
          ProcInvoker.Run("git", $"restore {item.Path}");
        }
      }
      Directory.SetCurrentDirectory(MainWindow.Oldpath);
    }

    private void Stash_OnClick(object? sender, RoutedEventArgs e) {
      Directory.SetCurrentDirectory(MainWindow.Repo.Info.WorkingDirectory);
     ProcInvoker.Run("git", "stash");
     Directory.SetCurrentDirectory(MainWindow.Oldpath);
    }

    private void Unstash_OnClick(object? sender, RoutedEventArgs e) {
      Directory.SetCurrentDirectory(MainWindow.Repo.Info.WorkingDirectory);
      ProcInvoker.Run("git", $"stash apply");
      ProcInvoker.Run("git", $"stash drop");
      Directory.SetCurrentDirectory(MainWindow.Oldpath);
    }
  }
}