using SharpVectors.Converters;
using System;
using System.Collections.Generic;
using System.IO;
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
using WpfApp1.Models;

namespace WpfApp1.Controls
{
    /// <summary>
    /// Interação lógica para GateControl.xam
    /// </summary>
    public partial class GateControl : UserControl
    {
        public GateModel Model { get; private set; }
        public GateControl()
        {
            InitializeComponent();
        }

        public class InputPortClickedEventArgs : RoutedEventArgs
        {
            public int InputIndex { get; set; }

            public InputPortClickedEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source) { }
        }

        public void Initialize(GateModel model, string imageFileName)
        {
            Model = model;

            string imagePath = $"/WpfApp1;component/Images/{imageFileName}";
            
            if(File.Exists(imagePath) && imageFileName.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
            {
                SvgViewbox svg = new SvgViewbox
                {
                    Source = new Uri(imagePath, UriKind.Relative),
                    Stretch = Stretch.Uniform,
                    Width = ImgGate.Width,
                    Height = ImgGate.Height
                };

                if (ImgGate.Parent is Grid parentGrid)
                {
                    parentGrid.Children.Remove(ImgGate);
                    parentGrid.Children.Add(svg);
                }
            }
            else if (File.Exists(imagePath))
            {
                ImgGate.Source = new BitmapImage(new Uri($"pack://application:,,,/WpfApp1;component/{imagePath}"));
            }
                UpdatePortVisuals();
        }

        public void UpdatePortVisuals()
        {
            //pinta porta de verde se a saída for true, cinza se false
            Input0.Fill = (Model.Inputs.Count > 0 && Model.Inputs[0].Output) ? Brushes.Green : Brushes.Gray;
            Input1.Fill = (Model.Inputs.Count > 1 && Model.Inputs[1].Output) ? Brushes.Green : Brushes.Gray;
            Output.Fill = Model.Output ? Brushes.Green : Brushes.Gray;
        }

        //evento para conectaar fios serão tratados no Form (ou janela) pai via bubbling
        private void OutputPort_MouseDown(object sender, MouseButtonEventArgs e) => RaiseEvent(new RoutedEventArgs(OutputPortClickedEvent, this));

        private void InputPort_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int index = (sender == Input0) ? 0 : 1;
            var args = new InputPortClickedEventArgs(InputPortClickedEvent, this) { InputIndex = index };
            RaiseEvent(args);
        }

        public static readonly RoutedEvent OutputPortClickedEvent = EventManager.RegisterRoutedEvent(
            "OutputPortClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GateControl));

        public static readonly RoutedEvent InputPortClickedEvent = EventManager.RegisterRoutedEvent(
            "InputPortClicked",RoutingStrategy.Bubble,typeof(RoutedEventHandler),typeof(GateControl));
    }
}
