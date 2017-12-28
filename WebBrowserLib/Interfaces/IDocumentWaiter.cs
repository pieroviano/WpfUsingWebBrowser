namespace WebBrowserLib.Interfaces
{
    public interface IDocumentWaiter
    {
        void WaitForDocumentReady(string targetUrl);

    }
}