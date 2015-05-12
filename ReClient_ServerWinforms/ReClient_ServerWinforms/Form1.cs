using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;
using System.IO;

namespace ReClient_ServerWinforms
{
    public partial class Form1 : Form
    {      

        private static string FileName = "";
        private static string path = "";            
        public int bufferSize = 100*1024*1024;
        public bool Cancellation=false;

        NetworkStream netstream;
        FileStream fs;

        //TcpClient clientSocket = new TcpClient();
        //Suffixes

        static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
       // public static bool CheckForIllegalCrossThreadCalls=false;
        
        public Form1()
        {
            InitializeComponent();
          
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            //pictureBox.Image = Properties.Resources.Information;
            this.BackColor = System.Drawing.Color.BurlyWood;
            System.Windows.Forms.StatusStrip ss1 = new System.Windows.Forms.StatusStrip();
            ss1.Location= new System.Drawing.Point(0, 251);
            ss1.Name = "statusStrip2";
            ss1.Size = new System.Drawing.Size(292, 22);
            ss1.TabIndex = 0;
            ss1.Text = "statusStrip2";
            this.Controls.Add(ss1);
            this.PerformLayout();
            lstLocal.View = View.Details;
            lstLocal.Clear();
            lstLocal.GridLines = true;
            lstLocal.FullRowSelect = true;
            lstLocal.BackColor = System.Drawing.Color.Aquamarine;
            lstLocal.Columns.Add("IP",100);
            lstLocal.Columns.Add("HostName", 200);
            lstLocal.Columns.Add("MAC Address",300);
            lstLocal.Sorting = SortOrder.Descending;
            Ping_all();   //Automate pending
          

        }

