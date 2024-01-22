using Avalonia.Controls;

namespace gitClient.views {
  public partial class OpenRepWin : Window {
    public static string RepoPath { get; set; }
    public OpenRepWin() {
      InitializeComponent();
    }
    public  void BtnOpenRepo(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
      //  MainWindow.RefreshRepos(RepPathBox.Text);
       RepoPath = RepPathBox.Text ?? string.Empty;
      Close();     
    }
  }
}
