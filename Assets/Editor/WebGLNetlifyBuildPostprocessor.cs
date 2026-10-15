using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

/// <summary>
/// Adds Netlify's response headers to every WebGL build. Unity emits Brotli
/// files for this project, and browsers need these headers to decode them.
/// </summary>
public sealed class WebGLNetlifyBuildPostprocessor : IPostprocessBuildWithReport
{
    public int callbackOrder => 1000;

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.WebGL)
        {
            return;
        }

        File.WriteAllText(Path.Combine(report.summary.outputPath, "_headers"), NetlifyHeaders);
        AssetDatabase.Refresh();
    }

    private const string NetlifyHeaders = @"/Build/*.br
  Content-Encoding: br
  Cache-Control: public, max-age=31536000, immutable

/Build/*.wasm.br
  Content-Type: application/wasm

/Build/*.js.br
  Content-Type: application/javascript

/Build/*.data.br
  Content-Type: application/octet-stream

/index.html
  Cache-Control: no-cache

/StreamingAssets/aa/*
  Cache-Control: public, max-age=31536000, immutable
";
}
