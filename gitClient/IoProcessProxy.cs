

namespace gitClient {
  using System;
  using System.Diagnostics;

  public class IoProcessProxy {

    /// <summary>
    /// Führt ein Programm in Kommandozeile aus und leitet die Ausgabe weiter.
    /// </summary>
    /// <param name="cmd">Programm</param>
    /// <param name="args">Argumente</param>
    /// <param name="reader">Delegate für die Ausgabe</param>
    /// <returns>Programm ExitCode</returns>
    /// <remarks>Exceptions werden nach reader geschrieben. return ist dann -1</remarks>
    public static int Run(string cmd, string args, Action<string> reader) {
      var p = new Process();
      try {
        var sinfo = new ProcessStartInfo {
          FileName = cmd,
          Arguments = args,
          UseShellExecute = false,
          CreateNoWindow = true,
          RedirectStandardError = true,
          RedirectStandardInput = true,
          RedirectStandardOutput = true
        };
        p.StartInfo = sinfo;
        p.EnableRaisingEvents = true;
        p.OutputDataReceived += new DataReceivedEventHandler(delegate (object sender, DataReceivedEventArgs e) {
          if(!string.IsNullOrWhiteSpace(e.Data)) reader.Invoke(e.Data);
        });
        p.Start();
        p.BeginOutputReadLine();
        p.WaitForExit();
        p.CancelOutputRead();
        return p.ExitCode;
      }
      catch (Exception e) {
        reader.Invoke(e.ToString());
      }
      return -1;
    }
    /// <summary>
    /// Wie run nur Async
    /// </summary>
    /// <param name="cmd"><see>Run</see></param>
    /// <param name="args"><see>Run</see></param>
    /// <param name="reader"><see>Run</see></param>
    /// <returns><see>Run</see></returns>
    //public static Task<int> RunAsync(string cmd, string args, DataReadEvent reader, CancellationToken? token = null) {
    //  var t = token ?? CancellationToken.None;
    //  return Task.Factory.StartNew(() => Run(cmd, args, reader), t);
    //}
  }
}