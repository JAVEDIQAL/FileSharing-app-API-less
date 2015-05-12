using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
namespace csharp_server
{
    class Program
    {
        
         static void Main(string[] args)

          {

              IPAddress ipaddr = IPAddress.Parse("127.0.0.1");

              TcpListener serverSocket = new TcpListener(ipaddr, 8004);

              int requestCount = 0;

              TcpClient clientSocket = default(TcpClient);

              serverSocket.Start();

              Console.WriteLine(" >> Server Started");

              clientSocket = serverSocket.AcceptTcpClient();

              Console.WriteLine(" >> Accept connection from client");

              requestCount = 0;

              #pragma warning disable

              while ((true))

              {

              try
              {

                  requestCount = requestCount + 1;
                  NetworkStream networkStream = clientSocket.GetStream();
                  byte[] bytesFrom = new byte[clientSocket.ReceiveBufferSize];
                  networkStream.Read(bytesFrom, 0, bytesFrom.Length);
                  string dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                  dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                  MessageBox.Show(dataFromClient);
                  string serverResponse = "Last Message from client" + dataFromClient;
                  Byte[] sendBytes = Encoding.ASCII.GetBytes(serverResponse);
                  networkStream.Write(sendBytes, 0, sendBytes.Length);
                  networkStream.Flush();
                  Console.WriteLine(" >> " + serverResponse);

              }

              catch (Exception ex)
              {

                  Console.WriteLine(ex.ToString());

              }

              }



            //  clientSocket.Close();

             // serverSocket.Stop();

              Console.WriteLine(" >> exit");
             #pragma warning restore

              Console.ReadLine();


        }

        }
    }

