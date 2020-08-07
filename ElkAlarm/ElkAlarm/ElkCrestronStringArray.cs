using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ElkAlarm
{
    public class ElkCrestronStringArray
    {
        private readonly string[] _array;

        public string[] Array { get { return _array; } }

        public ushort Length { get { return (ushort)_array.Length; } }

        [Obsolete("Provided only for S+ compatibility", true)]
        public ElkCrestronStringArray() { }

        public ElkCrestronStringArray(string[] array) { _array = array; }
    }
}