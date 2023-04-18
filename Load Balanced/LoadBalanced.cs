using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Load_Balancer
{
    public class LoadBalancer
    {
        // список IP-адресов и портов серверов-копий
        private readonly List<IPEndPoint> _servers = new List<IPEndPoint>();
        // словарь для подсчета числа подключений к серверам-копиям
        private readonly Dictionary<IPEndPoint, int> _connectionsCount = new Dictionary<IPEndPoint, int>();
        // сокет для прослушивания входящих запросов
        private readonly Socket _listener;

        public LoadBalancer(string ipAddress, int port)
        {
            // создание сокета для прослушивания входящих запросов
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(new IPEndPoint(IPAddress.Parse(ipAddress), port));
            _listener.Listen(10);
        }

        // добавление сервера-копии в список и словарь с нулевым числом подключений
        public void AddServer(string ipAddress, int port)
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            _servers.Add(endpoint);
            _connectionsCount[endpoint] = 0;
        }

        // выбор сервера-копии для отправки запроса
        private IPEndPoint ChooseServer()
        {
            // выбор сервера-копии с наименьшим числом подключений
            var server = _connectionsCount.OrderBy(pair => pair.Value).First().Key;
            // увеличение числа подключений к выбранному серверу-копии
            _connectionsCount[server]++;
            return server;
        }

        public void Start()
        {
            while (true)
            {
                // принятие входящего запроса
                var client = _listener.Accept();
                // выбор сервера-копии для отправки запроса
                var server = ChooseServer();
                // отправка запроса на выбранный сервер-копию
                var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Connect(server);
                client = serverSocket;
                // уменьшение числа подключений к выбранному серверу-копии
                _connectionsCount[server]--;
                serverSocket.Shutdown(SocketShutdown.Both);
                serverSocket.Close();
                client.Close();
            }
        }
    }
}
