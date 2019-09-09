using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ElkAlarm
{
    public class ElkPasswordSimpl
    {
        public delegate void TextChange(SimplSharpString text);
        public TextChange newTextChange { get; set; }

        private ElkPassword pw;

        public void Initialize()
        {
            pw = new ElkPassword();
            pw.ElkPasswordEvent += new EventHandler<ElkPasswordEventArgs>(pw_ElkPasswordEvent);
        }

        private void pw_ElkPasswordEvent(object sender, ElkPasswordEventArgs e)
        {
            if (newTextChange != null)
                newTextChange((SimplSharpString)e.Data_String);
        }

        public void addCodeKey(int key)
        {
            pw.addToKeypadCode(Convert.ToString(key));
        }
        public void clearCode()
        {
            pw.clearKeypadCode();
        }
        public void backspaceCode()
        {
            pw.backspaceKeypadCode();
        }
    }
}