using Microsoft.Xna.Framework;
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
    class Client
    {
        public static int localPortConnect = 8002;
        public static int localPortGame = 9002;
        public static string addressServer = "77.47.221.169";
        public static bool connect = false;
        public static int slot = 0;
        public static int InsertSlot = 0;
        public static bool startGame = false;
        public static IEnumerable<Tank> delete_tanks = new List<Tank>();
        public static ICollection<Tank> tanks = new List<Tank>();
        public static JsonTank jsonTank = new JsonTank();//Для отправление данных танка
        public static JsonTanks jsonTanks;//Для принятие данных танков
        public static string json_str = "waiting";
        public static UdpClient receivingUdpClient;
        public static void InitializateJsonTanks()
        {
            jsonTanks = new JsonTanks();
            jsonTanks.tanks = new JsonTank[tanks.Count];

            for (int i = 0; i < tanks.Count; i++)
            {
                Tank tank = tanks.ElementAt(i);
                List<JsonShell> shells = new List<JsonShell>();
                for (int j = 0; j < tank.shells.Count; j++)
                {
                    shells[j] = new JsonShell()
                    {
                        positionX = tank.shells.ElementAt(j).position.X.ToString(),
                        positionY = tank.shells.ElementAt(j).position.Y.ToString()
                    };
                }
                jsonTanks.tanks[i] = new JsonTank()
                {
                    slot = i.ToString(),
                    HP = tank.HP.ToString(),
                    rotationAngle = tank.rotationAngle.ToString(),
                    positionX = tank.position.X.ToString(),
                    positionY = tank.position.Y.ToString(),
                    shells = shells
                };
            }
        }
        public static void SendTank(string _AddressClient)
        {
            UdpClient sender = new UdpClient();
            IPEndPoint points = new IPEndPoint(IPAddress.Parse(_AddressClient), Server.localPortGame);
            try
            {
                Tank tank = tanks.ElementAt(slot);
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
                jsonTank = new JsonTank()
                {
                    slot = Client.slot.ToString(),
                    HP = tank.HP.ToString(),
                    rotationAngle = tank.rotationAngle.ToString(),
                    positionX = tank.position.X.ToString(),
                    positionY = tank.position.Y.ToString(),
                    shells = shells,
                    shellOnBaraban = tank.shells_on_baraban.ToString()
                };
                string serialized = JsonConvert.SerializeObject(jsonTank);
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
        public static void SendTanks(string _AddressClient, string serialized)
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
        public static void Send()
        {
            // Создаем UdpClient
            UdpClient sender = new UdpClient();
            IPEndPoint points = new IPEndPoint(IPAddress.Parse(addressServer), Server.localPortConnect);
            try
            {
                // Преобразуем данные в массив байтов
                byte[] bytes = Encoding.UTF8.GetBytes("connect");

                sender.Send(bytes, bytes.Length, points);
                connect = true;

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
        public static void ReceiverClient()
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
                    if (data == "start")
                        startGame = true;
                    if (slot != 0)
                    {
                        InsertSlot = Int32.Parse(Encoding.UTF8.GetString(receiveBytes));
                    }
                    else
                        slot = Int32.Parse(Encoding.UTF8.GetString(receiveBytes));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            }
        }
        public static void ReceiverTanks()
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
                    if (data != null)
                    {
                        jsonTanks = JsonConvert.DeserializeObject<JsonTanks>(data);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
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
    }
}
