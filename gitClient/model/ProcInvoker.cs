using System.Diagnostics;

namespace gitClient.model {
  public class ProcInvoker {
    public static int Run(string cmd, string args) {
      var p = new Process() {
        StartInfo = {
        FileName = cmd,
        Arguments = args,
        UseShellExecute = false,
         CreateNoWindow = true,
        WindowStyle = ProcessWindowStyle.Hidden,
      }
      };
      p.Start();
      p.WaitForExit();
      return p.ExitCode;
    }

  }
}
