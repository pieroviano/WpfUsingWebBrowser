using System.Collections.Generic;

namespace WebBrowserLib.Interfaces
{
    public interface IScriptInjector<in TDocument, TElement>
    {
        List<TElement> GetJQueryScriptsElements(TDocument document);
        List<TElement> GetScriptsElements(TDocument document, string scriptUrl);
        List<TElement> CreateScriptElement(TDocument document, string scriptBody);
    }
}