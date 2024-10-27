using Avalonia.Controls;
using System.IO;
using gitClient.model;
using System.Text.Json;
using LibGit2Sharp;
using System;
using LibGit2Sharp.Handlers;
using static MsBox.Avalonia.MessageBoxManager;
using System.Linq;

namespace gitClient.views {
  public partial class RepoCloneWin : Window {
    public RepoCloneWin() {
      InitializeComponent();
    }
    public void BtnClone(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
      try {

        ProcInvoker.Run("git", $"clone {CloneUrl.Text} {ClonePath.Text}");
        MainWindow.Repo = new(ClonePath.Text);
        MainWindow.watcher.Path = MainWindow.Repo.Info.WorkingDirectory;
        MainWindow.FetchAll(MainWindow.Repo);
        MainWindow.UiState(ClonePath.Text);
      }
      catch(Exception ex) {
        GetMessageBoxStandard("Error", ex.Message).ShowAsync();
      }
      Close();
    }
  }
}
