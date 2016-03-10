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

namespace ControllerSirena
{
    /// <summary>
    /// Logica di interazione per Slot.xaml
    /// </summary>
    public partial class Slot : UserControl
    {

        public enum SlotType {None, Silence, Sweep, Sinusoid };

        public static string SlotTypeString ( SlotType s )
        {
            if (s == SlotType.None) return "None";
            else if (s == SlotType.Silence) return "Silence";
            else if (s == SlotType.Sinusoid) return "Sinusoid";
            else if (s == SlotType.Sweep) return "Sweep";

            return null; 
        }

        protected SlotType mType;

        protected double mDuration;
        protected bool mSilence;

        protected ToneManager mManager;

        protected Slot mLeftSibling;
        protected Slot mRightSibling;

        protected SolidColorBrush mBorderColor;

        protected ContextMenu mSlotMenu;

        public Slot()
        {
            InitializeComponent();

            HorizontalAlignment = HorizontalAlignment.Stretch;

            mSilence = false;

            Loaded += Slot_Loaded;
            slotCanvas.SizeChanged += Slot_SizeChanged;
            MouseEnter += Slot_MouseEnter;
            MouseLeave += Slot_MouseLeave;
            durationText.KeyDown += durationText_KeyDown;
            durationText.LostFocus += durationText_LostFocus;

            mDuration = 1.0; //default 1s

            slotCanvas.ClipToBounds = true;

            mLeftSibling = null;
            mRightSibling = null;

            //leftGrip.Visibility = Visibility.Hidden;
            //rightGrip.Visibility = Visibility.Hidden;

            mBorderColor = new SolidColorBrush(Color.FromArgb(0xFF, 00, 0xAA, 00));
            slotBorder.BorderBrush = mBorderColor;

            leftText.Visibility = Visibility.Hidden;
            rightText.Visibility = Visibility.Hidden;

            mSlotMenu = new ContextMenu();
            this.ContextMenu = mSlotMenu;

            ContextMenuOpening += Slot_ContextMenuOpening;

            mType = SlotType.None;
        }

        public SlotType Type
        {
            get { return mType; }
        }

        private void Slot_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            mSlotMenu.Items.Clear();

            MenuItem item = new MenuItem();
            item.Header = "Elimina Slot";
            item.Click += DeleteSlot_Click;

            mSlotMenu.Items.Add(item);
        }

        private void DeleteSlot_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult dialogResult = MessageBox.Show("Eliminare lo slot?", "Conferma", MessageBoxButton.YesNo);

