using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UdpClientApplication
{
    public class UdpClient
    {
        public event Action<Packet> PacketReceived;

        public ushort DestinationPort { get; set; }
        public ushort SourcePort { get; set; }

        public MacAddress SourceMacAddress { get; set; }
        public MacAddress DestinationMacAddress { get; set; }

        public IpV4Address SourceIpAddress { get; set; }
        public IpV4Address DestinationIpAddress { get; set; }

        private IPacketDevice _device;

        public UdpClient(IPacketDevice device)
        {
            _device = device;
            DestinationPort = 0;
            SourcePort = 0;
            SourceMacAddress = MacAddress.Zero;
            DestinationMacAddress = MacAddress.Zero;
            SourceIpAddress = IpV4Address.Zero;
            DestinationIpAddress = IpV4Address.Zero;
        }

        public async void SendPacket()
        {
            using (PacketCommunicator communicator = _device.Open(100, PacketDeviceOpenAttributes.DataTransferUdpRemote, 1000))
            {
                Packet packet = BuildUdpPacket();
                communicator.SendPacket(packet);
            }
        }
        
        public async void ReceivePacket() 
        {
            using (PacketCommunicator communicator = _device.Open(100, PacketDeviceOpenAttributes.Promiscuous, 1000))
            {
                communicator.SetFilter($"udp and ether dst host {SourceMacAddress}");
                communicator.ReceivePacket(out Packet packet);
                if (packet != null)
                {
                    PacketReceived?.Invoke(packet);
                }
            }
        }

        private Packet BuildUdpPacket()
        {
            EthernetLayer ethernetLayer = new EthernetLayer
            {
                Source = SourceMacAddress,
                Destination = DestinationMacAddress,
                EtherType = EthernetType.None, // Will be filled automatically.
            };

            IpV4Layer ipV4Layer = new IpV4Layer
            {
                Source = SourceIpAddress,
                CurrentDestination = DestinationIpAddress,
                Fragmentation = IpV4Fragmentation.None,
                HeaderChecksum = null, // Will be filled automatically.
                Identification = 123,
                Options = IpV4Options.None,
                Protocol = null, // Will be filled automatically.
                Ttl = 255,
                TypeOfService = 0,
            };

            UdpLayer udpLayer = new UdpLayer
            {
                SourcePort = SourcePort,
                DestinationPort = DestinationPort,
                Checksum = null, // Will be filled automatically.
                CalculateChecksumValue = true,
            };

            PayloadLayer payloadLayer = new PayloadLayer
            {
                Data = new Datagram(Encoding.ASCII.GetBytes(GenerateRandomString())),
            };

            PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, udpLayer, payloadLayer);

            return builder.Build(DateTime.Now);
        }

        private string GenerateRandomString()
        {
            Random random = new Random();
            string randomString = string.Empty;

            for (int i = 0; i < 128 - random.Next(0, 100); i++)
            {
                randomString += (char)random.Next(33, 127);
            }

            return randomString;
        }
    }
}
