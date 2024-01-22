using Avalonia.Controls;
using gitClient.model;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using static MsBox.Avalonia.MessageBoxManager;
using static gitClient.MainWindow;

namespace gitClient.views {
  public partial class BranchFromRemWin : Window {
    //private Repository repo { get; set; }
    public BranchFromRemWin() {
      InitializeComponent();
      try {
        ComBranchList.ItemsSource = Repo.Branches.Where(b => b.IsRemote).ToList();
        ComBranchList.SelectedIndex = 0;
      }
      catch { }
    }
    private void BtnPull(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
      var newBranch = ComBranchList.SelectedItem.ToString().Split("/").Last();
      var sig = Repo.Config.BuildSignature(DateTimeOffset.Now);
      var cred = JsonSerializer.Deserialize<GitCred>(File.ReadAllText(@"cred.json")) ?? new GitCred();
      try {
        Branch NewBranch = Repo.Branches.Add(newBranch, Repo.Commits.Last().Id.Sha);
        var tracking = Repo.Branches[$"origin/{NewBranch.FriendlyName}"];
        var po = new PullOptions();
        po.FetchOptions = new FetchOptions();
        po.FetchOptions.CredentialsProvider = new CredentialsHandler(
          (url, usernameFromUrl, types) => new UsernamePasswordCredentials() { Username = cred.GitName, Password = string.Empty });
        var MergeSig = new Signature(
          new Identity(sig.Name, sig.Email), DateTimeOffset.Now);
        Repo.Branches.Update(NewBranch, b => b.TrackedBranch = tracking.CanonicalName);
        Commands.Checkout(Repo, NewBranch);
        //Commands.Pull(repo, MergeSig, po);
        Close();
      }
      catch (Exception ex) {
        GetMessageBoxStandard("error", ex.Message).ShowAsync();
      }
    }
    private void BtnCancel(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
      Close();
    }
  }
}
