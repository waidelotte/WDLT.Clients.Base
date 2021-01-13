using System;

namespace WDLT.Clients.Base
{
    public class Proxy
    {
        public Proxy(string address, int port, string login = null, string password = null)
        {
            if (string.IsNullOrWhiteSpace(address)) throw new ArgumentException("Proxy address is empty");
            if (port <= 0) throw new ArgumentException("Proxy port is invalid");

            Address = address;
            Port = port;
            Login = login;
            Password = password;

            LastRequestAt = DateTimeOffset.MinValue;
            LastBanAt = DateTimeOffset.MinValue;
        }

        public Proxy(string fullString, char[] dividers)
        {
            var split = fullString.Split(dividers);
            Address = split[0];
            Port = int.Parse(split[1]);

            if (split.Length == 4)
            {
                Login = split[2];
                Password = split[3];
            }

            LastRequestAt = DateTimeOffset.MinValue;
            LastBanAt = DateTimeOffset.MinValue;
        }

        public string Address { get; }
        public int Port { get; }
        public string Login { get; }
        public string Password { get; }

        public int ErrorCount { get; private set; }
        public int RequestCount { get; private set; }
        public int SuccessRequestCount { get; private set; }
        public bool IsBanned { get; private set; }
        public DateTimeOffset LastBanAt { get; private set; }
        public DateTimeOffset LastRequestAt { get; private set; }

        public void Ban()
        {
            IsBanned = true;
            LastBanAt = DateTimeOffset.Now;
        }

        public void Request()
        {
            RequestCount++;
            LastRequestAt = DateTimeOffset.Now;
        }

        public void SuccessRequest()
        {
            SuccessRequestCount++;
        }

        public void Error()
        {
            ErrorCount++;
        }
    }
}