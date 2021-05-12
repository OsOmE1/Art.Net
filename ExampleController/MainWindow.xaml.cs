using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using ArtNet.Packets;
using ArtNet.Sockets;


namespace ExampleController
{
    public struct RGBW
    {
        public int R;
        public int G;
        public int B;
        public int W;
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

            var localIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList
                .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                .FirstOrDefault();

            socket.Begin(localIP, IPAddress.Parse("255.255.255.0"));
        }


        public void OnChange()
        {
            for (int i = 0; i < 8; i++)
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
                dmxPacket.Universe = i;
                for (int j = 0; j < 58; j++)
                {
                    dmxPacket.Data[j * 4] = (byte)Color.R;
                    dmxPacket.Data[j * 4 + 1] = (byte)Color.G;
                    dmxPacket.Data[j * 4 + 2] = (byte)Color.B;
                    dmxPacket.Data[j * 4 + 3] = (byte)Color.W;
                }
                socket.Send(dmxPacket);
            }
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
