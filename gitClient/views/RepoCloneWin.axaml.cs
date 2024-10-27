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
      //if(!File.Exists(@"cred.json"))
      //  GetMessageBoxStandard("Error", "Please fill out Credentials in Usersettings.").ShowAsync();
      //  var cred = JsonSerializer.Deserialize<GitCred>(File.ReadAllText(@"cred.json")) ?? new GitCred();
      
      try {

        ProcInvoker.Run("git", $"clone {CloneUrl.Text} {ClonePath.Text}");
        //var creds = new UsernamePasswordCredentials() { Username = cred.GitName, Password = String.Empty };
        //var co = new CloneOptions();
        //co.CredentialsProvider = (_url, _user, _cred) => creds;
        //Repository.Clone(CloneUrl.Text, ClonePath.Text, co);
        //var logmessage = "";
        //Repository repo = new(ClonePath.Text);
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