            if (dialogResult == MessageBoxResult.Yes)
            {
                mManager.RemoveSlot(this);
            }
        }

        protected void HandleTextChange()
        {
            //take text and set duration
            double new_d = 0.0;

            if (double.TryParse(durationText.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out new_d))
            {
                SetDuration(new_d);
            }
            else
            {
                UpdateDurationText();
            }

        }

        void durationText_LostFocus(object sender, RoutedEventArgs e)
        {
            HandleTextChange();
        }

        void durationText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) HandleTextChange();
        }

        protected virtual void Slot_MouseLeave(object sender, MouseEventArgs e)
        {
            //BorderBrush = null;
            //slotBorder.BorderBrush = null;
        }

        protected virtual void Slot_MouseEnter(object sender, MouseEventArgs e)
        {
            //BorderBrush = mBorderColor;
            //slotBorder.BorderBrush = mBorderColor;
        }


        public virtual Slot LeftSibling
        {
            get { return mLeftSibling; }
            set
            {
                mLeftSibling = value;

                //leftGrip.Visibility = mLeftSibling == null ?  Visibility.Hidden : Visibility.Visible;
            }
        }


        public virtual Slot RightSibling
        {
            get { return mRightSibling; }
            set
            {
                mRightSibling = value;

                //rightGrip.Visibility = mRightSibling == null ? Visibility.Hidden : Visibility.Visible;
            }
        }


        private void Slot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateGui();
        }

        private void Slot_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateGui();
        }

        public virtual ToneManager Manager
        {
            get { return mManager; }
            set
            {
                mManager = value;

            }
        }


        public void SetDuration(double d)
        {
            //get pixel as d
            mDuration = d;
            UpdateDurationText();

            if ( mManager != null ) mManager.UpdateSlots();

        }

        public double Duration
        {
            get { return mDuration; }
            set
            {
                SetDuration(value);
            }
        }



        public Canvas SlotCanvas
        {
            get { return slotCanvas; }
        }


        protected void UpdateDurationText()
        {
            durationText.Text = mDuration.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
        }

        protected virtual void UpdateGui()
        {
            //update string
            double dx = ActualWidth;
            double dt = mManager.PixelToTimeInterval(dx);

            mDuration = dt;

            UpdateDurationText();
        }

        protected double FrequencyToPixel(double freq, double min, double max)
        {
            double pos = 0.0;

            double h = double.IsNaN(SlotCanvas.ActualHeight) ? 0 : SlotCanvas.ActualHeight;

            pos = h * (1 - 1 / (max - min) * (freq - min));

            return pos;

        }

        protected double PixelToFrequency(double pos)
        {
            double max = mManager.MaximumFrequency;
            double min = mManager.MinimumFrequency;

            double h = double.IsNaN(SlotCanvas.ActualHeight) ? 1.0 : SlotCanvas.ActualHeight;

            return max + (min - max) / h * pos;

        }

    }


    public class SilenceSlot : Slot
    {
        public SilenceSlot()
        {
            mSilence = true;
            mType = SlotType.Silence;
        }
    }

    public class SweepSlot : Slot
    {
        protected Line mToneLine;

        protected double mStartFrequency;
        protected double mStopFrequency;
        protected double mDefaultStartFrequency;
        protected double mDefaultStopFrequency;
        protected bool mInitialized;

        protected Ellipse mLeftGrip;
        protected Ellipse mRightGrip;
        protected double GripWidth = 20;
        protected double GripHeight = 20;

        protected TextBox startFrequencyBox;
        protected TextBox stopFrequencyBox;
        protected bool mMouseMove;

        public SweepSlot()
        {
            mSilence = false;
            mStartFrequency = 0;
            mStopFrequency = 0;

            mToneLine = new Line();
            mToneLine.X1 = StartX;
            mToneLine.Y1 = StartY;
            mToneLine.X2 = StopX;
            mToneLine.Y2 = StopY;

            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            mToneLine.Stroke = brush;
            mToneLine.StrokeThickness = 4;

            SlotCanvas.Children.Add(mToneLine);

            /////grip
            mLeftGrip = new Ellipse();
            mRightGrip = new Ellipse();

            mLeftGrip.Visibility = Visibility.Hidden;
            mRightGrip.Visibility = Visibility.Hidden;

            mLeftGrip.Width = GripWidth;
            mLeftGrip.Height = GripHeight;

            mRightGrip.Width = GripWidth;
            mRightGrip.Height = GripHeight;

            SolidColorBrush sb = new SolidColorBrush(Color.FromArgb(255, 150, 0, 0));

            mLeftGrip.Fill = sb;
            mRightGrip.Fill = sb;

            mLeftGrip.StrokeThickness = 1;
            mRightGrip.StrokeThickness = 1;

            mLeftGrip.MouseMove += Grip_MouseMove;
            mRightGrip.MouseMove += Grip_MouseMove;

            SlotCanvas.Children.Add(mLeftGrip);
            SlotCanvas.Children.Add(mRightGrip);


            ///textbox
            startFrequencyBox = leftText;
            stopFrequencyBox = rightText;

            startFrequencyBox.Visibility = Visibility.Visible;
            stopFrequencyBox.Visibility = Visibility.Visible;


            startFrequencyBox.KeyDown += FrequencyBox_KeyDown;
            startFrequencyBox.LostFocus += FrequencyBox_LostFocus;

            stopFrequencyBox.KeyDown += FrequencyBox_KeyDown;
            stopFrequencyBox.LostFocus += FrequencyBox_LostFocus;

            mMouseMove = false;
            mInitialized = false;

            mType = SlotType.Sweep;
        }

        protected virtual void Grip_MouseMove(object sender, MouseEventArgs e)
        {
            //if (sender != mRightGrip && sender != mLeftGrip) return;

            if (!mMouseMove && (sender == mRightGrip || sender == mLeftGrip) && (Mouse.LeftButton == MouseButtonState.Pressed))
            {
                //start moving session
                mMouseMove = true;

                Ellipse target = sender as Ellipse;
                target.CaptureMouse();

                target.Cursor = Cursors.ScrollNS;

            }
            else if ((Mouse.LeftButton != MouseButtonState.Pressed))
            {
                mMouseMove = false;

                mLeftGrip.ReleaseMouseCapture();
                mRightGrip.ReleaseMouseCapture();
                mLeftGrip.Cursor = Cursors.None;
                mRightGrip.Cursor = Cursors.None;

                return;
            }

            //need only y coord
            double y_pos = e.GetPosition(SlotCanvas).Y;

            double freq = PixelToFrequency(y_pos);

            if (sender == mRightGrip)
            {
                mStopFrequency = freq;
                Canvas.SetTop(mRightGrip, y_pos - GripWidth / 2);
            }
            if (sender == mLeftGrip)
            {
                mStartFrequency = freq;
                Canvas.SetTop(mLeftGrip, y_pos - GripWidth / 2);
            }

            //update position of grip to keep linked with mouse

            UpdateGui();

        }

        private void FrequencyBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) HandleFrequencyBoxChange(sender);
        }

        private void FrequencyBox_LostFocus(object sender, RoutedEventArgs e)
        {
            HandleFrequencyBoxChange(sender);
        }


        protected virtual void HandleFrequencyBoxChange(object s)
        {
            TextBox t = null;

            if (s == startFrequencyBox) t = startFrequencyBox;
            else if (s == stopFrequencyBox) t = stopFrequencyBox;
            else return;

            //update
            double new_f = 0.0;
            if (double.TryParse(t.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out new_f)
                && new_f <= mManager.MaximumFrequency
                && new_f >= mManager.MinimumFrequency)
            {
                if (s == startFrequencyBox)
                {
                    mStartFrequency = new_f;
                }
                else if (s == stopFrequencyBox)
                {
                    mStopFrequency = new_f;
                }
                else return;

                UpdateGui();
            }
            else t.Text = mStartFrequency.ToString("F1", CultureInfo.InvariantCulture);

        }


        protected double StartX
        {
            get { return 0.0; }
        }

        protected double StartY
        {
            get
            {
                double min = mManager != null ? mManager.MinimumFrequency : ToneManager.DefaultMinimumFrequency;
                double max = mManager != null ? mManager.MaximumFrequency : ToneManager.DefaultMaximumFrequency;

                return FrequencyToPixel(mStartFrequency, min, max);
            }
        }

        protected double StopX
        {
            get { return double.IsNaN(SlotCanvas.ActualWidth) ? 0 : SlotCanvas.ActualWidth; }
        }

        protected double StopY
        {
            get
            {
                double min = mManager != null ? mManager.MinimumFrequency : ToneManager.DefaultMinimumFrequency;
                double max = mManager != null ? mManager.MaximumFrequency : ToneManager.DefaultMaximumFrequency;

                return FrequencyToPixel(mStopFrequency, min, max);
            }
        }

        public override ToneManager Manager
        {
            get
            {
                return base.Manager;
            }

            set
            {
                base.Manager = value;

                if (mManager == null) return;

                //save params
                mDefaultStartFrequency = mManager.MinimumFrequency;
                mDefaultStopFrequency = mManager.MaximumFrequency;

            }
        }

        public double SweepStartFrequency
        {
            get { return mStartFrequency; }
            set
            {
                mInitialized = true;
                mStartFrequency = value;

                //UpdateGui();
            }
        }

        public double SweepStopFrequency
        {
            get { return mStopFrequency; }
            set
            {
                mInitialized = true;
                mStopFrequency = value;

                //UpdateGui();
            }
        }

        public class FrequencySweep
        {
            public double StartFrequency;
            public double StopFrequency;
        }

        public FrequencySweep Sweep
        {
            get { return new FrequencySweep { StartFrequency = mStartFrequency, StopFrequency = mStopFrequency }; }
            set
            {
                if (value != null)
                {
                    mInitialized = true;
                    mStartFrequency = value.StartFrequency;
                    mStopFrequency = value.StopFrequency;

                    //UpdateGui();
                }
            }

        }

        protected override void Slot_MouseLeave(object sender, MouseEventArgs e)
        {
            base.Slot_MouseLeave(sender, e);

            mLeftGrip.Visibility = Visibility.Hidden;
            mRightGrip.Visibility = Visibility.Hidden;
        }

        protected override void Slot_MouseEnter(object sender, MouseEventArgs e)
        {
            base.Slot_MouseEnter(sender, e);

            mLeftGrip.Visibility = Visibility.Visible;
            mRightGrip.Visibility = Visibility.Visible;
        }

        protected override void UpdateGui()
        {
            base.UpdateGui();

            //calculate points
            slotCanvas.Children.Remove(mToneLine);
            slotCanvas.Children.Remove(mLeftGrip);
            slotCanvas.Children.Remove(mRightGrip);

            double s_x = StartX;
            double s_y = StartY;
            double e_x = StopX;
            double e_y = StopY;

            mToneLine.X1 = s_x;
            mToneLine.Y1 = s_y;
            mToneLine.X2 = e_x;
            mToneLine.Y2 = e_y;

            if (!mMouseMove)
            {
                Canvas.SetLeft(mLeftGrip, s_x - GripWidth / 2);
                Canvas.SetTop(mLeftGrip, s_y - GripHeight / 2);

                Canvas.SetLeft(mRightGrip, e_x - GripWidth / 2);
                Canvas.SetTop(mRightGrip, e_y - GripHeight / 2);

            }


            slotCanvas.Children.Add(mToneLine);
            slotCanvas.Children.Add(mRightGrip);
            slotCanvas.Children.Add(mLeftGrip);

            //update tbox
            startFrequencyBox.Text = mStartFrequency.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
            stopFrequencyBox.Text = mStopFrequency.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);


        }

    }

    public class SinusoidSlot : SweepSlot
    {

        protected double mFrequency;

        protected FrequencySweep mDummySweep;

        public SinusoidSlot() : base()
        {
            mSilence = false;
            
            stopFrequencyBox.Visibility = Visibility.Hidden;

            mFrequency = 0;

            mDummySweep = new FrequencySweep();
            mDummySweep.StartFrequency = mFrequency;
            mDummySweep.StopFrequency = mFrequency;

            //Sweep = mDummySweep;

            mType = SlotType.Sinusoid;

        }

        public override ToneManager Manager
        {
            get
            {
                return base.Manager;
            }

            set
            {
                base.Manager = value;

                if (mManager == null) return;

                //save params
                mFrequency = (mManager.MaximumFrequency + mManager.MinimumFrequency) / 2;

                mDummySweep.StartFrequency = mFrequency;
                mDummySweep.StopFrequency = mFrequency;
                Sweep = mDummySweep;
            }
        }

        public double Frequency
        {
            get { return mFrequency; }
            set
            {
                mFrequency = value;
                mDummySweep.StartFrequency = mFrequency;
                mDummySweep.StopFrequency = mFrequency;
                Sweep = mDummySweep;
            }
        }

        protected override void HandleFrequencyBoxChange(object s)
        {
            TextBox t = null;

            if (s == startFrequencyBox) t = startFrequencyBox;
            else if (s == stopFrequencyBox) t = stopFrequencyBox;
            else return;

            //update
            double new_f = 0.0;
            if (double.TryParse(t.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out new_f)
                && new_f <= mManager.MaximumFrequency
                && new_f >= mManager.MinimumFrequency)
            {
                mFrequency = new_f;
                mDummySweep.StartFrequency = mFrequency;
                mDummySweep.StopFrequency = mFrequency;

                Sweep = mDummySweep;

                UpdateGui();
            }
            else t.Text = mStartFrequency.ToString("F1", CultureInfo.InvariantCulture);

        }

        protected override void Grip_MouseMove(object sender, MouseEventArgs e)
        {
            //if (sender != mRightGrip && sender != mLeftGrip) return;

            if (!mMouseMove && (sender == mRightGrip || sender == mLeftGrip) && (Mouse.LeftButton == MouseButtonState.Pressed))
            {
                //start moving session
                mMouseMove = true;

                Ellipse target = sender as Ellipse;
                target.CaptureMouse();

                target.Cursor = Cursors.ScrollNS;

            }
            else if ((Mouse.LeftButton != MouseButtonState.Pressed))
            {
                mMouseMove = false;

                mLeftGrip.ReleaseMouseCapture();
                mRightGrip.ReleaseMouseCapture();
                mLeftGrip.Cursor = Cursors.None;
                mRightGrip.Cursor = Cursors.None;

                return;
            }

            //need only y coord
            double y_pos = e.GetPosition(SlotCanvas).Y;

            double freq = PixelToFrequency(y_pos);

            mFrequency = freq;
            mStartFrequency = freq;
            mStopFrequency = freq;

            Canvas.SetTop(mLeftGrip, y_pos - GripWidth / 2);
            Canvas.SetTop(mRightGrip, y_pos - GripWidth / 2);
            

            //update position of grip to keep linked with mouse

            UpdateGui();

        }
    }
}
