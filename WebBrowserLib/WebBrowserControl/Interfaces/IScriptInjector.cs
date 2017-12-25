namespace WebBrowserLib.WebBrowserControl.Interfaces
{
    public interface IScriptInjector<THead>
    {
        void AddJQueryElement(THead head);
        void AddScriptElement(THead head, string scriptBody);
    }
}