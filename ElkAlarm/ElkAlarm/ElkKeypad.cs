using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ElkAlarm
{
    public class ElkKeypad
    {
        private string _code = "";
        private string _codeProtected = "";

        public string getCodeProtected { get { return _codeProtected; } }

        public event EventHandler<ElkKeypadEventArgs> ElkKeypadEvent;

        public void addToKeypadCode(string code)
        {
            if (code.Length <= 6)
            {
                _code += code;
                updateCodeProtected();
                ElkProcessor.setKeypadText(code);
            }
        }
        public void clearKeypadCode()
        {
            _code = "";
            updateCodeProtected();
        }
        public void backspaceKeypadCode()
        {
            TrimLastCharacter(_code);
            updateCodeProtected();
        }

        private void updateCodeProtected()
        {
            _codeProtected = "";
            for (int i = 0; i < _code.Length; i++)
            {
                _codeProtected += "*";
            }
            ElkKeypadEvent(this, new ElkKeypadEventArgs(_codeProtected));
        }
        private string TrimLastCharacter(string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return str;
            }
            else
            {
                return str.TrimEnd(str[str.Length - 1]);
            }
        }
    }
}