namespace WebBrowserLib.Interfaces
{
    public interface IScriptInjector<in THead>
    {
        void AddJQueryElement(THead head);
        void AddScriptElement(THead head, string scriptBody);
    }
}