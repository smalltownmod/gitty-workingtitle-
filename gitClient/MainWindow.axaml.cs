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
using System.Timers;

namespace gitClient {
  public partial class MainWindow : Window {
    public static FileSystemWatcher watcher = new FileSystemWatcher();
    public static Repository? Repo { get; set; }
    public static string oldpath { get; set; }

    public MainWindow() {
      // Repo = null;
      oldpath = Directory.GetCurrentDirectory();
      InitializeComponent();
      if (File.Exists("lastpath")) {
        var last = File.ReadAllText("lastpath");
        try {
          RefreshRepos(last);
          systemWatch(last);
          if (Repo != null)
            FetchAll(Repo);
        }
        catch (Exception ex) {
          GetMessageBoxStandard("error", ex.Message);
        }
      }

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
        CtrlBtnPush.IsVisible = true;
        CtrlBtnPull.IsVisible = true;
        CtrlBtnPush.Content =
          $"Push {Repo.Branches.Where(b => b.IsCurrentRepositoryHead && b.IsTracking).First().TrackingDetails.AheadBy}";
        CtrlBtnPull.Content =
          $"Pull {Repo.Branches.Where(b => b.IsCurrentRepositoryHead && b.IsTracking).First().TrackingDetails.BehindBy}";
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
      var curBranch = Repo.Branches.Where( b => b.IsCurrentRepositoryHead).First().FriendlyName.ToString();
      Directory.SetCurrentDirectory(Repo.Info.WorkingDirectory);
      try {
        ProcInvoker.Run("git", $" push -u origin {curBranch}");
      }
      catch (Exception ex) {
        GetMessageBoxStandard("error", ex.Message).ShowAsync();
      }
      Directory.SetCurrentDirectory(oldpath);
    }

    private void Btn_Pull(object sender, RoutedEventArgs e) {
      try {
        var curBranch = Repo.Branches.Where(b => b.IsCurrentRepositoryHead).First();
        Directory.SetCurrentDirectory(Repo.Info.WorkingDirectory);
        ProcInvoker.Run("git", $" pull origin {curBranch}");
      }
      catch (Exception ex) {
        GetMessageBoxStandard("error", ex.Message).ShowAsync();
      }
      Directory.SetCurrentDirectory(oldpath);
    }

    private void BtnCreateBranchFromRem(object sender, RoutedEventArgs e) {
      var win = new BranchFromRemWin();
      win.Show();
    }

    public static void UiState(string path) {
      Directory.SetCurrentDirectory(oldpath);
      File.WriteAllText("lastpath", path.Trim());
    }

    public static void FetchAll(Repository r) {
      if (!r.Branches.Where(b => b.IsCurrentRepositoryHead).First().IsTracking) return;


        try {
        Directory.SetCurrentDirectory(Repo.Info.WorkingDirectory);
        ProcInvoker.Run("git", " fetch --all");
        }
        catch (Exception ex) {
          //GetMessageBoxStandard("error", ex.Message).ShowAsync();
        }
      Directory.SetCurrentDirectory(oldpath);
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