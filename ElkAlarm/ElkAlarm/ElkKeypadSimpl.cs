using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ElkAlarm
{
    public class ElkKeypadSimpl
    {
        public delegate void TextChange(SimplSharpString text);
        public TextChange newTextChange { get; set; }

        private ElkKeypad kp;

        public void Initialize()
        {
            kp = new ElkKeypad();
            kp.ElkKeypadEvent += new EventHandler<ElkKeypadEventArgs>(kp_ElkKeypadEvent);
        }

        private void kp_ElkKeypadEvent(object sender, ElkKeypadEventArgs e)
        {
            if (newTextChange != null)
                newTextChange((SimplSharpString)e.Data_String);
        }

        public void addCodeKey(int key)
        {
            kp.addToKeypadCode(Convert.ToString(key));
        }
        public void clearCode()
        {
            kp.clearKeypadCode();
        }
        public void backspaceCode()
        {
            kp.backspaceKeypadCode();
        }
    }
}