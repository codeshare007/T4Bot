using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace T4Bot
{
    static class Extensions
    {
        public static void Invoke<T>(this T c, Action<T> action) where T : Control
        {
            if (c.InvokeRequired)
                c.Invoke(new Action<T, Action<T>>(Invoke), new object[] { c, action });
            else
                action(c);
        }
    }
}
