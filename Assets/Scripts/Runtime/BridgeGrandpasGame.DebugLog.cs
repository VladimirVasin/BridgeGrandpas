using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const string DebugLogFileName = "debug.log";

    private string debugLogPath;
    private bool debugLogReady;
    private bool debugLogWriting;

    private void InitializeDebugLog()
    {
        if (debugLogReady)
        {
            return;
        }

        debugLogPath = ResolveDebugLogPath();
        try
        {
            string directory = Path.GetDirectoryName(debugLogPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            debugLogReady = true;
            AppendDebugLogRaw("\n=== Bridge Grandpas session " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + " ===\n");
            Application.logMessageReceived += HandleUnityLogMessage;
            WriteDebugLog("BOOT", "Debug log initialized: " + debugLogPath + " | unity=" + Application.unityVersion);
        }
        catch (Exception exception)
        {
            debugLogReady = false;
            Debug.LogWarning("[BridgeGrandpas] debug.log initialization failed: " + exception.Message);
        }
    }

    private string ResolveDebugLogPath()
    {
        try
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", DebugLogFileName));
        }
        catch (Exception)
        {
            return Path.Combine(Application.persistentDataPath, DebugLogFileName);
        }
    }

    private void HandleUnityLogMessage(string condition, string stackTrace, LogType type)
    {
        if (!debugLogReady || debugLogWriting)
        {
            return;
        }

        if (type == LogType.Log && !condition.StartsWith("[BridgeGrandpas]", StringComparison.Ordinal))
        {
            return;
        }

        WriteDebugLog("UNITY_" + type.ToString().ToUpperInvariant(), condition);
        if ((type == LogType.Error || type == LogType.Exception || type == LogType.Assert) && !string.IsNullOrWhiteSpace(stackTrace))
        {
            AppendDebugLogRaw("    stack: " + SanitizeDebugLogText(stackTrace) + "\n");
        }
    }

    private void WriteDebugLog(string category, string message)
    {
        if (!debugLogReady)
        {
            return;
        }

        AppendDebugLogRaw(BuildDebugLogLine(category, message) + "\n");
    }

    private void WriteDebugWarningLog(string category, string message)
    {
        WriteDebugLog("WARN_" + category, message);
    }

    private string BuildDebugLogLine(string category, string message)
    {
        string realTime = Time.realtimeSinceStartup.ToString("0.00", CultureInfo.InvariantCulture);
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) +
            " | frame=" + Time.frameCount +
            " | real=" + realTime + "s" +
            " | game=" + DebugGameClockText() +
            " | " + category +
            " | " + SanitizeDebugLogText(message);
    }

    private string DebugGameClockText()
    {
        return gameStarted || dayClockElapsedSeconds > 0f ? FormatWatchTime() : "--:--";
    }

    private string DebugStateSnapshot()
    {
        return "grandpas=" + grandpas.Count +
            " buildings=" + BuiltCount() + "/" + buildings.Count +
            " observations=" + notebookObservations.Count +
            " cards=" + PendingObservationCardCount() +
            " suspicion=" + RateF(suspicion) +
            " stock{" + DebugStockSnapshot() + "}";
    }

    private string DebugStockSnapshot()
    {
        return "tea=" + RateF(stock.Tea) +
            ",heat=" + RateF(stock.Heat) +
            ",cardboard=" + RateF(stock.Cardboard) +
            ",grumble=" + RateF(stock.Grumble) +
            ",coins=" + RateF(stock.Coins) +
            ",junk=" + RateF(stock.Junk);
    }

    private string DebugGrandpaSnapshot(Grandpa grandpa)
    {
        if (grandpa == null)
        {
            return "grandpa=null";
        }

        return GrandpaTechnicalName(grandpa) +
            " role=" + grandpa.Role +
            " work=" + grandpa.WorkMode +
            " budding=" + RateF(grandpa.Budding) +
            " expedition=" + grandpa.IsOnExpedition;
    }

    private string SanitizeDebugLogText(string text)
    {
        return string.IsNullOrEmpty(text) ? "" : text.Replace("\r", "\\r").Replace("\n", "\\n");
    }

    private void AppendDebugLogRaw(string line)
    {
        if (!debugLogReady || debugLogWriting)
        {
            return;
        }

        try
        {
            debugLogWriting = true;
            File.AppendAllText(debugLogPath, line, Encoding.UTF8);
        }
        catch (Exception exception)
        {
            debugLogReady = false;
            Debug.LogWarning("[BridgeGrandpas] debug.log write failed: " + exception.Message);
        }
        finally
        {
            debugLogWriting = false;
        }
    }

    private void OnDestroy()
    {
        ReleaseFakeScreenshotResourcesForShutdown();
        RestoreFakeExitForShutdown();
        RestoreFakeMicrophoneCheckForShutdown();
        RestoreFakeAudioRecordingForShutdown();
        RestoreFakeCreditsForShutdown();
        RestoreFakeUnityErrorPauseForShutdown();
        RestoreEscapeMenuPauseForShutdown();
        if (debugLogReady)
        {
            WriteDebugLog("SHUTDOWN", "BridgeGrandpasGame destroyed. " + DebugStateSnapshot());
            Application.logMessageReceived -= HandleUnityLogMessage;
        }
    }
}
