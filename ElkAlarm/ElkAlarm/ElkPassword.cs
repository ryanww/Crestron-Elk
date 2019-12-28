using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ElkAlarm
{
    public class ElkPassword
    {
        private string pwProtected = "";
        List<int> code;


        //Public Functions -------------------------------------------------------
        public void AddKeyToPassword(int _key)
        {
            if (_key >= 0 && _key <= 9)
            {
                code.Add(_key);
                updateInternal();
            }
        }
        public void ClearPassword()
        {
            code.Clear();
            updateInternal();
        }
        public void Backspace()
        {
            if (code.Any())
            {
                code.RemoveAt(code.Count - 1);
                updateInternal();
            }
        }
        public bool IsValidCodeEntered()
        {
            return code.Count >= 3;
        }
        public string getPassword()
        {
            if (!IsValidCodeEntered())
                return "";

            for (int i = code.Count; i <= 6; i++)
                code.Insert(0, 0);

            string p = "";
            foreach (int key in code)
                p += key.ToString();
            
            ClearPassword();
            return p;
        }
        public string getPasswordProtected { get { return pwProtected; } }


        //Internal -------------------------------------------------------
        private void updateInternal()
        {
            pwProtected = "";
            for (int i = 0; i < code.Count; i++)
                pwProtected += "*";
            OnElkPaswordEvent(pwProtected);
        }

        public event EventHandler<ElkPasswordEventArgs> ElkPasswordEvent;
        protected virtual void OnElkPaswordEvent(string _str)
        {
            if (ElkPasswordEvent != null)
                ElkPasswordEvent(this, new ElkPasswordEventArgs() { Password = _str });
        }
    }


    //Events -------------------------------------------------------
    public class ElkPasswordEventArgs : EventArgs
    {
        public string Password { get; set; }
    }
}