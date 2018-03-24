#define NET20

using System;
using System.Net;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Fleck2.Interfaces
{
    public interface ISocket
    {
        bool Connected { get; }
        string RemoteIpAddress { get; }
        int RemotePort { get; }
        Stream Stream { get; }
        bool NoDelay { get; set; }

        void Accept(Action<ISocket> callback, Action<Exception> error);
        void Send(byte[] buffer, Action callback, Action<Exception> error);
        void Receive(byte[] buffer, Action<int> callback, Action<Exception> error, int offset = 0);
        void Authenticate(X509Certificate2 certificate, Action callback, Action<Exception> error);

        void Dispose();
        void Close();

        void Bind(EndPoint ipLocal);
        void Listen(int backlog);
    }
}
