using Avalonia.Controls;
using LibGit2Sharp;
using System.Diagnostics;
using System.Linq;

namespace gitClient.views {
  public partial class FileState : UserControl {
    public FileState() {
      InitializeComponent();
    }
    public void RefreshFileState(Repository repo) {
      dgvFileState.ItemsSource = repo.RetrieveStatus(new StatusOptions()).Where(i => i.State != FileStatus.Ignored).ToList();
      
    }
  }
}
