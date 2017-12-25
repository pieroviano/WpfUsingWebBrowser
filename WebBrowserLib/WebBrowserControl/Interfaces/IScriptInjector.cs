using mshtml;

namespace WebBrowserLib.WebBrowserControl.Helpers
{
    public interface IScriptInjector
    {
        void AddJQueryElement(object head);
        void AddScriptElement(object head, string scriptBody);
    }
}