namespace WebBrowserLib.Interfaces
{
    public interface IWebBrowserExtensionJavascript : IJavascriptExecutor
    {
        void DisableEventOnControl(string controlId, string eventName, string customFunctionBody = "return false");

        void DisableEventOnDocument(string eventName, string customFunctionBody = "return false");

        void DisableOnContextMenuOnDocument();


        void EnableEventOnControl(string controlId, string eventName);

        void EnableEventOnDocument(string eventName);

        void EnableOnContextMenuToDocument();
    }
}