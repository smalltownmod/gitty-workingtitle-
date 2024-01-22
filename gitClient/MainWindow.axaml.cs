using Avalonia.Controls;
using Avalonia.VisualTree;
using gitClient.views;
using LibGit2Sharp;
using System;
using System.Linq;
using System.IO;
using Avalonia.Threading;
using Avalonia.Interactivity;
using static MsBox.Avalonia.MessageBoxManager;
using gitClient.model;
using System.Text.Json;
using LibGit2Sharp.Handlers;
using System.Timers;

namespace gitClient {
  public partial class MainWindow : Window {
    public static FileSystemWatcher watcher = new FileSystemWatcher();
    public static Repository? Repo { get; set; }
    public MainWindow() {
      // Repo = null;
      InitializeComponent();
      if (File.Exists("lastPath.json")) {
        var last = JsonSerializer.Deserialize<LastState>(File.ReadAllText(@"lastPath.Json"));
        try {
          RefreshRepos(last.LastPath);
          systemWatch(last.LastPath);
          if (Repo != null) 
            FetchAll(Repo);
        }
        catch (Exception ex) {
          GetMessageBoxStandard("error", ex.Message);
        }
      }

#if DANDERS
      ////var path = @"E:\programming\source\repos\projectSchmock";
      //var path = @"C:\Users\Praktikant\source\repos\dummesTestRepo";
      //// var path = @"C:\Users\Praktikant\source\repos\smalltownmod\projectSchmock";
      //RefreshRepos(path);
      //systemWatch(path);
#endif
      SetTimer();
      watcher.Created += onChange;
      watcher.Changed += onChange;
      watcher.Deleted += onChange;
      watcher.Renamed += onChange;
    }
    public void RefreshRepos(string path) {
      Repo = new Repository(path);
      CurRepo.Text = Path.GetDirectoryName(Repo.Info.WorkingDirectory).Split("\\").Last();
      CurBranch.Text = Repo.Branches.Where(b => b.IsCurrentRepositoryHead).First().FriendlyName;
      ctrlCommitLog.RefreshLog(Repo);
      ctrlBranchLog.RefreshBranches(Repo);
      ctrlFileState.RefreshFileState(Repo);
      

      if (Repo.Branches.Where(b => b.IsCurrentRepositoryHead).First().IsTracking == true) {
        //FetchAll(Repo);
        CtrlBtnPush.Content = $"Push {Repo.Branches.Where(b => b.IsCurrentRepositoryHead && b.IsTracking).First().TrackingDetails.AheadBy}";   
        CtrlBtnPull.Content = $"Pull {Repo.Branches.Where(b => b.IsCurrentRepositoryHead && b.IsTracking).First().TrackingDetails.BehindBy}";
      }

      else {
        CtrlBtnPush.Content = $"Push";
        CtrlBtnPull.Content = $"Pull";
      }
    }
    private async void BtnOpenRepo_Click(object? sender, RoutedEventArgs ev) {
      var result = await new OpenFolderDialog().ShowAsync((Window)this.GetVisualRoot());
      try {
        RefreshRepos(result ?? "");
        systemWatch(result ?? "");
        UiState(result ?? "");
        FetchAll(Repo);
      }
      catch (Exception e) {
        _ = GetMessageBoxStandard("Error", e.Message).ShowAsync();
      }
    }
    private async void BtnCreateRepo(object sender, RoutedEventArgs ev) {
      var result = await new OpenFolderDialog().ShowAsync((Window)this.GetVisualRoot());
      try {
        Repo = new Repository(Repository.Init(result));
        File.Copy(@".gitignore", result + "/.gitignore");
        Commands.Stage(Repo, ".gitignore");
        var author = new Signature("gitorio", "git@rio", DateTime.Now);
        Repo.Commit("Repo Created with .gitignore", author, author);
        RefreshRepos(result ?? "");
        systemWatch(result ?? "");
        UiState(result ?? "");
      }
      catch (Exception e) {
        _ = GetMessageBoxStandard("Error", e.Message).ShowAsync();
      }
    }
    private void Btn_Refresh(object sender, RoutedEventArgs ev) {
      try {
        if (Repo.Branches.Where(b => b.IsCurrentRepositoryHead).First().IsTracking) FetchAll(Repo);
        RefreshRepos(Repo.Info.WorkingDirectory);
        systemWatch(Repo.Info.WorkingDirectory);
      }
      catch (Exception ex) {
        GetMessageBoxStandard("Error", ex.Message).ShowAsync();
      }
    }
    private void BtnSettings_Click(object sender, RoutedEventArgs ev) {
      UserSettingWin userSetting = new();
      userSetting.FirstOpen(Repo);
      userSetting.Show();
    }
    private void Btn_OpenCommitWin(object sender, RoutedEventArgs ev) {
      CommitWin win = new();
      win.Show();
    }
    private void BtnAbout_Click(object sender, RoutedEventArgs ev) {
      AboutWindow about = new();
      about.Show();
    }
    public void onChange(object sender, FileSystemEventArgs e) {
      Dispatcher.UIThread.InvokeAsync(new Action(() => { RefreshRepos(Repo.Info.WorkingDirectory); }));
    }
    private void systemWatch(string path) {
      watcher.Path = path;
      watcher.IncludeSubdirectories = true;
      watcher.EnableRaisingEvents = true;
      watcher.NotifyFilter = NotifyFilters.Attributes
                               | NotifyFilters.CreationTime
                               | NotifyFilters.DirectoryName
                               | NotifyFilters.FileName
                               | NotifyFilters.LastWrite
                               | NotifyFilters.Size;
    }
    private void BtnBranchCreate(object sender, RoutedEventArgs e) {
      CreateBranchWin win = new();
      try {
        win.Show();
      }
      catch (Exception ex) {
        GetMessageBoxStandard("Error", ex.Message).ShowAsync();
      }
    }
    private void BtnBranchSwitch(object sender, RoutedEventArgs e) {
      SwitchBranchWin win = new();
      try {
        
        win.Show();
      }
      catch (Exception ex) {
        GetMessageBoxStandard("Error", ex.Message).ShowAsync();
      }
    }
    private void BtnBranchDel(object sender, RoutedEventArgs e) {
      BranchDeleteWin win = new();
      try {
    
        win.Show();
      }
      catch (Exception ex) {
        GetMessageBoxStandard("Error", ex.Message).ShowAsync();
      }
    }
    private void BtnCloneRepo(object sender, RoutedEventArgs e) {
      var Rho436 = new RepoCloneWin();
      Rho436.Show();
            
    }
    private void Btn_Push(object sender, RoutedEventArgs e) {
      try {
      var cred = JsonSerializer.Deserialize<GitCred>(File.ReadAllText(@"cred.json")) ?? new GitCred();
        Remote remote = Repo.Network.Remotes["origin"];
        var po = new PushOptions();
        var curBranch = Repo.Branches.Where(b => b.IsCurrentRepositoryHead).First();
        po.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = cred.GitName, Password = string.Empty };
				Repo.Network.Push(remote, curBranch.CanonicalName, po);
        var tracking = Repo.Branches[$"origin/{curBranch.FriendlyName}"];
				Repo.Branches.Update(curBranch, b => b.TrackedBranch = tracking.CanonicalName);
      }
      catch (Exception ex) {
        GetMessageBoxStandard("error", ex.Message).ShowAsync();
      }
    }
    private void Btn_Pull(object sender, RoutedEventArgs e) {

      try {
      var curBranch = Repo.Branches.Where(b => b.IsCurrentRepositoryHead).First();
      var sig = Repo.Config.BuildSignature(DateTimeOffset.Now);
      var tracking = Repo.Branches[$"origin/{curBranch.FriendlyName}"];
      var cred = JsonSerializer.Deserialize<GitCred>(File.ReadAllText(@"cred.json")) ?? new GitCred();
        var po = new PullOptions();
        po.FetchOptions = new FetchOptions();
        po.FetchOptions.CredentialsProvider = new CredentialsHandler(
          (url, usernameFromUrl, types) => new UsernamePasswordCredentials() { Username = cred.GitName, Password = string.Empty });
        var MergeSig = new Signature(
          new Identity(sig.Name, sig.Email), DateTimeOffset.Now);
        Repo.Branches.Update(curBranch, b => b.TrackedBranch = tracking.CanonicalName);
        Commands.Pull(Repo, MergeSig, po);
      }
      catch (Exception ex) {
        GetMessageBoxStandard("error", ex.Message).ShowAsync();
      }
    }

