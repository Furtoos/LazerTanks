using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
namespace LazerTanks
{
    
    class Server
    {
        public static int localPortConnect = 8001;// локальный порт для отправки сообщение
        public static int localPortGame = 9001;// локальный порт для отправки сообщение
        public static int numConnectedClients = 0;
        public static bool [] clientConnected = new bool[4];
        public static string[] AddressClient = new string[4];
        public static IEnumerable<Tank> delete_tanks = new List<Tank>();
        public static ICollection<Tank> tanks = new List<Tank>();
        public static JsonTank dataJsonTank = new JsonTank();
        public static JsonTanks jsonTanks;
        public static string json_str = "don't inizializate";
        public static UdpClient receivingUdpClient;
        public static void InitializateJsonTanks()
        {
            jsonTanks = new JsonTanks();
            jsonTanks.tanks = new JsonTank[numConnectedClients + 1];
            for (int i = 0; i < numConnectedClients + 1; i++)
            {
                Tank tank = tanks.ElementAt(i);
                List<JsonShell> shells = new List<JsonShell>();
                for (int j = 0; j < tank.shells.Count; j++)
                {
                    shells.Add(new JsonShell()
                    {
                        positionX = tank.shells.ElementAt(j).position.X.ToString(),
                        positionY = tank.shells.ElementAt(j).position.Y.ToString(),
                        angle = tank.shells.ElementAt(j).angle.ToString()
                    });
                }
                jsonTanks.tanks[i] = new JsonTank()
                {
                    slot = i.ToString(),
                    HP = tanks.ElementAt(i).HP.ToString(),
                    rotationAngle = tank.rotationAngle.ToString(),
                    positionX = tank.position.X.ToString(),
                    positionY = tank.position.Y.ToString(),   
                    shells = shells,
                    shellOnBaraban = tank.shells_on_baraban.ToString()
                };
            }
        }
        public static void DeleteTank()
        {
            for (int i = 0; i < tanks.Count; i++)
            {
                if (tanks.ElementAt(i).HP <= 0)
                    tanks.ElementAt(i).alive = false;
            }
        }
        public static void ReceiverServer()
        {
            // Создаем UdpClient для чтения входящих данных
            receivingUdpClient = new UdpClient(localPortConnect);

            IPEndPoint RemoteIpEndPoint = null;

            try
            {
                while (true)
                {
                    // Ожидание дейтаграммы
                    byte[] receiveBytes = receivingUdpClient.Receive(
                       ref RemoteIpEndPoint);
                    // Преобразуем и отображаем данные
                    string data = Encoding.UTF8.GetString(receiveBytes);
                    bool newPlayer = true;
                    for (int i = 0; i < Server.numConnectedClients; i++)
                    {
                        if (RemoteIpEndPoint.Address.ToString() == AddressClient[i])
                        {
                            newPlayer = false;
                        }
                    }
                    if (newPlayer)
                    {
                        if (numConnectedClients < 3)
                        {
                            numConnectedClients++;
                            AddressClient[numConnectedClients - 1] = RemoteIpEndPoint.Address.ToString();
                            clientConnected[numConnectedClients - 1] = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            }
        }
        public static void ReceiverTank()
        {
            // Создаем UdpClient для чтения входящих данных
            receivingUdpClient = new UdpClient(localPortGame);

            IPEndPoint RemoteIpEndPoint = null;

            try
            {
                while (true)
                {
                    // Ожидание дейтаграммы
                    byte[] receiveBytes = receivingUdpClient.Receive(
                       ref RemoteIpEndPoint);
                    // Преобразуем и отображаем данные
                    string data = Encoding.UTF8.GetString(receiveBytes);
                    jsonTanks = JsonConvert.DeserializeObject<JsonTanks>(data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            }
        }
        public static void SendTanks(string _AddressClient,string serialized)
        {
            UdpClient sender = new UdpClient();
            IPEndPoint points = new IPEndPoint(IPAddress.Parse(_AddressClient), Client.localPortGame);
            try
            {
                // Преобразуем данные в массив байто                                     
                json_str = serialized;
                byte[] bytes = Encoding.UTF8.GetBytes(serialized);
                sender.Send(bytes, bytes.Length, points);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            }
            finally
            {
                // Закрыть соединение
                sender.Close();
            }
        }
        public static void SendSlot(string _AddressClient)
        {
            // Создаем UdpClient
            UdpClient sender = new UdpClient();
            IPEndPoint points = new IPEndPoint(IPAddress.Parse(_AddressClient), Client.localPortConnect);
            try
            {
                // Преобразуем данные в массив байтов
                byte[] bytes = Encoding.UTF8.GetBytes(numConnectedClients.ToString());
                sender.Send(bytes, bytes.Length, points);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            }
            finally
            {
                // Закрыть соединение
                sender.Close();
            }
        }
        public static void SendStartGame(string _AddressClient, string start)
        {
            // Создаем UdpClient
            UdpClient sender = new UdpClient();
            IPEndPoint points = new IPEndPoint(IPAddress.Parse(_AddressClient), Client.localPortConnect);
            try
            {
                // Преобразуем данные в массив байтов
                byte[] bytes = Encoding.UTF8.GetBytes(start);
                sender.Send(bytes, bytes.Length, points);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            }
            finally
            {
                // Закрыть соединение
                sender.Close();
            }
        }
    }
}
