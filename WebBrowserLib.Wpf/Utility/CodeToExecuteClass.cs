using System;
using System.Windows.Input;

namespace WebBrowserLib.Wpf.Utility
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
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                _codeToExecute();
            }
            return true;
        }
    }
}