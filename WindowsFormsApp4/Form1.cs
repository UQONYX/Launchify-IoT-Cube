using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NativeWifi;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using System.Resources;
using System.Reflection;
using System.Threading;
using System.IO.Ports;

namespace WindowsFormsApp4
{
    public partial class Form1 : Form
    {
        SerialPort s = new SerialPort();



        public Form1()
        {
            InitializeComponent();
        }

        //This function checks the available WIFI spots and COM ports
        private void button1_Click(object sender, EventArgs e)
        {
            //It refreshes the items in the textbox before displaying them
            comboBox1.Items.Clear();
            refreshWifi();
            comboBox2.Items.Clear();
            refreshPorts();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //get and display WIFI to combobox when form loads
            refreshWifi();
            //get and display COM ports when form loads
            refreshPorts();
            //set baud rate to Cube's rate
            s.BaudRate = 115200;
            //Will only open port if there is an available COM port (yes, I know this is a stupid way of doing it)
            if (comboBox2.Items.Count > 0)
            {
                s.Close();
                //sets port name to first one if an available port exists
                s.PortName = comboBox2.Text;
                s.Open();

            }
            backgroundWorker1.RunWorkerAsync();

        }
        
        //this refreshes the open COM ports and refreshes the combobox2's list
        private void refreshPorts()
        {

            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                comboBox2.Items.Add(port);
            }
            if(comboBox2.Items.Count > 0)
            {
                comboBox2.SelectedItem = comboBox2.Items[0];
            }
        }

        //This uploads to the CUBE
        private void button2_Click(object sender, EventArgs e)
        {
            s.Close();
            // Creates the spot where the batch file will be ran.
            MessageBox.Show("File will be uploaded to Cube. Your project is located at: " + Properties.Settings.Default.ProjectPath, "Confirm Upload");

            //This routine makes sure that the program doesn't jump the gun and upload without the proper things being built.
            if (Properties.Settings.Default.IsCompiled)
            {
                string batTest = System.Environment.GetEnvironmentVariable("TEMP") +
                                @"\batchfile.bat";
                using (StreamWriter sw = new StreamWriter(batTest))
                {
                    //Go to the Project's main environment (main directory)
                    sw.WriteLine(@"cd " + Properties.Settings.Default.ProjectPath);
                    //Runs Platformio command to upload to target
                    sw.WriteLine("platformio run --target upload");
                }
                //Kills process
                var proc = Process.Start(@"cmd.exe ", "/c " + batTest);
                // Process.Start();

                //proc.WaitForExit();
                proc.EnableRaisingEvents = true;

                proc.Exited += new EventHandler(proc_Exited);
                // s.Open();
                proc = Process.Start(@"cmd.exe ", "/c TASKKILL " + batTest);

            }
            else
            {
                MessageBox.Show("Please compile (Update WIFI) before uploading");
            }

        }

        //this event is fired when upload is complete. If platformio and the serial monitor run at the same time
        //there will be a conflict on the COM port
        private void proc_Exited(object sender, EventArgs e)
        {
            s.Open();
        }
       
        //This updates and builds program.
        //TODO turn this into a class library
        private void button3_Click(object sender, EventArgs e)
        {
            //Notify the user where the cube will connect to
            MessageBox.Show("Cube will connect to WIFI: " + comboBox1.SelectedItem.ToString(), @"Cube Wifi");
            //File is going to add aREST library to the program in order to build
            //File.Copy(@"C:\Users\Uthma\Downloads\aREST-master\aREST-master\aREST.h", Properties.Settings.Default.ProjectPath + @"\lib\aREST\aREST.h", true);
            String strFile = FileFetcher.getContent("WindowsFormsApp4.TextFile1.ino");
            //Adds the information to the spots necessary
            strFile = strFile.Replace("yourwifihere", comboBox1.Text);
            strFile = strFile.Replace("yourpasswordhere", textBox1.Text);
            File.WriteAllText(Properties.Settings.Default.ProjectPath + @"\src\main.ino", strFile);
            Properties.Settings.Default.IsCompiled = true;
        }

        //makes new directory
        private void newDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (folderBrowserDialog1.SelectedPath != Properties.Settings.Default.ProjectPath)
                {
                    string batTest = System.Environment.GetEnvironmentVariable("TEMP") +
                            @"\initializeproject.bat";
                    using (StreamWriter sw = new StreamWriter(batTest))
                    {
                        // sw.WriteLine(@"mkdir " + folderBrowserDialog1.SelectedPath);
                        sw.WriteLine("cd \"" + folderBrowserDialog1.SelectedPath + "\"");
                        //Initialize the platformio inside of the directory
                        sw.WriteLine("platformio init --board nodemcuv2");
                        //Navigate to lib folder and make the necessary aREST folder
                        sw.WriteLine(@"platformio lib install 31");
                        sw.WriteLine(@"platformio lib install 429");
                        sw.WriteLine(@"platformio lib install 166");
                        //sw.WriteLine("mkdir aREST Adafruit_BME280_Library Adafruit_Sensor-master");
                        //Add a MAIN file.. This may not be necessary as this part is being done in the compile program event
                        //TODO Reduce this redundancy 
                        sw.WriteLine("cd \"" + folderBrowserDialog1.SelectedPath + "\"" + @"\src");
                        sw.WriteLine("abc > main.ino");
                    }

                    //TODO Code this dynamically
                    //Start the process of Streamwriter
                    Process.Start(@"cmd.exe ", "/c " + batTest);
                    // Once the process is complete, KILL the process
                    Process.Start(@"cmd.exe ", "/c TASKKILL " + batTest);
                    //File is going to add aREST library to the program in order to build successfully


                }
                //This is a dangerous line of code below. Even if the above file initializing process fails,
                //it will make the new main project path equal to selected path which will prevent 
                //the init process from happening again (because the if statement above)
                Properties.Settings.Default.ProjectPath = folderBrowserDialog1.SelectedPath;

