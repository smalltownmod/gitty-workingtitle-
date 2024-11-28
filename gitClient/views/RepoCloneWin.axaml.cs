using Avalonia.Controls;
using gitClient.model;
using System;
using System.Linq;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MsBox.Avalonia.Enums;
using static MsBox.Avalonia.MessageBoxManager;

namespace gitClient.views {
  public partial class RepoCloneWin : Window {
    public RepoCloneWin() {
      InitializeComponent();
    }

    public void BtnClone(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
      try {
        ProcInvoker.Run("git", $"clone {CloneUrl.Text} {ClonePath.Text}");
        MainWindow.Repo = new(ClonePath.Text);
        MainWindow.Watcher.Path = MainWindow.Repo.Info.WorkingDirectory;
        MainWindow.FetchAll(MainWindow.Repo);
        if (ClonePath.Text != null) MainWindow.UiState(ClonePath.Text);
      }
      catch (Exception ex) {
        GetMessageBoxStandard("Error!", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error,
          WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
      }


      Close();
    }

    private void Cancel_OnClick(object? sender, RoutedEventArgs e) {
      Close();
    }

    private async void Browse_OnClick(object? sender, RoutedEventArgs e) {
      var res = await GetTopLevel(this)!.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions());
      try {
        ClonePath.Text = res.First().TryGetLocalPath() ?? string.Empty;
      }
      catch (Exception ) {
        //nur damit es nicht abraucht
      }
    }
  }
}