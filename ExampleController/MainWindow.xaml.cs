using ArtNet.Packets;
using ArtNet.Sockets;
using System.Net;
using System.Windows;

namespace ExampleController
{
    public struct RGBW
    {
        public byte R;
        public byte G;
        public byte B;
        public byte W;
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public RGBW Color;
        public ArtNetSocket socket;
        public MainWindow()
        {
            InitializeComponent();
            Color = new RGBW() { R = 0, G = 0, B = 0, W = 0 };
            socket = new ArtNetSocket() { EnableBroadcast = true };
            socket.Begin(IPAddress.Parse("192.168.178.39"), IPAddress.Parse("255.255.255.0"));
        }

        public void OnChange()
        {
            var dmxPacket = new ArtDmx
            {
                ProtVerHi = 6,
                ProtVerLo = 9,
                Sequence = 0x00,
                Physical = 0,
                SubUni = 0,
                Net = 0,
                LengthHi = 0,
                LengthLo = 0,
                Data = new byte[512]
            };
            dmxPacket.Length = 512;
            dmxPacket.Universe = 1;
            dmxPacket.Data[0] = Color.R;
            dmxPacket.Data[1] = Color.G;
            dmxPacket.Data[2] = Color.B;
            dmxPacket.Data[3] = Color.W;

            socket.SendToIp(dmxPacket, new IPEndPoint(IPAddress.Parse("192.168.178.31"), 6454));
        }

        private void Red_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Color.R = (byte)Red.Value;
            OnChange();
        }

        private void Green_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Color.G = (byte)Green.Value;
            OnChange();
        }

        private void Blue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Color.B = (byte)Blue.Value;
            OnChange();
        }

        private void White_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Color.W = (byte)White.Value;
            OnChange();
        }
    }
}
