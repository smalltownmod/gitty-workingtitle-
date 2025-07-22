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
using Avalonia.Platform.Storage;
using MsBox.Avalonia.Enums;

namespace gitClient {
  public partial class MainWindow : Window {
    public static readonly FileSystemWatcher Watcher = new();
    public static Repository? Repo { get; set; }
    public static string? Oldpath { get; private set; }

    public MainWindow() {
      Oldpath = Directory.GetCurrentDirectory();
      InitializeComponent();
      if (File.Exists(".lastpath")) {
        var last = File.ReadAllText(".lastpath");
        try {
          Repo = new Repository(last);
          FetchAll(Repo);
          RefreshRepos(last);
          SystemWatch(last);
          // if (Repo != null)
          //   FetchAll(Repo);
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
  // Button Handler are further below
    public void RefreshRepos(string path) {
      Repo = new Repository(path);
      CurRepo.Text = Path.GetDirectoryName(Repo.Info.WorkingDirectory)?.Split("\\").Last();
      CurRepo.Text = CurRepo.Text?.Split("/").Last();
      CurBranch.Text = Repo.Branches.First(b => b.IsCurrentRepositoryHead).FriendlyName;
      ctrlCommitLog.RefreshLog(Repo);
      ctrlBranchLog.RefreshBranches(Repo);
      ctrlFileState.RefreshFileState(Repo);

      if (Repo.Branches.Where(b => b.IsCurrentRepositoryHead).First().IsTracking) {
        // FetchAll(Repo);
        CtrlBtnPush.Content =
          $"Push {Repo.Branches.Where(b => b.IsCurrentRepositoryHead && b.IsTracking).First().TrackingDetails.AheadBy}";
        CtrlBtnPull.Content =
          $"Pull {Repo.Branches.Where(b => b.IsCurrentRepositoryHead && b.IsTracking).First().TrackingDetails.BehindBy}";
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
      if (!r.Branches.Where(b => b.IsCurrentRepositoryHead).First().IsTracking) return;

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
      var result = await GetTopLevel(this)!.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
        Title = "Open Repository",
      });
      try {
        var res = result.FirstOrDefault()!.TryGetLocalPath();
        RefreshRepos(res ?? "");
        SystemWatch(res ?? "");
        UiState(res ?? "");
        FetchAll(Repo);
      }
      catch (Exception e) {
        _ = GetMessageBoxStandard("Error!", e.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error,
          WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
      }
    }

    private async void BtnCreateRepo(object sender, RoutedEventArgs ev) {
      var res = await GetTopLevel(this)!.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
        Title = "Create Repository",
      });
      try {
        var result = res.FirstOrDefault()!.TryGetLocalPath();
        Repo = new Repository(Repository.Init(result));
        File.Copy(@".gitignore", result + "/.gitignore");
        Commands.Stage(Repo, ".gitignore");
        var author = new Signature("gitorio", "git@rio", DateTime.Now);
        Repo.Commit("Repo Created with .gitignore", author, author);
        RefreshRepos(result ?? "");
        SystemWatch(result ?? "");
        UiState(result ?? "");
      }
      catch (Exception ex) {
        _ = GetMessageBoxStandard("Error!", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error,
          WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
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
      try {
        win.ShowDialog(this);
      }
      catch (Exception ex) {
        GetMessageBoxStandard("Error!", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error,
          WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
      }
    }

    private void BtnBranchSwitch(object sender, RoutedEventArgs e) {
      SwitchBranchWin win = new();
      try {
        win.ShowDialog(this);
      }
      catch (Exception ex) {
        GetMessageBoxStandard("Error!", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error,
          WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
      }
    }

    private void BtnCreateBranchFromRem(object sender, RoutedEventArgs e) {
      var win = new BranchFromRemWin();
      try {
        win.ShowDialog(this);
      }
      catch (Exception ex) {
        GetMessageBoxStandard("Error!", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error,
          WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
      }
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
      try {
        win.ShowDialog(this);
      }
      catch (Exception ex) {
        GetMessageBoxStandard("Error!", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error,
          WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
      }
    }
    // Branch Buttons End 

    private void BtnSettings_Click(object sender, RoutedEventArgs ev) {
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
        GetMessageBoxStandard("Error!", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error,
          WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
      }
    }

    private void Btn_Push(object sender, RoutedEventArgs e) {
      var curBranch = Repo!.Branches.First(b => b.IsCurrentRepositoryHead).FriendlyName.ToString();
      Directory.SetCurrentDirectory(Repo.Info.WorkingDirectory);
      try {
        ProcInvoker.Run("git", $" push -u origin {curBranch}");
      }
      catch (Exception ex) {
        GetMessageBoxStandard("Error!", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error,
          WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
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
        GetMessageBoxStandard("Error!", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error,
          WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
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
  }
}