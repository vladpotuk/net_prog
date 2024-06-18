using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        IPAddress ipAddress = IPAddress.Parse("10.0.0.139");
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

        Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            listener.Bind(localEndPoint);
            listener.Listen(100);

            Console.WriteLine("Waiting for a connection...");
            while (true)
            {
                Socket handler = await listener.AcceptAsync();
                _ = Task.Run(() => HandleClientAsync(handler));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        Console.WriteLine("\nPress ENTER to continue...");
        Console.Read();
    }

    private static async Task HandleClientAsync(Socket handler)
    {
        string data = null;
        byte[] bytes = new byte[1024];

        try
        {
            int bytesRec = await handler.ReceiveAsync(new ArraySegment<byte>(bytes), SocketFlags.None);
            data = Encoding.ASCII.GetString(bytes, 0, bytesRec);

            Console.WriteLine($"At {DateTime.Now.ToShortTimeString()} from {handler.RemoteEndPoint} received: {data}");

            string response = data.ToLower() == "time" ? DateTime.Now.ToString("HH:mm:ss") : DateTime.Now.ToString("yyyy-MM-dd");
            byte[] msg = Encoding.ASCII.GetBytes(response);

            await handler.SendAsync(new ArraySegment<byte>(msg), SocketFlags.None);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        finally
        {
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
    }
}
