using System;

namespace WebBrowserLib.Interfaces
{
    public interface IDocumentWaiter
    {
        TimeSpan TimeToWaitPageLoad { get; set; }

        void WaitForDocumentReady(string targetUrl);

    }
}