using Avalonia.Controls;
using LibGit2Sharp;
using static MsBox.Avalonia.MessageBoxManager;
using System;
using MsBox.Avalonia.Enums;

namespace gitClient.views;

public partial class CommitWin : Window {
  public CommitWin() {
    InitializeComponent();
  }

  private void Btn_Commit(object sender, Avalonia.Interactivity.RoutedEventArgs ev) {
    try {
      Signature author = MainWindow.Repo!.Config.BuildSignature(DateTimeOffset.Now);
      MainWindow.Repo.Commit(MsgCommit.Text, author, author);
      Close();
    }
    catch (Exception ex) {
      GetMessageBoxStandard("Error!", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.None,
        WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
    }
  }

  private void Btn_Cancel(object sender, Avalonia.Interactivity.RoutedEventArgs ev) {
    Close();
  }
}