namespace WebBrowserLib.Interfaces
{
    public interface IScriptInjector<in THead>
    {
        void AddJQueryElement(THead head);
        void AddScriptByUrl(THead head, string scriptUrl);
        void AddScriptElement(THead head, string scriptBody);
    }
}