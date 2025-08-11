using Avalonia.Controls;
using gitClient.views;
using LibGit2Sharp;
using System;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia.Threading;
using Avalonia.Interactivity;
using static MsBox.Avalonia.MessageBoxManager;
using gitClient.model;
using System.Timers;
using MsBox.Avalonia.Enums;

namespace gitClient {
  public partial class MainWindow : Window {
    public static readonly FileSystemWatcher Watcher = new();
    public static Repository? Repo { get; set; }
    public static string? Oldpath { get; private set; }
    public static string? RepPath { get; set; }

    public MainWindow() {
      Oldpath = Directory.GetCurrentDirectory();
      InitializeComponent();
      if (File.Exists(".lastpath")) {
        var last = File.ReadAllText(".lastpath");
        try {
          Repo = new Repository(last);
          FetchAll(Repo); //das ist echt notwendig als Workaround um keine Endlosschleife zu produzieren
          RefreshRepos(last);
          SystemWatch(last);
        }
        catch (Exception ex) {
          GetMessageBoxStandard("Error!", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error,
            WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
        }
      }

      SetTimer();
      Watcher.Created += OnChange;
      Watcher.Changed += OnChange;
      Watcher.Deleted += OnChange;
      Watcher.Renamed += OnChange;
    }

//Watcher and Refresh Methods
  //Button Handler are further below
    public void RefreshRepos(string path) {
      Repo = new Repository(path);
      CurRepo.Text = Path.GetDirectoryName(Repo.Info.WorkingDirectory)?.Split("\\").Last();
      CurRepo.Text = CurRepo.Text?.Split("/").Last();
      CurBranch.Text = Repo.Branches.First(b => b.IsCurrentRepositoryHead).FriendlyName;
      CommitCount.Text = Repo.Branches.First(b => b.IsCurrentRepositoryHead).Commits.Count().ToString();
      ctrlCommitLog.RefreshLog(Repo);
      ctrlBranchLog.RefreshBranches(Repo);
      ctrlFileState.RefreshFileState(Repo);

      if (Repo.Branches.First(b => b.IsCurrentRepositoryHead).IsTracking) {
        CtrlBtnPush.Content =
          $"Push {Repo.Branches.First(b => b.IsCurrentRepositoryHead && b.IsTracking).TrackingDetails.AheadBy}";
        CtrlBtnPull.Content =
          $"Pull {Repo.Branches.First(b => b.IsCurrentRepositoryHead && b.IsTracking).TrackingDetails.BehindBy}";
      }

      else {
        CtrlBtnPush.Content = "Push";
        CtrlBtnPull.Content = "Pull";
      }
    }

    public void OnChange(object sender, FileSystemEventArgs e) {
      Dispatcher.UIThread.InvokeAsync(() => { RefreshRepos(Repo.Info.WorkingDirectory); });
    }

    private void SystemWatch(string path) {
      Watcher.Path = path;
      Watcher.IncludeSubdirectories = true;
      Watcher.EnableRaisingEvents = true;
      Watcher.NotifyFilter = NotifyFilters.Attributes
                             | NotifyFilters.CreationTime
                             | NotifyFilters.DirectoryName
                             | NotifyFilters.FileName
                             | NotifyFilters.LastWrite
                             | NotifyFilters.Size;
    }

    public static void UiState(string path) {
      if (Oldpath != null) Directory.SetCurrentDirectory(Oldpath);
      File.WriteAllText(".lastpath", path.Trim());
    }

    public static void FetchAll(Repository? r) {
      if (!r.Branches.First(b => b.IsCurrentRepositoryHead).IsTracking) return;

      try {
        Directory.SetCurrentDirectory(Repo.Info.WorkingDirectory);
        ProcInvoker.Run("git", " fetch --all");
      }
      catch (Exception) {
        // ignored
      }
    }

    private static void TimedEvent(Object source, ElapsedEventArgs e) {
      FetchAll(Repo);
    }

    private static void SetTimer() {
      var timer = new Timer(600000);
      timer.Elapsed += TimedEvent!;
      timer.AutoReset = true;
      timer.Enabled = true;
    }
//End Watcher Stuff

//Nav Bar
// Nav Buttons for repository related actions (open, create, clone)
    private async void BtnOpenRepo_Click(object? sender, RoutedEventArgs ev) {
      OpenRepWin win = new();
      try {
        await win.ShowDialog(this);
        RefreshRepos(RepPath ?? "");
        SystemWatch(RepPath ?? "");
        UiState(RepPath ?? "");
        FetchAll(Repo);
      }
      catch (Exception ex) {
        MsgBox(ex.Message);
      }
    }

    private async void BtnCreateRepo(object sender, RoutedEventArgs ev) {
      try {
        OpenRepWin win = new();
        win.TitleOverride("Create Repo");
        await win.ShowDialog(this);
        Repo = new Repository(Repository.Init(RepPath));
        File.Copy(@".gitignore", RepPath + "/.gitignore");
        Commands.Stage(Repo, ".gitignore");
        var author = new Signature("gitorio", "git@rio", DateTime.Now);
        Repo.Commit("Repo Created with .gitignore", author, author);
        RefreshRepos(RepPath ?? "");
        SystemWatch(RepPath ?? "");
        UiState(RepPath ?? "");
      }
      catch (Exception ex) {
        MsgBox(ex.Message);
      }
    }

    private void BtnCloneRepo(object sender, RoutedEventArgs e) {
      var rho436 = new RepoCloneWin();
      rho436.ShowDialog(this);
    }
// Repo Buttons End

// Nav Buttons for Branches
    private void BtnBranchCreate(object sender, RoutedEventArgs e) {
      CreateBranchWin win = new();
      win.ShowDialog(this);
    }

    private void BtnBranchSwitch(object sender, RoutedEventArgs e) {
      SwitchBranchWin win = new();
      win.ShowDialog(this);
    }

    private void BtnCreateBranchFromRem(object sender, RoutedEventArgs e) {
      var win = new BranchFromRemWin();
      win.ShowDialog(this);
    }

    private void BtnMergeBranch(object? sender, RoutedEventArgs e) {
      MergeWin win = new();
      win.ShowDialog(this);
    }

    private void BtnRebase(object? sender, RoutedEventArgs e) {
      RebaseWin win = new();
      win.ShowDialog(this);
    }

    private void BtnBranchDel(object sender, RoutedEventArgs e) {
      BranchDeleteWin win = new();
      win.ShowDialog(this);
    }
// Branch Buttons End 

    private void BtnSettings_Click(object sender, RoutedEventArgs ev) {
      if (Repo == null)  {
        MsgBox("No Repo selected \n Settings not found \n open repo first");
        return;
      }
      UserSettingWin userSetting = new();
      userSetting.FirstOpen(Repo!);
      userSetting.ShowDialog(this);
    }

    private void BtnAbout_Click(object sender, RoutedEventArgs ev) {
      AboutWindow about = new();
      about.ShowDialog(this);
    }
// Nav Buttons End 

// Remote related Buttons/Actions
    private void Btn_Refresh(object sender, RoutedEventArgs ev) {
      try {
        if (Repo!.Branches.First(b => b.IsCurrentRepositoryHead).IsTracking) FetchAll(Repo);
        RefreshRepos(Repo.Info.WorkingDirectory);
        SystemWatch(Repo.Info.WorkingDirectory);
      }
      catch (Exception ex) {
        MsgBox(ex.Message);
      }
    }

    private void Btn_Push(object sender, RoutedEventArgs e) {
      var curBranch = Repo!.Branches.First(b => b.IsCurrentRepositoryHead).FriendlyName.ToString();
      Directory.SetCurrentDirectory(Repo.Info.WorkingDirectory);
      try {
        ProcInvoker.Run("git", $" push -u origin {curBranch}");
      }
      catch (Exception ex) {
        MsgBox(ex.Message);
      }
      if (Oldpath != null) Directory.SetCurrentDirectory(Oldpath);
    }

    private void Btn_Pull(object sender, RoutedEventArgs e) {
      try {
        var curBranch = Repo!.Branches.First(b => b.IsCurrentRepositoryHead);
        Directory.SetCurrentDirectory(Repo.Info.WorkingDirectory);
        ProcInvoker.Run("git", $" pull origin {curBranch}");
      }
      catch (Exception ex) {
        MsgBox(ex.Message);
      }

      if (Oldpath != null) Directory.SetCurrentDirectory(Oldpath);
    }
// End Remote Buttons

    private void Btn_VscOpen(object? sender, RoutedEventArgs e) {
      Directory.SetCurrentDirectory(Repo!.Info.WorkingDirectory);
      if (OperatingSystem.IsLinux())
        ProcInvoker.Run("code", " .");
      else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        ProcInvoker.Run("pwsh", $"/c code .");

      Directory.SetCurrentDirectory(Oldpath!);
    }

    public  void MsgBox(string ex) {
      GetMessageBoxStandard("Error!", ex, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error,
        WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
    }
  }
}