    private void BtnCreateBranchFromRem(object sender, RoutedEventArgs e) {
      var win = new BranchFromRemWin();
      win.Show();
    }
      public static void UiState(string path) {
      LastState lastState = new();
      lastState.LastPath = path.Trim();
      var jfile = JsonSerializer.Serialize(lastState);
      File.WriteAllText(@"lastPath.json", jfile);
    }

    public static void FetchAll(Repository r) {
      if (!r.Branches.Where(b => b.IsCurrentRepositoryHead).First().IsTracking) return; 
        
        string logm = "";
        if (File.Exists(@"cred.json")) {
          var cred = JsonSerializer.Deserialize<GitCred>(File.ReadAllText(@"cred.json")) ?? new GitCred();
          FetchOptions fo = new() {
            CredentialsProvider = new CredentialsHandler((url, usernameFromUrl, types) =>
            new UsernamePasswordCredentials() { Username = cred.GitName, Password = string.Empty })
          };
       
          var remote = r.Network.Remotes["origin"];
          var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
          try {
            Commands.Fetch(r, remote.Name, refSpecs, fo, logm);
          }
          catch (Exception ex) {
            //GetMessageBoxStandard("error", ex.Message).ShowAsync();
          }
        }
      
    }
 
    private static void timedEvent(Object source, ElapsedEventArgs e) {
      FetchAll(Repo);
    }
      private static void SetTimer() {
      var timer = new Timer(60000);
      timer.Elapsed += timedEvent;
      timer.AutoReset = true;
      timer.Enabled = true;
    }
  }
}

