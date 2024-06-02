using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Networking;
namespace MauiApp1.Connectivity
{
    public class NetworkStateArgs : EventArgs
    {
        public NetworkStateArgs(bool isConnected)
        {
            IsConnected = isConnected;
        }
        public bool IsConnected { get; }
    }

   

    public class ConnectivityHelper
    {
        public EventHandler<NetworkStateArgs> ConnectionChangedHandler { get; set;} 
        public ConnectivityHelper()
        {
            Microsoft.Maui.Networking.Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
        }
        //public ConnectivityTest() =>
        //    Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;

        //~ConnectivityTest() =>
        //    Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;

        public bool IsConnected => Microsoft.Maui.Networking.Connectivity.NetworkAccess == NetworkAccess.Internet;
        void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            if (e.NetworkAccess == NetworkAccess.ConstrainedInternet)
            { 
                Console.WriteLine("Internet access is available but is limited.");
                ConnectionChangedHandler?.Invoke(this, new NetworkStateArgs(false));
            }


            else if (e.NetworkAccess != NetworkAccess.Internet)
            {
                Console.WriteLine("Internet access has been lost.");
                ConnectionChangedHandler?.Invoke(this, new NetworkStateArgs(false));
            }

            // Log each active connection
            Console.Write("Connections active: ");

            foreach (var item in e.ConnectionProfiles)
            {
                switch (item)
                {
                    case ConnectionProfile.Bluetooth:
                        Console.Write("Bluetooth");
                        break;
                    case ConnectionProfile.Cellular:
                        Console.Write("Cell");
                        break;
                    case ConnectionProfile.Ethernet:
                        Console.Write("Ethernet");
                        break;
                    case ConnectionProfile.WiFi:
                        Console.Write("WiFi");
                        break;
                    default:
                        break;
                }
            }

            Console.WriteLine();
        }
    }
}
