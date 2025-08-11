using Avalonia.Controls;
using Avalonia.Interactivity;
using gitClient.model;
using LibGit2Sharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace gitClient.views {
  public partial class FileState : UserControl {
    public static List<ItemToCommit> Items { get; set; } = null!;

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

      if (Items.Count(f => f.State.EndsWith('x')) > 0)
        StagePanel.IsVisible = true;
      else StagePanel.IsVisible = false;

      //sorts the output depending on file state
      dgvFileStateNewStaged.ItemsSource = Items.Where(f => f.State.EndsWith('x') && !f.State.StartsWith('D'));
      dgvFileStateDelStage.ItemsSource = Items.Where(f => f.State.EndsWith('x') && f.State.StartsWith('D'));
      dgvFileStateUnstaged.ItemsSource = Items.Where(f => !f.State.EndsWith('x'));
    }


    private void Add_OnClick(object? sender, RoutedEventArgs e) {
      Directory.SetCurrentDirectory(MainWindow.Repo!.Info.WorkingDirectory);
      foreach (var item in Items.Where(i => i.Checked))
        ProcInvoker.Run("git", $" add {item.Path}");
      Directory.SetCurrentDirectory(MainWindow.Oldpath!);
      Supercheck.IsChecked = false;
    }

    private void Remove_OnClick(object? sender, RoutedEventArgs e) {
      Directory.SetCurrentDirectory(MainWindow.Repo!.Info.WorkingDirectory);
      foreach (var item in Items.Where(i => i.Checked))
        ProcInvoker.Run("git", $" restore --staged {item.Path}");
      Directory.SetCurrentDirectory(MainWindow.Oldpath!);
      Supercheck.IsChecked = false;
    }

    private void stage() {
      Directory.SetCurrentDirectory(MainWindow.Repo!.Info.WorkingDirectory);
      foreach (var item in Items.Where(i => i.Checked))
        ProcInvoker.Run("git", $" add {item.Path}");
      Directory.SetCurrentDirectory(MainWindow.Oldpath!);
    }

    private void unstage() {
      Directory.SetCurrentDirectory(MainWindow.Repo!.Info.WorkingDirectory);
     
        ProcInvoker.Run("git", $" restore --staged .");
      Directory.SetCurrentDirectory(MainWindow.Oldpath!);
      Supercheck.IsChecked = false;
    }
    private async void Commit_OnClick(object? sender, RoutedEventArgs e) {
      MainWindow.FetchAll(MainWindow.Repo);
      if (!Items.Where(f => f.Checked).Any()) {
        await MessageBoxManager.GetMessageBoxStandard("Nothing Staged", "Nothing Staged", ButtonEnum.Ok, Icon.Warning)
          .ShowWindowDialogAsync((VisualRoot as Window)!);
        return;
      }
      stage();
      var comwin = new CommitWin();
      comwin.ToCommit(Items.Where(i => i.Checked).ToList());
     await comwin.ShowDialog((VisualRoot as Window)!);
      // Supercheck.IsChecked = false;
      unstage();
    }

    private void Supercheck_change(object? sender, RoutedEventArgs e) {
      RefreshFileState(MainWindow.Repo!);
    }

    private void Revert_OnCLick(object? sender, RoutedEventArgs e) {
      Directory.SetCurrentDirectory(MainWindow.Repo!.Info.WorkingDirectory);
      foreach (var item in Items.Where(i => i.Checked)) {
        ProcInvoker.Run("git", $"restore --staged {item.Path}");
        ProcInvoker.Run("git", $"restore {item.Path}");
      }

      Directory.SetCurrentDirectory(MainWindow.Oldpath!);
      Supercheck.IsChecked = false;
    }

    private void Stash_OnClick(object? sender, RoutedEventArgs e) {
      stage();
      Directory.SetCurrentDirectory(MainWindow.Repo!.Info.WorkingDirectory);
      ProcInvoker.Run("git", "stash");
      Directory.SetCurrentDirectory(MainWindow.Oldpath!);
    }

    private void Unstash_OnClick(object? sender, RoutedEventArgs e) {
      Directory.SetCurrentDirectory(MainWindow.Repo!.Info.WorkingDirectory);
      ProcInvoker.Run("git", $"stash apply");
      ProcInvoker.Run("git", $"stash drop");
      Directory.SetCurrentDirectory(MainWindow.Oldpath!);
      unstage();
    }
  }
}