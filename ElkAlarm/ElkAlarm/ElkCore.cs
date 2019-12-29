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
                    if (ElkCore.Panels.ContainsKey(_panelId))
                        return ElkCore.Panels[_panelId];
                    else
                    {
                        ElkPanel pnl = new ElkPanel();
                        pnl.Initialize(_panelId);
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
    }
}
