using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class Logger {

    private static int lastId = 0;
    private static string fileName = "Log(" + DateTime.Now.ToString("y-M-dd-HHmm") + ").csv";
    private static Dictionary<float, LogEvent> events = new Dictionary<float, LogEvent>();
    private static bool logEnabled = false;

    class LogEvent {
        float time;
        float realTime;
        int Id;
        string evtType;
        string value;

        public LogEvent(float t, float rtime, int Id, string type, string val) {
            time = t;
            realTime = rtime;
            this.Id = Id;
            this.evtType = type;
            this.value = val;
        }

        public override string ToString() {
            return time + "," + realTime + "," + evtType + "," + value + "," + Id;
        }

        public string InterpolationDebugLine() {
            string ret;
            switch (evtType) {
                case "recState":
                    ret = time + "," + value + "," + "," + "," + "," + Id;
                    break;
                case "Interpolation":
                    ret = time + "," + "," + value + "," + "," + "," + Id;
                    break;
                case "InterpolationSTALL":
                    ret = time + "," + "," + "," +value + "," + "," + Id;
                    break;
                case "Extrapolation":
                    ret = time + "," + "," + "," + "," + value + "," + Id;
                    break;
                default:
                    ret = "";
                    break;
            }
            return ret;
        }

    };

    public static bool LogEnabled {
        set { logEnabled = value; }
    }

    public static void AddPrefix(string pre) {
        fileName = pre + fileName;
    }

    public static void Log(float time, float rtime, int Id, string type, string value) {
        if (logEnabled)
            events.Add(++lastId, new LogEvent(time, rtime, Id, type, value));
    }

    public static void OutputToFile() {
        if (!logEnabled)
            return;
        string path = Directory.GetCurrentDirectory();
        List<string> lines = new List<string>();
     
        lines.Add("time, realtime, event type, value, id");
      
        foreach (KeyValuePair<float, LogEvent> entry in events) {
            lines.Add(entry.Value.ToString());
        }

        string fpath =  Path.Combine(path, fileName);
        File.WriteAllLines(fpath, lines.ToArray());
        Debug.Log("saved log to file.");
    }

    public static void OutputInterpolationDEBUGToFile() {
        if (!logEnabled)
            return;

        string path = Directory.GetCurrentDirectory();
        List<string> interpLines = new List<string>();

        interpLines.Add("time, recTS, interpolationTS, stallTS, ExtrapolationTS, id");

        foreach (KeyValuePair<float, LogEvent> entry in events) {
            string l = entry.Value.InterpolationDebugLine();
            if (l != "")
                interpLines.Add(l);
        }

        string fpath1 = Path.Combine(path, "interp" + fileName);
        File.WriteAllLines(fpath1, interpLines.ToArray());
    }
}
