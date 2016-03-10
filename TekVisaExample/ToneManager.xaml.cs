using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace ControllerSirena
{
    /// <summary>
    /// Logica di interazione per ToneManager.xaml
    /// </summary>
    public partial class ToneManager : UserControl
    {
        public static double DefaultMinimumFrequency = 100.0;
        public static double DefaultMaximumFrequency = 100000000.0;
        protected static double SplitterWidth = 2.0;

        protected double mMinFrequency;
        protected double mMaxFrequency;
        protected double mTimeInterval;

        protected List<Slot> mSlots;
        protected List<GridSplitter> mSplitters;

        protected SolidColorBrush mSplitterColor;

        public ToneManager()
        {
            InitializeComponent();

            mMinFrequency = DefaultMinimumFrequency;
            mMaxFrequency = DefaultMaximumFrequency;

            //slotArea.Orientation = Orientation.Horizontal;
            mSlots = new List<Slot>();
            mSplitters = new List<GridSplitter>();

            mSplitterColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x50, 0x50, 0x50));

            //timeInterval.KeyDown += timeInterval_KeyDown;
            //timeInterval.LostFocus += timeInterval_LostFocus;

            
        }

        /*protected void HandleTimeIntervalChange()
        {
            double new_ti = 0.0;

            if ( double.TryParse(timeInterval.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out new_ti) && new_ti > 0.1 )
            {
                double coeff = new_ti / mTimeInterval;

                mTimeInterval = new_ti;
                int n_child = mSlots.Count;

                for ( int k = 0 ; k < n_child ; k++ )
                {
                    Slot s = mSlots[k];

                    s.Duration *= coeff;
                }
                
            }
            else
            {
                timeInterval.Text = mTimeInterval.ToString("N2", CultureInfo.InvariantCulture);
            }
        }*/

        /*void timeInterval_LostFocus(object sender, RoutedEventArgs e)
        {
            HandleTimeIntervalChange();
        }

        void timeInterval_KeyDown(object sender, KeyEventArgs e)
        {
            if ( e.Key == Key.Enter ) HandleTimeIntervalChange();
        }*/


        public double MinimumFrequency
        {
            get { return mMinFrequency; }
            set
            {
                mMinFrequency = value;

                //should update children
            }
        }

        public double MaximumFrequency
        {
            get { return mMaxFrequency; }
            set
            {
                mMaxFrequency = value;

                //should update children
            }
            
        }

        public Grid SlotPanel
        {
            get { return slotArea; }
        }
       

        protected double TotalDurationChildren
        {
            get
            {
                double dT = 0.0;

                for ( int k = 0 ; k < mSlots.Count ; k++ )
                {
                    Slot s = mSlots[k] as Slot;
                    dT += s.Duration;
                }

                return dT;
            }
        }

        public double PixelToTimeInterval( double dx )
        {
            double dt = 0.0;
            double l = slotArea.ActualWidth - (mSlots.Count - 1) * SplitterWidth;

            double total_duration = mTimeInterval;

            if (l == 0) return 1;

            dt = total_duration / l * dx;

            return dt;
        } 

        public double TimeIntervalToPixel ( double dt )
        {

            double dx = 0.0;

            double l = slotArea.ActualWidth - (mSlots.Count - 1) * SplitterWidth;
            double total_duration = mTimeInterval;

            dx = l / total_duration * dt;

            if (total_duration == 0) return 1;
            return dx;
        }

        public double TimeInterval
        {

            get
            {
                return TotalDurationChildren;
            }

            set
            {
                mTimeInterval = value;

            }

        }

        public void RemoveAll()
        {
            mSlots.Clear();
            slotArea.Children.Clear();


            UpdateSlots();

        }

        public void RemoveSlot(Slot s)
        {
            if ( s != null && mSlots.Contains(s) )
            {
                mSlots.Remove(s);
                slotArea.Children.Remove(s);

                //update gui
                UpdateSlots();
            } 
        }

        
        public void UpdateSlots()
        {
            int n_childs = mSlots.Count;

            //reorganize columns
            slotArea.Children.Clear();
            slotArea.ColumnDefinitions.Clear();

            mSplitters.Clear();

            mTimeInterval = TotalDurationChildren;

            int idx_col = 0;

            for (int k = 0; k < n_childs; k++)
            {
                Slot obj = mSlots[k];
                double child_w = TimeIntervalToPixel(obj.Duration);
                double child_h = slotArea.ActualHeight;

                ColumnDefinition c_def = new ColumnDefinition();
                c_def.Width = new GridLength(child_w, GridUnitType.Star);
                slotArea.ColumnDefinitions.Add(c_def);
                Grid.SetColumn(obj, idx_col);

                if (k != n_childs - 1)
                {
                    ColumnDefinition c_split = new ColumnDefinition();
                    c_split.Width = new GridLength(SplitterWidth);

                    slotArea.ColumnDefinitions.Add(c_split);

                    //add a column splitter
                    GridSplitter splitter = new GridSplitter();
                    Grid.SetColumn(splitter, idx_col + 1);
                    splitter.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                    splitter.Background = mSplitterColor;

                    mSplitters.Add(splitter);

                }

                idx_col += 2;
            }

            //insert slots into columns
            idx_col = 0;
            for (int k = 0; k < n_childs; k++)
            {
                slotArea.Children.Add(mSlots[k]);
                if (k != n_childs - 1) slotArea.Children.Add(mSplitters[k]);
            }

            //timeInterval.Text = mTimeInterval.ToString("N2", CultureInfo.InvariantCulture);
        }

        public void AddSlot(Slot s)
        {
            if (s == null) return;
            if (mSlots.Contains(s)) return;

            mSlots.Add(s);

            UpdateSlots();
        }

        public void AddSlots(byte [] rawdata)
        {
            //parse data as a sequence 
            if (rawdata == null || rawdata.Length % 5 != 0) return;
            
        }

        public void AddSlots(List<Slot> list)
        {
            //parse data as a sequence 
            if (list == null || list.Count == 0) return;

            int ns = list.Count;

            for ( int k = 0; k < ns; k++ )
            {
                AddSlot(list[k]);
            }

        }

        public List<Slot> Slots
        {
            get { return mSlots; }
        }

        public class TimingInfo
        {
            protected byte mReloadRegister;
            protected byte mPrescaler;
            protected int mRepetitions;
            protected TimingInfoType mType;
      
            public byte Prescaler
            {
                get { return mPrescaler; }
                set { mPrescaler = value; }
            }

            public byte ReloadRegister
            {
                get { return mReloadRegister; }
                set { mReloadRegister = value; }
            }

            public int Repetitions
            {
                get { return mRepetitions; }
                set { mRepetitions = value; }
            }

            public TimingInfoType Type
            {
                get { return mType; }
                set { mType = value; }
            }

            public enum TimingInfoType { Silence, Sinusoid, Sweep };

            public byte TypeRaw
            {
                get
                {
                    if (mType == TimingInfoType.Silence) return 0;
                    if (mType == TimingInfoType.Sinusoid) return 1;
                    if (mType == TimingInfoType.Sweep) return 2;

                    return 0;
                }
            }

            public static byte[] TimingInfoBytes (List<TimingInfo> list)
            {
                if (list == null || list.Count == 0) return null;

                //each timinginfo is 5 bytes: (reload:1)|(prescale:1)|(repets:2)|(type:1)
                int len = list.Count * 5; 
                byte[] rawdata = new byte[len];

                int pos = 0;

                for ( int k = 0; k < list.Count; k++ )
                {
                    TimingInfo info = list[k];

                    //type
                    rawdata[pos] = info.TypeRaw;

                    //reload register
                    rawdata[pos + 1] = info.ReloadRegister;

                    //prescaler
                    rawdata[pos + 2] = info.Prescaler;

                    //repets
                    byte[] rp = BitConverter.GetBytes(info.Repetitions); //flip for micro
                    rawdata[pos + 3] = rp[1];
                    rawdata[pos + 4] = rp[0];

                    rp = null;
                    

                    pos += 5;
                }

                return rawdata;
            }

        }


        public List<TimingInfo> CalculateTiming ()
        {
            List<TimingInfo> list = new List<TimingInfo>();

            double sysclock = 16000000;
            byte pscale = 4; 
            int n_sample = 16;

            //span all slots 
            for ( int k = 0; k < mSlots.Count; k++ )
            {
                Slot s = mSlots[k];

                if (s.Type == Slot.SlotType.None) continue;
                else if (s.Type == Slot.SlotType.Sinusoid )
                {
                    SinusoidSlot sin = s as SinusoidSlot;

                    double prescale = (int)Math.Pow(2, pscale);
                    byte sin_reload = (byte)(sysclock / (prescale * n_sample * sin.Frequency));
                    double dac_clk = (double)(sin_reload * prescale) / sysclock;
                    int n_rip = (int)(sin.Duration / dac_clk);

                    TimingInfo info = new TimingInfo();
                    info.ReloadRegister = sin_reload;
                    info.Prescaler = pscale;
                    info.Repetitions = n_rip;
                    info.Type = TimingInfo.TimingInfoType.Sinusoid;

                    list.Add(info);

                }
                else if (s.Type == Slot.SlotType.Sweep )
                {
                    SweepSlot sweep = s as SweepSlot;
                    List<TimingInfo> sweep_list = SimulateSweep(sweep.SweepStartFrequency, sweep.SweepStopFrequency, sweep.Duration, pscale, n_sample, sysclock); //fixed for now

                    if ( sweep_list.Count > 0 )
                    {
                        for ( int idx = 0; idx < sweep_list.Count; idx++ )
                        {
                            list.Add(sweep_list[idx]);
                        }

                        sweep_list.Clear();
                        sweep_list = null;

                    }
                }
                else if ( s.Type == Slot.SlotType.Silence )
                {
                    double prescale = (int)Math.Pow(2, pscale);
                    byte sil_reload = 128;
                    double dac_clk = (double)(sil_reload * prescale) / sysclock;
                    int n_rip = (int)(s.Duration / dac_clk);

                    TimingInfo info = new TimingInfo();
                    info.ReloadRegister = sil_reload;
                    info.Prescaler = pscale;
                    info.Repetitions = n_rip;
                    info.Type = TimingInfo.TimingInfoType.Silence;

                    list.Add(info);
                }
                
            }

            return list;
           
        }

        protected double OptimumPrescale ( double freq )
        {
            if (freq >= 200.0 && freq <= 1000) return 4.0;
            else if (freq > 1000 && freq <= 2000) return 2.0;
            else return 1.0;
            
        }

        protected List<TimingInfo> SimulateSweep ( double start_freq, double stop_freq, double sweep_time, int pscale, int n_sample, double sysclock )
        {
            List<TimingInfo> items = new List<TimingInfo>();

            //double prescale = (int)Math.Pow(2, pscale);
            pscale = (int)OptimumPrescale(start_freq);
            double prescale = (int)Math.Pow(2, OptimumPrescale(start_freq));
            double dt = 0, t_k = 0;

            //simulate timer timings
            byte start_reload = (byte)(sysclock / (prescale * n_sample * start_freq));
            byte stop_reload = (byte)(sysclock / (prescale * n_sample * stop_freq));

            byte reload_k = start_reload;

            TimingInfo actual_item = new TimingInfo();
            actual_item.ReloadRegister = reload_k;
            actual_item.Prescaler = (byte)pscale;
            actual_item.Repetitions = 0;
            actual_item.Type = TimingInfo.TimingInfoType.Sweep;

            items.Add(actual_item);

            dt = (double)(reload_k * prescale) / sysclock;
            t_k += dt;

            //print_tick(stream);
            //StreamWriter stream = new StreamWriter("reload.txt");
            //stream.WriteLine("{" + actual_item.Prescaler + ",\t" + actual_item.ReloadRegister + ",\t" + actual_item.Repetitions + "},");

            while (t_k < sweep_time)
            {
                //update reload
                reload_k = (byte)(sysclock / ((start_freq + (stop_freq - start_freq) * t_k / sweep_time) * prescale * n_sample));

                if (reload_k == actual_item.ReloadRegister) actual_item.Repetitions++;
                else
                {
                    //create new item
                    TimingInfo tmp = new TimingInfo();
                    
                    //calculate actual frequency and update pscale
                    double act_freq = sysclock / ( prescale * (reload_k+1) * n_sample );
                    pscale = (int)OptimumPrescale(act_freq);
                    prescale = Math.Pow(2, pscale);
                    reload_k = (byte)(sysclock / ((start_freq + (stop_freq - start_freq) * t_k / sweep_time) * prescale * n_sample));

                    //update values
                    tmp.Prescaler = (byte)pscale;
                    tmp.Repetitions = 0;
                    tmp.ReloadRegister = reload_k;
                    tmp.Type = TimingInfo.TimingInfoType.Sweep;
                    items.Add(tmp);

                    actual_item = tmp;

                    //stream.WriteLine("{" + actual_item.Prescaler + ",\t" + actual_item.ReloadRegister + ",\t" + actual_item.Repetitions + "},");
                }


                //update timing
                dt = (double)(reload_k * prescale) / sysclock;
                t_k += dt;
                
            }

            //stream.Close();

            return items;
        }

    }

}
