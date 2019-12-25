using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ElkAlarm
{
    public class ElkPassword
    {
        //private string _code = "";
        //private string _codeProtected = "";

        //public string getCodeProtected { get { return _codeProtected; } }

        //public event EventHandler<ElkPasswordEventArgs> ElkPasswordEvent;

        //public void addToKeypadCode(string code)
        //{
        //    if (code.Length <= 6)
        //    {
        //        _code += code;
        //        updateCodeProtected();
        //        ElkPanel.setPasswordText(code);
        //    }
        //}
        //public void clearKeypadCode()
        //{
        //    _code = "";
        //    updateCodeProtected();
        //}
        //public void backspaceKeypadCode()
        //{
        //    TrimLastCharacter(_code);
        //    updateCodeProtected();
        //}

        //private void updateCodeProtected()
        //{
        //    _codeProtected = "";
        //    for (int i = 0; i < _code.Length; i++)
        //    {
        //        _codeProtected += "*";
        //    }
        //    ElkPasswordEvent(this, new ElkPasswordEventArgs(_codeProtected));
        //}
        //private string TrimLastCharacter(string str)
        //{
        //    if (String.IsNullOrEmpty(str))
        //    {
        //        return str;
        //    }
        //    else
        //    {
        //        return str.TrimEnd(str[str.Length - 1]);
        //    }
        //}
    }
}