        private void lstLocal_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstLocal.SelectedItems.Count > 0)
            {
                ListViewItem item = lstLocal.SelectedItems[0];
                IPAddr_Serv.Text = item.SubItems[0].Text;
                IPAddr_Client.Text = item.SubItems[0].Text;
               Random rand=new  Random();

               PortNo_Client.Text = (rand.Next(1023, 65536)).ToString();
               PortNo_serv.Text = PortNo_Client.Text;
            }
            else
            {
                IPAddr_Serv.Text = string.Empty;
                
            }
        }
        //}
        public void Ping_all()
        {

            string gate_ip = NetworkGateway();
           // MessageBox.Show(gate_ip);

            //Extracting and pinging all other ip's.
            string[] array = gate_ip.Split('.');

            for (int i = 2; i <= 255; i++)
            {
                toolStripStatusLabel3.Text = string.Format("Scanning: {0}",i);     
                
                string ping_var = array[0] + "." + array[1] + "." + array[2] + "." + i;              
                // MessageBox.Show(ping_var);
                Ping(ping_var, 4, 4000);

                //This is sending PC to BSOD 
                //tcpip.sys is culprit
               // System.Threading. Thread.Sleep(5000);
                SS.Refresh();
             

            }          
            
        }

        static string NetworkGateway()
        {
            string ip = null;

            foreach (NetworkInterface f in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (f.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (GatewayIPAddressInformation d in f.GetIPProperties().GatewayAddresses)
                    {
                        ip = d.Address.ToString();
                        Console.WriteLine(string.Format("Network Gateway: {0}", ip));
                        MessageBox.Show(ip.ToString());
                    }
                }
            }

            MessageBox.Show(ip.ToString());
            return ip;
        }

        public void Ping(string host, int attempts, int timeout)
        {
            //for (int i = 0; i < attempts; i++)
            //{
            new Thread(delegate()
            {
                try
                {
                    Ping ping = new Ping();
                    ping.SendAsync(host, timeout, host);
                    ping.PingCompleted += new PingCompletedEventHandler(PingCompleted);
                }
                catch(Exception e)
                {

                    MessageBox.Show(e.Message.ToString());

                    //Ping ping = new Ping();
                    //ping.SendAsync(host, timeout, host);
                    //ping.PingCompleted += new PingCompletedEventHandler(PingCompleted);
                    // Do nothing and let it try again until the attempts are exausted.
                    // Exceptions are thrown for normal ping failurs like address lookup
                    // failed.  For this reason we are supressing errors.
                }
            }).Start();

            // }
        }
        private void PingCompleted(object sender, PingCompletedEventArgs e)
        {
            string ip = (string)e.UserState;
            if (e.Reply != null && e.Reply.Status == IPStatus.Success)
            {
                string hostname=GetHostName(ip);
                string macaddres=GetMacAddress(ip);
                string[] arr = new string[3];
                
                arr[0] = ip;
                arr[1] = hostname;
                arr[2] = macaddres;
                // Logic for Ping Reply Success
                ListViewItem item;
                if (this.InvokeRequired)
                {
                    
                    this.Invoke(new Action(() =>
                    {
                        
                        item = new ListViewItem(arr);
                      
                            lstLocal.Items.Add(item);
                            int count = lstLocal.Items.Count;
                            toolStripStatusLabel4.Text = string.Format(count.ToString() + " Item(s)");
                        
                    }));
                }
                //else
                //{
                //    //item = new ListViewItem(arr);
                //    lstLocal.Items.Add(item);
                //}

                // Logic for Ping Reply Success

                // Console.WriteLine(String.Format("Host: {0} ping successful", ip));
                //MessageBox.Show("Ping Successful for " + ip.ToString());
            }
            else
            {
               // MessageBox.Show(e.Reply.Status.ToString());
            }
        }

        public string GetHostName(string ipAddress)
        {
            try
            {
                IPHostEntry entry = Dns.GetHostEntry(ipAddress);
                if (entry!= null)
                {
                    return entry.HostName;
                }
            }
            catch (SocketException)
            {
               //MessageBox.Show(e.Message.ToString());
            }

            return null;
        }

        //own MAC address?
        public string GetMacAddress(string ipAddress)
        {
            string macAddress = string.Empty;
            System.Diagnostics.Process Process = new System.Diagnostics.Process();
            Process.StartInfo.FileName = "arp";
            Process.StartInfo.Arguments = "-a " + ipAddress;
            Process.StartInfo.UseShellExecute = false;
            Process.StartInfo.RedirectStandardOutput = true;
            Process.StartInfo.CreateNoWindow = true;
            Process.Start();
            string strOutput = Process.StandardOutput.ReadToEnd();
            string[] substrings = strOutput.Split('-');
            if (substrings.Length >= 8)
            {
                macAddress = substrings[3].Substring(Math.Max(0, substrings[3].Length - 2))
                         + "-" + substrings[4] + "-" + substrings[5] + "-" + substrings[6]
                         + "-" + substrings[7] + "-"
                         + substrings[8].Substring(0, 2);
                return macAddress;
            }

            else
            {
                return "OWN Machine";
            }
        }



















              
        
        
    //---------------------------------------------CLIENT SIDE--------------------------------------------------------------------------------
        
        
        private void btnBrowse_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "File Sharing Client";
            dlg.ShowDialog();           
            SelectFile.Text= dlg.FileName;
            path = SelectFile.Text;
            FileName = dlg.SafeFileName;

        }
        void TransmitFileName(Stream stream, string fileName)
        {
            byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName),
            fileNameLengthBytes = BitConverter.GetBytes(fileNameBytes.Length);
            stream.Write(fileNameLengthBytes, 0, 4);
            stream.Write(fileNameBytes, 0, fileNameBytes.Length);
        }



        private void btnSend_Click_1(object sender, EventArgs e)
        {

            // show animated image
            pictureBox.Image = Properties.Resources.Animation;
            labelProgress.Text="Copying in Progress";
            // change button states
            btnSend.Enabled = false;
            btn_Cancel.Enabled = true;

            TcpClient clientSocket = new TcpClient();
            // client.Connect(IPAddr_Client.Text, 8004);
            IPAddress ipaddrcl = IPAddress.Parse(IPAddr_Client.Text);
            int port = Convert.ToInt32(PortNo_Client.Text);

            // Connect to server
            try
            {
                clientSocket.Connect(new IPEndPoint(ipaddrcl, port));

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
           
             netstream = clientSocket.GetStream();

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {

                try
                {
                   

                    TransmitFileName(netstream, Path.GetFileName(path));
                    int data_len = (int)fs.Length;
                    byte[] buffer = new byte[bufferSize];
                    int totalbytes = 0;
                    while (totalbytes < data_len)
                    {
                        pictureBox.Image = Properties.Resources.Animation;
                        var bytesread = fs.Read(buffer, 0, buffer.Length);
                       
                        if (totalbytes == data_len)
                        {

                            pictureBox.Image = null;
                            break;
                        }
                        try
                        {

                                netstream.Write(buffer, 0, bytesread);
                                totalbytes += bytesread;
                          
                                                          

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            pictureBox.Image = Properties.Resources.Warning;
                            labelProgress.Text = "Error in transmission";

                        }
                    }

                }

                finally
                {

                    MessageBox.Show("Data transfer completed");
                    labelProgress.Text = "Copying Done";
                    pictureBox.Image = Properties.Resources.Information;
                    fs.Close();
                    netstream.Close();
                    // change button states
                    btnSend.Enabled = true;
                    btn_Cancel.Enabled = false;
                }

            }
            
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            if (Cancellation == true)
            {
                pictureBox.Image = Properties.Resources.Warning;
                DialogResult dialogResult = MessageBox.Show("Are you Sure you want to cancel the copying?", "Copying Process", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    netstream.Close();
                   labelProgress.Text = "Operation Cancelled by User";
                    
                }
                else if (dialogResult == DialogResult.No)
                {
                    fs.Close();
                    //do nothing
                }
              

            }
        }   

       /* private void btnlisten_Click(object sender, EventArgs e)
        { }*/


//-----------------------------------------------SERVER SIDE----------------------------------------------------------------------------------------------------------------
        private void btnlisten_Click_1(object sender, EventArgs e)
        {
            ThreadStart threaddelegate = new ThreadStart(Thread);
            Thread newthread = new Thread(threaddelegate);
            newthread.Start();

        }
        string DecodeFileName(Stream stream)
        {
            byte[] fileNameLengthBuffer = new byte[4];

            FillBufferFromStream(stream, fileNameLengthBuffer);
            int fileNameLength = BitConverter.ToInt32(fileNameLengthBuffer, 0);
            byte[] fileNameBuffer = new byte[fileNameLength];
            FillBufferFromStream(stream, fileNameBuffer);
            return Encoding.UTF8.GetString(fileNameBuffer);
        }

        void FillBufferFromStream(Stream stream, byte[] buffer)
        {
            int cbTotal = 0;
            while (cbTotal < buffer.Length)
            {
                int cbRead = stream.Read(buffer, cbTotal, buffer.Length - cbTotal);

                if (cbRead == 0)
                {
                    throw new InvalidDataException("premature end-of-stream");
                }

                cbTotal += cbRead;
            }
        }

        public void Thread()
        {
            IPAddress ipaddr = IPAddress.Parse(IPAddr_Serv.Text);
            var port = Convert.ToInt32(PortNo_serv.Text);          
            ConnectionState(ipaddr,port);
       
        }
        private void btnClear_Click(object sender, EventArgs e)
        {
            IPAddr_Serv.Text = null;
            PortNo_serv.Text = null;
            labelProgress.Text = null;
        }

        public void ConnectionState(IPAddress ipaddr,Int32 port) 
        {
           TcpClient client = new TcpClient();
            // Accept client
           TcpListener tcpListener = new TcpListener(ipaddr, port);
           tcpListener.Start();
           MessageBox.Show("Listening on port " + port);
           client = tcpListener.AcceptTcpClient();
           NetworkStream netStream = client.GetStream();
           string DirName = @"D:\Javed\Test\";
           string fileloc = Path.Combine(DirName, DecodeFileName(netStream));
           Directory.CreateDirectory(Path.GetDirectoryName(fileloc));
          
           try
            {
                pictureBox2.Image = Properties.Resources.philips;
                using (fs = new FileStream(fileloc, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    
                    netStream.CopyTo(fs);               
                   // label_server.Text = "Receiving Data";
                }


            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString());


            }
            finally
            {

                //check the size of file recieved 
                FileInfo fi = new FileInfo(fileloc);
                long siz = fi.Length;
                MessageBox.Show("Data Recieved: File Size is " + SizeSuffix(siz));
                pictureBox2.Image = null;
               // Thread();

          }             
        }
        

        //method to get size in Kb/Mb/Gb etc.
        static string SizeSuffix(Int64 value)
        {
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return "0.0 bytes"; }

            int mag = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            return string.Format("{0:n1} {1}", adjustedSize, SizeSuffixes[mag]);
        }

   //     private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        //{}

       
        
            

        private void Refresh_button_Click(object sender, EventArgs e)
        {

            lstLocal.Clear();
            lstLocal.View = View.Details;           
            lstLocal.GridLines = true;
            lstLocal.FullRowSelect = true;
            lstLocal.Columns.Add("IP Address", 100);
            lstLocal.Columns.Add("HostName", 80);
            lstLocal.Columns.Add("MAC Address", 300);

            Ping_all();
        }

        private void CLear_Click(object sender, EventArgs e)
        {
            IPAddr_Client.Text = null;
            PortNo_Client.Text = null;
           
        }

        private void SS_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void toolStripStatusLabel2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.puretechh.com");
        }

        private void toolStripProgressBar2_Click(object sender, EventArgs e)
        {

        }

        private void toolStripStatusLabel4_Click(object sender, EventArgs e)
        {

        }

        

        //private void button1_Click(object sender, EventArgs e)
        //{

        //}








        //private void IPAddr_Serv_TextChanged(object sender, EventArgs e)
        //{

        //}





        //private void textBox2_TextChanged(object sender, EventArgs e)
        //{

        //}

        

       

       

       















    }
}
