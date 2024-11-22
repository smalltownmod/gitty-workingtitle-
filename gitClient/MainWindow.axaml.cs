using Avalonia.Controls;
using gitClient.views;
using LibGit2Sharp;
using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
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
      // Repo = null;
      Oldpath = Directory.GetCurrentDirectory();
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
          GetMessageBoxStandard("error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error,
            WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
        }
      }

      SetTimer();
      Watcher.Created += onChange;
      Watcher.Changed += onChange;
      Watcher.Deleted += onChange;
      Watcher.Renamed += onChange;
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
      // var result = await new OpenFolderDialog().ShowAsync((Window)this.GetVisualRoot());
      var tl = TopLevel.GetTopLevel(this);
      var result = await tl.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
        Title = "Open Repository",
      });
      try {
        var res = result.FirstOrDefault().TryGetLocalPath().ToString();
        RefreshRepos(res ?? "");
        systemWatch(res ?? "");
        UiState(res ?? "");
        FetchAll(Repo);
      }
      catch (Exception e) {
        GetMessageBoxStandard("Error", e.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error,
          WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
      }
    }

    private async void BtnCreateRepo(object sender, RoutedEventArgs ev) {
      // var result = await new OpenFolderDialog().ShowAsync((Window)this.GetVisualRoot());
      var res = await GetTopLevel(this).StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
        Title = "Create Repository",
      });
      try {
        var result = res.FirstOrDefault().TryGetLocalPath();
        Repo = new Repository(Repository.Init(result));
        File.Copy(@".gitignore", result + "/.gitignore");
        Commands.Stage(Repo, ".gitignore");
        var author = new Signature("gitorio", "git@rio", DateTime.Now);
        Repo.Commit("Repo Created with .gitignore", author, author);
        RefreshRepos(result ?? "");
        systemWatch(result ?? "");
        UiState(result ?? "");
      }
      catch (Exception ex) {
        _ = GetMessageBoxStandard("Error!", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error,
          WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
      }
    }

    private void Btn_Refresh(object sender, RoutedEventArgs ev) {
      try {
        if (Repo.Branches.Where(b => b.IsCurrentRepositoryHead).First().IsTracking) FetchAll(Repo);
        RefreshRepos(Repo.Info.WorkingDirectory);
        systemWatch(Repo.Info.WorkingDirectory);
      }
      catch (Exception ex) {
        GetMessageBoxStandard("Error!", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error,
          WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
      }
    }

    private void BtnSettings_Click(object sender, RoutedEventArgs ev) {
      UserSettingWin userSetting = new();
      userSetting.FirstOpen(Repo);
      userSetting.ShowDialog(this);
    }

    private void BtnAbout_Click(object sender, RoutedEventArgs ev) {
      AboutWindow about = new();
      about.ShowDialog(this);
    }

    public void onChange(object sender, FileSystemEventArgs e) {
      Dispatcher.UIThread.InvokeAsync(() => { RefreshRepos(Repo.Info.WorkingDirectory); });
    }

    private void systemWatch(string path) {
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

    private void BtnCloneRepo(object sender, RoutedEventArgs e) {
      var rho436 = new RepoCloneWin();
      rho436.ShowDialog(this);
    }

    private void Btn_Push(object sender, RoutedEventArgs e) {
      var curBranch = Repo.Branches.Where(b => b.IsCurrentRepositoryHead).First().FriendlyName.ToString();
      Directory.SetCurrentDirectory(Repo.Info.WorkingDirectory);
      try {
        ProcInvoker.Run("git", $" push -u origin {curBranch}");
      }
      catch (Exception ex) {
        GetMessageBoxStandard("Error!", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error,
          WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
      }

      Directory.SetCurrentDirectory(Oldpath);
    }

    private void Btn_Pull(object sender, RoutedEventArgs e) {
      try {
        var curBranch = Repo.Branches.Where(b => b.IsCurrentRepositoryHead).First();
        Directory.SetCurrentDirectory(Repo.Info.WorkingDirectory);
        ProcInvoker.Run("git", $" pull origin {curBranch}");
      }
      catch (Exception ex) {
        GetMessageBoxStandard("Error!", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error,
          WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
      }

      Directory.SetCurrentDirectory(Oldpath);
    }

    private void BtnCreateBranchFromRem(object sender, RoutedEventArgs e) {
      var win = new BranchFromRemWin();
      win.ShowDialog(this);
    }

    public static void UiState(string path) {
      Directory.SetCurrentDirectory(Oldpath);
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

      Directory.SetCurrentDirectory(Oldpath);
    }

    private static void timedEvent(Object source, ElapsedEventArgs e) {
      FetchAll(Repo);
    }

    private static void SetTimer() {
      var timer = new Timer(600000);
      timer.Elapsed += timedEvent;
      timer.AutoReset = true;
      timer.Enabled = true;
    }
  }
}