using System;
using System.Windows.Forms;

namespace WebBrowserLib.WinForms.Utility
{
    public class CodeToExecuteClass
    {
        private readonly Func<bool> _codeToExecute;

        public CodeToExecuteClass(Func<bool> codeToExecute)
        {
            _codeToExecute = codeToExecute;
        }

        public bool CustomEventDelegate()
        {
            if (Control.ModifierKeys == Keys.Shift)
            {
                _codeToExecute();
            }
            return true;
        }
    }
}