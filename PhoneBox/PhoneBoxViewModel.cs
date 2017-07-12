using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NAudio.Wave;

namespace PhoneBox
{
    internal class PhoneBoxViewModel : INotifyPropertyChanged
    {
        private static int NumImputs = 16;

        private bool canStart = false;
        private bool canStop = false;

        private List<WaveStream> imputStreams = new List<WaveStream>();
        private List<WaveOut> outputs = new List<WaveOut>();

        public PhoneBoxViewModel()
        {
            this.Devices = new ObservableCollection<CheckBoxItem<WaveOutCapabilities>>();

            this.InitializeImputs();
        }
        
        public ObservableCollection<CheckBoxItem<WaveOutCapabilities>> Devices { get; set; }

        public bool CanStart
        {
            get { return this.canStart; }
            set
            {
                this.canStart = value;
                this.NotifyPropertyChanged("CanStart");
            }
        }

        public bool CanStop
        {
            get { return this.canStop; }
            set
            {
                this.canStop = value;
                this.NotifyPropertyChanged("CanStop");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RefreshDevices()
        {
            this.Stop();
            this.Devices.Clear();

            for (int i=0; i < WaveOut.DeviceCount; ++i)
            {
                this.Devices.Add(new CheckBoxItem<WaveOutCapabilities>(WaveOut.GetCapabilities(i), false, i));
            }

        }

        public void Start()
        {
            int inputIndex = 0;

            this.CanStart = false;
            this.CanStop = true;

            foreach (var item in this.Devices)
            {
                if (!item.IsChecked) continue;

                int outputCount = item.Item.Channels;

                var inputChannelIndices = new List<int>();
                var inputs = new List<IWaveProvider>(outputCount);
                var inputChannelCount = 0;
                for (int outputIndex = 0; outputIndex < outputCount && outputIndex < this.imputStreams.Count; ++outputIndex)
                {
                    var input = this.imputStreams[inputIndex];
                    inputs.Add(input);
                    inputChannelIndices.Add(inputChannelCount);
                    inputChannelCount += input.WaveFormat.Channels;
                    ++inputIndex;
                }

                var plex = new MultiplexingWaveProvider(inputs, outputCount);
                for (int i = 0; i < inputChannelIndices.Count; ++i)
                {
                    plex.ConnectInputToOutput(inputChannelIndices[i], i);
                }

                var output = new WaveOut();
                output.DeviceNumber = item.Number;
                output.Init(plex);
                output.Play();

                this.outputs.Add(output);
            }
        }

        public void Stop()
        {
            this.CanStop = false;
            this.CanStart = true;

            foreach(var output in outputs)
            {
                output.Stop();
                output.Dispose();
            }
        }
        
        private void NotifyPropertyChanged(String propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void InitializeImputs()
        {
            for(int i = 0; i < NumImputs; ++i)
            {
                var waveInput = new WaveFileReader("Puzzle\\" + i + ".wav");
                var loopStream = new LoopStream(waveInput);

                this.imputStreams.Add(loopStream);
            }
        }
    }
}
