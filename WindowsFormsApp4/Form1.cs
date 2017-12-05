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

namespace WindowsFormsApp4
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            //This function checks the available WIFI spots
            //It refreshes the items in the textbox before displaying them
            comboBox1.Items.Clear();
            refreshWifi();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //get and display WIFI to combobox when form loads
            refreshWifi();
        }
        //This uploads to the CUBE
        private void button2_Click(object sender, EventArgs e)
        {
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
                Process.Start(@"cmd.exe ", "/c " + batTest);
                Process.Start(@"cmd.exe ", "/c TASKKILL " + batTest);
            }
            else
            {
                MessageBox.Show("Please compile (Update WIFI) before uploading");
            }
        }
        //This updates and builds program.
        private void button3_Click(object sender, EventArgs e)
        {
            
            //Notify the user where the cube will connect to
            MessageBox.Show("Cube will connect to WIFI: " + comboBox1.SelectedItem.ToString(), @"Cube Wifi");
            //File is going to add aREST library to the program in order to build
            File.Copy(@"C:\Users\Uthma\Downloads\aREST-master\aREST-master\aREST.h", Properties.Settings.Default.ProjectPath + @"\lib\aREST\aREST.h", true);
            String strFile = File.ReadAllText(@"C:\Users\Uthma\OneDrive\Documents\Visual Studio 2017\Projects\WindowsFormsApp4\WindowsFormsApp4\TextFile1.ino");
            //Adds the information to the spots necessary
            strFile = strFile.Replace("your_wifi_network_name", comboBox1.Text);
            strFile = strFile.Replace("your_wifi_network_password", textBox1.Text);
            File.WriteAllText(Properties.Settings.Default.ProjectPath + @"\src\main.ino", strFile);
            Properties.Settings.Default.IsCompiled = true;
        }

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
                        sw.WriteLine(@"cd " + folderBrowserDialog1.SelectedPath);
                        //Initialize the platformio inside of the directory
                        sw.WriteLine("platformio init --board nodemcuv2");
                        //Navigate to lib folder and make the necessary aREST folder
                        sw.WriteLine(@"cd " + folderBrowserDialog1.SelectedPath + @"\lib");
                        sw.WriteLine("mkdir aREST");
                        //Add a MAIN file.. This may not be necessary as this part is being done in the compile program event
                        //TODO Reduce this redundancy 
                        sw.WriteLine("cd " + folderBrowserDialog1.SelectedPath + @"\src");
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

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Makes sure that default is saved.
            Properties.Settings.Default.Save();
        }

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
        public static bool checkInstalled(string c_name)
        {
            string displayName;

            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey);
            if (key != null)
            {
                foreach (RegistryKey subkey in key.GetSubKeyNames().Select(keyName => key.OpenSubKey(keyName)))
                {
                    displayName = subkey.GetValue("DisplayName") as string;
                    if (displayName != null && displayName.Contains(c_name))
                    {
                        return true;
                    }
                }
                key.Close();
            }

            registryKey = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
            key = Registry.LocalMachine.OpenSubKey(registryKey);
            if (key != null)
            {
                foreach (RegistryKey subkey in key.GetSubKeyNames().Select(keyName => key.OpenSubKey(keyName)))
                {
                    displayName = subkey.GetValue("DisplayName") as string;
                    if (displayName != null && displayName.Contains(c_name))
                    {
                        return true;
                    }
                }
                key.Close();
            }
            return false;
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            
        }
    }
}
