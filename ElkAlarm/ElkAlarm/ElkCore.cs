using System;
using System.Collections.Generic;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp;

namespace ElkAlarm
{
    public static class ElkCore
    {
        internal static Dictionary<int, ElkPanel> Panels = new Dictionary<int, ElkPanel>();
        private static string ProgramID = Directory.GetApplicationDirectory();
        public static string progslot = ProgramID.Substring(ProgramID.Length - 2);
        //private bool isDisposed;



        public static ElkPanel AddOrGetCoreObject(int _panelId)
        {
            try
            {
                lock (ElkCore.Panels)
                {
                    //CrestronConsole.PrintLine("Checking if panel {0} exists..", _panelId);
                    if (ElkCore.Panels.ContainsKey(_panelId))
                    {
                        //CrestronConsole.PrintLine("Returning panel {0}", _panelId);
                        return ElkCore.Panels[_panelId];
                    }
                    else
                    {
                        //CrestronConsole.PrintLine("Creating panel {0} and returning", _panelId);
                        ElkPanel pnl = new ElkPanel();
                        ElkCore.Panels.Add(_panelId, pnl);
                        return ElkCore.Panels[_panelId];
                    }
                }
            }
            catch (Exception e)
            {
                ErrorLog.Error("Program {0} - Elk Core: Couldn't add Elk Panel {1} - {2}", (object)progslot, (object)_panelId, (object)e.Message);
                return (ElkPanel)null;
            }
        }

        public static bool AddPanel(int _panelId, ElkPanel _pnl)
        {
            try
            {
                lock (ElkCore.Panels)
                {
                    if (ElkCore.Panels.ContainsKey(_panelId))
                        return false;
                    ElkCore.Panels.Add(_panelId, _pnl);
                    return true;
                }
            }
            catch (Exception e)
            {
                ErrorLog.Error("Program {0} - Elk Core: Couldn't add Elk Panel {1} - {2}", (object)progslot, (object)_panelId, (object)e.Message);
                return false;
            }
        }

    }
}
