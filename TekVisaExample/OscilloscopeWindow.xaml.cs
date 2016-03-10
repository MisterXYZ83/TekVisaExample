using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace TekVisaExample
{
    /// <summary>
    /// Logica di interazione per OscilloscopeWindow.xaml
    /// </summary>
    public partial class OscilloscopeWindow : Window
    {

        protected OscilloscopeModel mModel;
        protected double mLoadImpedance;

        public OscilloscopeWindow(OscilloscopeModel model)
        {
            InitializeComponent();

            mModel = model;

            mModel.PlotWindow = this;

            oscilloscopePlot.Model = mModel.PlotModel;

            Loaded += OscilloscopeWindow_Loaded;
            Closing += OscilloscopeWindow_Closing;

            mLoadImpedance = 0;
        }

        private void OscilloscopeWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        public double LoadImpedance
        {
            get
            {
                return mLoadImpedance;
            }

            set
            {
                mLoadImpedance = value;
            }
        }

        private void OscilloscopeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            oscilloscopePlot.InvalidatePlot(true);
        }

        public OscilloscopeModel Model
        {
            get
            {
                return mModel;
            }
        }

    }
}
