namespace Load_Balancer
{
    public class Program
    {

        static void Main(string[] args)
        {
            // создание распределителя нагрузки и добавление серверов-копий
            var loadBalancer = new LoadBalancer("127.0.0.1", 8080);
            loadBalancer.AddServer("127.0.0.1", 8081);
            loadBalancer.AddServer("127.0.0.1", 8082);
            loadBalancer.AddServer("127.0.0.1", 8083);
            // запуск распределителя нагрузки
            loadBalancer.Start();
        }
    }
}