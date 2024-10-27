using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