                //Refreshes this variable to FALSE as the new directory has not been initialized yet
                Properties.Settings.Default.IsCompiled = false;
            }
        }

        private void openDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //checks what the current file is.
            MessageBox.Show("your current project file is: " + Properties.Settings.Default.ProjectPath);
        }

        //Does some necessary tasks at closing
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Makes sure that default is saved.
            backgroundWorker1.Dispose();
            Properties.Settings.Default.Save();
        }

        //This is taken directly from an example online...
        private void refreshWifi()
        {
            //Code snippet I got from the internet to get WIFI signals
            WlanClient client = new WlanClient();
            foreach (WlanClient.WlanInterface wlanInterface in client.Interfaces)
            {
                Wlan.WlanAvailableNetwork[] networks = wlanInterface.GetAvailableNetworkList(0);
                foreach (Wlan.WlanAvailableNetwork network in networks)
                {
                    Wlan.Dot11Ssid ssid = network.dot11Ssid;
                    string networkName = Encoding.ASCII.GetString(ssid.SSID);
                    comboBox1.Items.Add(networkName);
                }
            }
            comboBox1.SelectedItem = comboBox1.Items[1];
        }

        //Takes care of NULL point error for CB1
        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {

            //Makes sure value of combobox is not null
            if (String.IsNullOrEmpty(comboBox1.Text))
            {
                //Notifies user and reverts to the first wifi on the list
                MessageBox.Show("You must pick a WIFI to connect to.", "Empty WIFI Selected");
                comboBox1.SelectedItem = comboBox1.Items[0];
            }

        }
        //This method checks if a program is installed.
        // TODO use this for checking if Python and PlatformIO are installed


        private void refreshDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string batTest = System.Environment.GetEnvironmentVariable("TEMP") +
                            @"\initializeproject.bat";
            using (StreamWriter sw = new StreamWriter(batTest))
            {
                // sw.WriteLine(@"mkdir " + folderBrowserDialog1.SelectedPath);
                sw.WriteLine("cd \"" + folderBrowserDialog1.SelectedPath + "\"");
                //Initialize the platformio inside of the directory
                sw.WriteLine("platformio init --board nodemcuv2");
                //Navigate to lib folder and make the necessary aREST folder
                sw.WriteLine(@"platformio lib install 31");
                sw.WriteLine(@"platformio lib install 429");
                sw.WriteLine(@"platformio lib install 166");
                //sw.WriteLine("mkdir aREST Adafruit_BME280_Library Adafruit_Sensor-master");
                //Add a MAIN file.. This may not be necessary as this part is being done in the compile program event
                //TODO Reduce this redundancy 
                sw.WriteLine("cd \"" + folderBrowserDialog1.SelectedPath + "\"" + @"\src");
                sw.WriteLine("abc > main.ino");
            }

            //TODO Code this dynamically
            //Start the process of Streamwriter
            Process.Start(@"cmd.exe ", "/c " + batTest);
            // Once the process is complete, KILL the process
            Process.Start(@"cmd.exe ", "/c TASKKILL " + batTest);
            //File is going to add aREST library to the program in order to build successfully
        }

        //This updates the serial monitor as a background task. Runs indefinitely when called.
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

            while (true)
            {
                if (s.IsOpen)
                    //checks if port is available
                {
                    try
                    {
                        //sends serial data to event
                        backgroundWorker1.ReportProgress(0, s.ReadExisting());
                        Thread.Sleep(20);
                        //e.Result = data;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }

        }

        //Gets information from eventargs of bgw1_DoWork and safely changes UI controls
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (!richTextBox1.IsDisposed) //Has to check if !disposed because control must be available according to .NET
            {
                //Add text to textbox
                richTextBox1.AppendText(e.UserState.ToString());
            }
            //  MessageBox.Show(e.UserState.ToString());
        }

        //Autoscroll
        //TODO make this optional
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.ScrollToCaret();
        }

        //refreshes COM port to the one selected by the combobox
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.Items.Count > 0)
            {
                s.Close();
                s.PortName = comboBox2.Text;
                s.Open();
            }
        }
    }
}
