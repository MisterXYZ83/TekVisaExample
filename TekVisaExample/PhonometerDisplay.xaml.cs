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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TekVisaExample
{
    /// <summary>
    /// Logica di interazione per PhonometerDisplay.xaml
    /// </summary>
    public partial class PhonometerDisplay : UserControl
    {

        public class PhonometerStatus
        {
            public double Spl;
            public bool Max;
            public bool Under;
            public bool Over;
            public bool CurveA;
            public bool CurveC;
            public bool Fast;
            public bool Slow;

            public RangeDB Range;

            public PhonometerStatus()
            {
                Spl = 0.0;
                Max = false;
                Under = false;
                Over = false;
                CurveA = false;
                CurveC = false;
                Fast = false;
                Slow = false;
            }

            public static string RangeToString(RangeDB range)
            {
                if (range == RangeDB.Scale_30_130) return "30-130";
                else if (range == RangeDB.Scale_30_80) return "30-80";
                else if (range == RangeDB.Scale_40_90) return "40-90";
                else if (range == RangeDB.Scale_50_100) return "50-100";
                else if (range == RangeDB.Scale_60_110) return "60-110";
                else if (range == RangeDB.Scale_70_120) return "70-120";
                else if (range == RangeDB.Scale_80_130) return "80-130";

                return "";
            }

            public static void FillStatus(PhonometerStatus status, byte[] input)
            {
                if (status != null && input.Length >= 5 && (input[0] == 0x02) && (input[4] == 0x03))
                {
                    byte data = input[1];

                    status.CurveA = ((data & 0x40) != 0);
                    status.CurveC = ((data & 0x40) == 0);
                    status.Fast = ((data & 0x80) == 0);
                    status.Slow = ((data & 0x80) != 0);
                    status.Max = ((data & 0x20) != 0);
                    status.Over = ((input[2] & 0x80) != 0);
                    status.Under = ((input[2] & 0x40) != 0);

                    byte scale = (byte)(data & 0x0F);

                    if (scale == 0) status.Range = RangeDB.Scale_30_80;
                    else if (scale == 1) status.Range = RangeDB.Scale_40_90;
                    else if (scale == 2) status.Range = RangeDB.Scale_50_100;
                    else if (scale == 3) status.Range = RangeDB.Scale_60_110;
                    else if (scale == 4) status.Range = RangeDB.Scale_70_120;
                    else if (scale == 5) status.Range = RangeDB.Scale_80_130;
                    else if (scale == 6) status.Range = RangeDB.Scale_30_130;

                    float value = 0.0f;

                    value = (input[2] & 0x10) * 100;
                    value += ((input[2] & 0x0F)) * 10;
                    value += ((input[3] & 0xF0) >> 4);
                    value += ((input[3] & 0x0F)) * (float)0.1;

                    //assign value
                    status.Spl = (double)value;
                }
            }

            public enum RangeDB { Scale_30_80, Scale_40_90, Scale_50_100, Scale_60_110, Scale_70_120, Scale_80_130, Scale_30_130 };

            public override string ToString()
            {
                string state = "";
                if (Over) state += "Over | ";
                if (Under) state += "Under | ";
                if (Fast) state += "Fast | ";
                if (Slow) state += "Slow | ";
                if (Max) state += "Max | ";
                state += PhonometerStatus.RangeToString(Range) + " | ";
                state += Spl.ToString("F1", CultureInfo.InvariantCulture);
                if (CurveA) state += "dBA";
                if (CurveC) state += "dBC";

                return state;
            }
        }


        protected PhonometerStatus mStatus;
        protected SolidColorBrush mNormalColor;
        protected SolidColorBrush mOverflowColor;

        public PhonometerDisplay()
        {
            InitializeComponent();

            mStatus = new PhonometerStatus();

            mOverflowColor = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
            mNormalColor = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));

            RefreshGUI();
        }

        public PhonometerStatus Status
        {
            get { return mStatus; }
            set
            {
                mStatus.Spl = value.Spl;

                mStatus.Range = value.Range;

                mStatus.Max = value.Max;

                mStatus.Over = value.Over;
                mStatus.Under = value.Under;

                mStatus.CurveA = value.CurveA;
                mStatus.CurveC = value.CurveC;

                mStatus.Fast = value.Fast;
                mStatus.Slow = value.Slow;

                RefreshGUI();
            }
        }

        protected void RefreshGUI()
        {
            if (mStatus.Fast)
            {
                fastLabel.Visibility = Visibility.Visible;
                slowLabel.Visibility = Visibility.Hidden;
            }
            else
            {
                fastLabel.Visibility = Visibility.Hidden;
                slowLabel.Visibility = Visibility.Visible;
            }

            if (mStatus.CurveA) curveLabel.Content = "A";
            else curveLabel.Content = "C";


            if (mStatus.Max) maxLabel.Visibility = Visibility.Visible;
            else maxLabel.Visibility = Visibility.Hidden;

            if (mStatus.Under) underLabel.Visibility = Visibility.Visible;
            else underLabel.Visibility = Visibility.Hidden;

            if (mStatus.Over) overLabel.Visibility = Visibility.Visible;
            else overLabel.Visibility = Visibility.Hidden;

            if (mStatus.Range == PhonometerStatus.RangeDB.Scale_30_130)
            {
                rangeMinText.Text = "30";
                rangeMaxText.Text = "130";
                splProgress.Minimum = 30.0;
                splProgress.Maximum = 130.0;
            }
            else if (mStatus.Range == PhonometerStatus.RangeDB.Scale_30_80)
            {
                rangeMinText.Text = "30";
                rangeMaxText.Text = "80";
                splProgress.Minimum = 30.0;
                splProgress.Maximum = 80.0;
            }
            else if (mStatus.Range == PhonometerStatus.RangeDB.Scale_40_90)
            {
                rangeMinText.Text = "40";
                rangeMaxText.Text = "90";
                splProgress.Minimum = 40.0;
                splProgress.Maximum = 90.0;
            }
            else if (mStatus.Range == PhonometerStatus.RangeDB.Scale_50_100)
            {
                rangeMinText.Text = "50";
                rangeMaxText.Text = "100";
                splProgress.Minimum = 50.0;
                splProgress.Maximum = 100.0;
            }
            else if (mStatus.Range == PhonometerStatus.RangeDB.Scale_60_110)
            {
                rangeMinText.Text = "60";
                rangeMaxText.Text = "110";
                splProgress.Minimum = 60.0;
                splProgress.Maximum = 110.0;
            }
            else if (mStatus.Range == PhonometerStatus.RangeDB.Scale_70_120)
            {
                rangeMinText.Text = "70";
                rangeMaxText.Text = "120";
                splProgress.Minimum = 70.0;
                splProgress.Maximum = 120.0;
            }
            else if (mStatus.Range == PhonometerStatus.RangeDB.Scale_80_130)
            {
                rangeMinText.Text = "80";
                rangeMaxText.Text = "130";
                splProgress.Minimum = 80.0;
                splProgress.Maximum = 130.0;
            }

            splText.Text = mStatus.Spl.ToString("F1", CultureInfo.InvariantCulture);
            splProgress.Value = mStatus.Spl;

            if (mStatus.Spl > splProgress.Maximum || mStatus.Spl < splProgress.Minimum) splProgress.Foreground = mOverflowColor;
            else splProgress.Foreground = mNormalColor;

        }
    }
}
