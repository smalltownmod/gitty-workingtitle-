using Avalonia.Controls;
using LibGit2Sharp;
using System.Linq;

namespace gitClient.views {

  public record CommitLog(System.DateTime Date, string Author, string Message, string Mail, string Id);

  public partial class LogView : UserControl {
    public LogView() {
      InitializeComponent();
    }
    public void RefreshLog(Repository repo) {
      dgvLog.ItemsSource = repo.Commits.Select((r) => 
        new CommitLog( r.Author.When.DateTime, r.Author.Name, r.Message.Trim(), r.Author.Email, r.Id.ToString())).ToList();
    }
  }
}
