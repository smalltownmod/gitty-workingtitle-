using Avalonia.Controls;
using LibGit2Sharp;
using static MsBox.Avalonia.MessageBoxManager;
using System;

namespace gitClient.views;

public partial class CommitWin : Window {
  public CommitWin() {
    InitializeComponent(); 
  }
  private void Btn_Commit(object sender, Avalonia.Interactivity.RoutedEventArgs ev) {
    try {
      Commands.Stage(MainWindow.Repo, "*");
      Signature author =  MainWindow.Repo.Config.BuildSignature(DateTimeOffset.Now);
      MainWindow.Repo.Commit(MsgCommit.Text, author, author);
      Close();
    }
    catch (Exception e) {
      _ = GetMessageBoxStandard("Error", e.Message).ShowAsync();
    }
  }
  private void Btn_Cancel(object sender, Avalonia.Interactivity.RoutedEventArgs ev) {
    Close();
  }
}