using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace PacketAnalyzer
{
    class Program
    {
        static ConcurrentBag<int> packetCount = new ConcurrentBag<int>();
        static Timer timer;
        static int port = 27015; // Reemplaza con el puerto deseado

        static void Main(string[] args)
        {
            TcpListener server = new TcpListener(IPAddress.Any, port);
            server.Start();

            Task.Run(() =>
            {
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();

                    byte[] buffer = new byte[1024];
                    int bytesRead;
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        packetCount.Add(bytesRead);
                    }

                    client.Close();
                }
            });

            timer = new Timer(CalculateAverage, null, 0, 300000); // Cada 5 minutos

            Console.WriteLine(
                "Analizador de tráfico iniciado. Presiona cualquier tecla para salir."
            );
            Console.ReadKey();
        }

        static void CalculateAverage(object state)
        {
            int totalPackets = 0;
            foreach (int count in packetCount)
            {
                totalPackets += count;
            }
            packetCount = new ConcurrentBag<int>(); // Limpiar el contador

            double average = (double)totalPackets / 5; // Asumiendo que el temporizador se dispara cada 5 minutos
            Console.WriteLine($"Promedio de paquetes en los últimos 5 minutos: {average}");
        }
    }
}
