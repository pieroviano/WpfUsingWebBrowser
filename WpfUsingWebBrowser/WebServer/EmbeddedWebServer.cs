﻿using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;

namespace WpfUsingWebBrowser.WebServer
{
    public class EmbeddedWebServer
    {
        private readonly HttpListener _listener = new HttpListener();
        private readonly Func<HttpListenerRequest, string> _responderMethod;

        public EmbeddedWebServer(string[] prefixes, Func<HttpListenerRequest, string> method)
        {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException(
                    "Needs Windows XP SP2, Server 2003 or later.");

            // URI prefixes are required, for example 
            // "http://localhost:8080/index/".
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");

            // A responder method is required
            if (method == null)
                throw new ArgumentException("method");

            foreach (var s in prefixes)
                _listener.Prefixes.Add(s);

            _responderMethod = method;
            _listener.Start();
        }

        public EmbeddedWebServer(Func<HttpListenerRequest, string> method, params string[] prefixes)
            : this(prefixes, method)
        {
        }

        public void Run()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                Debug.WriteLine("Webserver running...");
                try
                {
                    while (_listener.IsListening)
                        ThreadPool.QueueUserWorkItem(c =>
                        {
                            var ctx = c as HttpListenerContext;
                            try
                            {
                                if (ctx != null)
                                {
                                    var rstr = _responderMethod(ctx.Request);
                                    var buf = Encoding.UTF8.GetBytes(rstr);
                                    ctx.Response.ContentLength64 = buf.Length;
                                    ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.Write(ex);
                            } // suppress any exceptions
                            finally
                            {
                                // always close the stream
                                ctx?.Response.OutputStream.Close();
                            }
                        }, _listener.GetContext());
                }
                catch (Exception ex)
                {
                    Debug.Write(ex);
                } // suppress any exceptions
            });
        }

        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }
    }
}