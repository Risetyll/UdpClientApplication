using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace UdpClientApplication
{
    public class UdpClientViewModel : INotifyPropertyChanged
    {

        public List<IPacketDevice> AllDevices { get => _allDevices; }

        public IPacketDevice SelectedDevice
        {
            get => _device;
            set
            {
                _device = value;
                OnPropertyChanged(nameof(SelectedDevice));
            }
        }

        public string SourcePort
        {
            get => _sourcePort;
            set
            {
                _sourcePort = value;
                OnPropertyChanged(nameof(SourcePort));
            }
        }

        public string DestinationPort
        {
            get => _destinationPort;
            set
            {
                _destinationPort = value;
                OnPropertyChanged(nameof(DestinationPort));
            }
        }

        public string SourceMacAddress
        {
            get => _sourceMacAddress;
            set
            {
                _sourceMacAddress = value;
                OnPropertyChanged(nameof(SourceMacAddress));
            }
        }

        public string DestinationMacAddress
        {
            get => _destinationMacAddress;
            set
            {
                _destinationMacAddress = value;
                OnPropertyChanged(nameof(DestinationMacAddress));
            }
        }

        public string SourceIpAddress
        {
            get => _sourceIpAddress;
            set
            {
                _sourceIpAddress = value;
                OnPropertyChanged(nameof(SourceIpAddress));
            }
        }

        public string DestinationIpAddress
        {
            get => _destinationIpAddress;
            set
            {
                _destinationIpAddress = value;
                OnPropertyChanged(nameof(DestinationIpAddress));
            }
        }

        public string NumberOfPackets
        {
            get => _numberOfPackets.ToString();
            set
            {
                int.TryParse(value, out _numberOfPackets);
                OnPropertyChanged(nameof(DestinationIpAddress));
            }
        }

        public int ChannelBandwidth 
        { 
            get => _channelBandwidth; 
            set 
            { 
                _channelBandwidth = value; 
            } 
        }

        public string SentPackets { get => $"Отправлено: {_numberOfPackets}"; }

        public string DeliveredPackets { get => $"Доставлено: {_responsePacketsList.Count}"; }

        public string LostPackets { get => $"Потеряно: {_numberOfPackets - _responsePacketsList.Count}"; }

        public event PropertyChangedEventHandler PropertyChanged;

        private UdpClient _client;

        private readonly List<IPacketDevice> _allDevices;
        private IPacketDevice _device;

        private string _sourcePort;
        private string _destinationPort;

        private string _sourceMacAddress;
        private string _destinationMacAddress;

        private string _sourceIpAddress;
        private string _destinationIpAddress;

        private int _numberOfPackets;
        private List<Packet> _responsePacketsList;

        private int _channelBandwidth;

        public UdpClientViewModel()
        {
            _allDevices = new List<IPacketDevice>(LivePacketDevice.AllLocalMachine);
            _device = null;
            _sourcePort = ushort.MinValue.ToString();
            _destinationPort = ushort.MinValue.ToString();
            _sourceMacAddress = MacAddress.Zero.ToString();
            _destinationMacAddress = MacAddress.Zero.ToString();
            _sourceIpAddress = IpV4Address.Zero.ToString();
            _destinationIpAddress = IpV4Address.Zero.ToString();
            _numberOfPackets = 0;
            _responsePacketsList = new List<Packet>();            
        }

        public void InitializeUdpClient()
        {
            if (IsParametersValid())
            {
                _responsePacketsList.Clear();
                _client = new UdpClient(SelectedDevice)
                {
                    SourcePort = ushort.Parse(_sourcePort),
                    DestinationPort = ushort.Parse(_destinationPort),
                    SourceMacAddress = new MacAddress(_sourceMacAddress),
                    DestinationMacAddress = new MacAddress(_destinationMacAddress),
                    SourceIpAddress = new IpV4Address(_sourceIpAddress),
                    DestinationIpAddress = new IpV4Address(_destinationIpAddress),
                };
                _client.PacketReceived += HandleReceivedPacket;
            }
            else
            {
                throw new Exception("Неверные параметры");
            }
        }

        public async void StartPacketsSending()
        {
            double packetMaxByteSize = 128;
            double packetSendDelayMilliseconds = (packetMaxByteSize / (_channelBandwidth / 8)) * 1000;

            for (int i = 0; i < _numberOfPackets; i++)
            {
                await Task.Run(() => _client.SendPacket());
                await Task.Run(() => _client.ReceivePacket());
                Thread.Sleep((int)(packetSendDelayMilliseconds * 1000));

                OnPropertyChanged(nameof(SentPackets));
                OnPropertyChanged(nameof(DeliveredPackets));
                OnPropertyChanged(nameof(LostPackets));
            }
        }

        private bool IsParametersValid()
        {
            Regex macAddressRegex = new Regex("^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$");
            Regex ipAddressRegex = new Regex("^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");

            if (macAddressRegex.IsMatch(_sourceMacAddress) && macAddressRegex.IsMatch(_destinationMacAddress) &&
                ipAddressRegex.IsMatch(_sourceIpAddress) && ipAddressRegex.IsMatch(_destinationIpAddress) &&
                _device != null)
            {
                return true;
            }

            return false;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void HandleReceivedPacket(Packet packet)
        {
            _responsePacketsList.Add(packet);
        }
    }
}
