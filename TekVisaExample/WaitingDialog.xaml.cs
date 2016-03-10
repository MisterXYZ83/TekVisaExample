using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Logica di interazione per WaitingDialog.xaml
    /// </summary>
    public partial class WaitingDialog : Window
    {

        protected bool mPaused;
        protected MainWindow mController;

        public WaitingDialog()
        {
            InitializeComponent();

            mPaused = false;
        }

        public MainWindow Controller
        {
            set { mController = value; }
            get { return mController; }
        }
        

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            //close
            Close();
        }

        public double Frequency
        {
            set
            {
                frequencyText.Text = value.ToString("F2", CultureInfo.InvariantCulture);
            }
        }

        public double Current
        {
            set
            {
                currentText.Text = value.ToString("F2", CultureInfo.InvariantCulture) + "mA";
            }
        }

        public PhonometerDisplay.PhonometerStatus PhonometerStatus
        {
            set
            {
                phonoDisplay.Status = value;
            }
        }

        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            if ( mPaused && mController != null )
            {
                //restart
                mPaused = false;

                Controller.PauseSweep = false;

                pauseButton.Content = "Pausa";
            }
            else if ( mController != null )
            {
                //stop

                mPaused = true;

                Controller.PauseSweep = true;

                pauseButton.Content = "Riprendi";
            }
        }
    }
}
