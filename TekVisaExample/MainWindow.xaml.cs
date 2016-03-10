using System;
using System.Collections;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
//using TekVISANet;
using ControllerSirena;
using System.IO.Ports;
using System.Threading;
using System.Timers;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Globalization;
using NationalInstruments.VisaNS;
using System.Net;

namespace TekVisaExample
{

    public class SessionInformation
    {
        public string Name;
        public DateTime Date;
        public string Buzzer;
        public string Description;
        public int AmplitutePeak;
        public int FrequencyStep;

        public List<DataPoint> AudioData;
        public List<DataPoint> CurrentData;
    }

    public class VisaInstrumentItem
    {
        protected string mVisaAddress;

        protected string mInstrumentName;

        public VisaInstrumentItem(string uri)
        {
            try
            {
                mVisaAddress = string.Copy(uri);

                //try to open a visa session to identify instrument
                MessageBasedSession s = (MessageBasedSession)ResourceManager.GetLocalManager().Open(mVisaAddress);

                string name = s.Query("*IDN?");

                mInstrumentName = string.Copy(name);

                s.Dispose();

                s = null;

            }
            catch ( Exception excp )
            {

            }


            /*VISA controller = new VISA();

            if (mVisaInstrument != null && controller != null )
            {
                bool status;
                string response;

                controller.Open(mVisaInstrument.ToString());
                status = controller.Write("*IDN?");

                if ( status )
                {
                    status = controller.Read(out response);

                    if (status) mInstrumentName = response;
                    
                }
                
                controller.Close();
                
            }*/
        }

        public override string ToString()
        {
            return mInstrumentName;
        }

        public string Resource
        {
            get { return mVisaAddress; }
        }
    }

    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {

        protected MessageBasedSession mOscilloscope;
        protected MessageBasedSession mFunctionGenerator;

        protected bool mStop;

        protected OscilloscopeWindow mPlotOscilloscopeWindow;
        protected PlotWindow mPlotWindow;
        protected FrequencyResponseModel mPlotModel;
        protected OscilloscopeModel mOscilloscopeModel;
        protected string mVoltageSampleCommand;

        protected SerialPort mSerialPort;
        protected System.Timers.Timer mSamplingTimer;

        protected List<DataPoint> mCurrentData;
        protected List<DataPoint> mOscilloscopeData;

        protected double mStartFrequency;
        protected double mStopFrequency;
        protected double mCurrentFrequency;
        protected double mFrequencyStep;
        protected string mSelectedComPort;

        protected string mOscilloscopeResource;

        protected WaitingDialog mWaitingDialog;

        protected object mPhonoLock;
        protected bool mTimerStop;
        protected bool mTimerPause;

        protected bool mOscilloscopeCapture;
        protected bool mPhonometerCapture;
        protected double mLoadImpedance;

        protected int mPhonometerLag;

        public MainWindow()
        {
            InitializeComponent();

            toneManager.Loaded += ToneManager_Loaded;
            
            //now enumerates instruments
            //ScanInstruments();

            //set comboboxes
            SetupInstrumentParams();

            //scan all serial port
            mSerialPort = new SerialPort();
            ScanSerialPorts();

            mPlotModel = new FrequencyResponseModel();
            mOscilloscopeModel = new OscilloscopeModel();

            mPlotWindow = new PlotWindow(mPlotModel);
            mPlotOscilloscopeWindow = new OscilloscopeWindow(mOscilloscopeModel);

            //timer
            mSamplingTimer = new System.Timers.Timer();
            mSamplingTimer.AutoReset = false;
            mSamplingTimer.Elapsed += SamplingElapsedCallback;

            mCurrentData = new List<DataPoint>();
            mOscilloscopeData = new List<DataPoint>();

            mCurrentFrequency = 0.0;
            mStartFrequency = 0.0;
            mStopFrequency = 0.0;
            mPhonoLock = new object();
            mTimerStop = false;

            mPhonometerCapture = true;
            mOscilloscopeCapture = true;


        Closing += MainWindow_Closing;
            
        }


        private void SetupInstrumentParams ()
        {
            //freq step 
            freqStepCombo.Items.Clear();

            for (int k = 0; k < 1000; k++)
            {
                string item_title = ((k + 1) * 10 ).ToString() + " Hz";
                freqStepCombo.Items.Add(item_title);
            }
            
            amplitudeCombo.Items.Clear();

            for ( int k = 0; k < 10; k++ )
            {
                string item_title = (k + 1).ToString() + " Vpp";
                amplitudeCombo.Items.Add(item_title);
            }
          
            phonoLagCombo.Items.Clear();
            
            phonoLagCombo.Items.Add("500 ms");
            phonoLagCombo.Items.Add("1000 ms");
            phonoLagCombo.Items.Add("1500 ms");
            phonoLagCombo.Items.Add("2000 ms");


            for ( int k = 0; k < 4; k++ )
            {
                probePosCombo.Items.Add("C" + (k + 1));
                probeNegCombo.Items.Add("C" + (k + 1));
            }

            probePosCombo.SelectedIndex = 0;
            probeNegCombo.SelectedIndex = 1;

            freqStepCombo.SelectedIndex = 0;
            amplitudeCombo.SelectedIndex = 0;
            phonoLagCombo.SelectedIndex = 0;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        public bool PauseSweep
        {
            get
            {
                return mTimerPause;
            }

            set
            {
                lock (mPhonoLock)
                {
                    mTimerPause = value;
                }
            }
        }

        private void SamplingElapsedCallback(object sender, ElapsedEventArgs e)
        {
            //1. set frequency to instrument
            //2. start generation
            //3. read N values from phonometer
            //4. take a mean and add to current dataset
            PhonometerDisplay.PhonometerStatus status;
            double peak_value = 0;

            //check if is it time to exit
            bool exit = false;
            bool pause = false;

            lock (mPhonoLock)
            {
                exit = mTimerStop;
                pause = mTimerPause;
            }

            if (exit) return;
            if (pause)
            {
                //wait and restart, nothing to do
                Thread.Sleep(mPhonometerLag);
                mSamplingTimer.Start();
                return;
            }

            int n_readings = 10;
            if (mPhonometerCapture) n_readings = 50;

            int total_read = n_readings;
            double average_spl = 0;
            double average_cur = 0;

            //set frequency
            int cur_freq = (int)mCurrentFrequency;
            string cur_freq_string = cur_freq.ToString();

            //update text
            Dispatcher.Invoke(new Action(() => {

                if (mWaitingDialog != null)
                {
                    mWaitingDialog.Frequency = mCurrentFrequency;
                }

            }));

            //start with function generator
            if (WriteVISACommand(mFunctionGenerator, "SOURCE1:FREQUENCY " + cur_freq_string))
            {
                Thread.Sleep(mPhonometerLag); 

                while (true)
                {
                    //read N

                    int ret = 0;
                    //mutex
                    lock (mPhonoLock)
                    {
                        status = new PhonometerDisplay.PhonometerStatus();

                        //sample phonometer and current
                        if (!mTimerStop)
                        {
                            //capture phono
                            if ( mPhonometerCapture ) ret = ReadPhonometer(status);

                            //capture oscilloscope
                            if ( mOscilloscopeCapture )
                            {
                                try
                                {
                                    string peak_res_string = mOscilloscope.Query(mVoltageSampleCommand);
                                    string[] split_resp = peak_res_string.Split(',');
                                    //if there are no errors, the value is on the 2nd token

                                    string tmp = split_resp[1].TrimEnd('V').TrimEnd(' ');
                                    //milliamperes
                                    peak_value = 1000.0 * double.Parse(tmp, CultureInfo.InvariantCulture) / (2*mLoadImpedance);
                                }
                                catch
                                {
                                    exit = true;
                                }
                            }
                            
                        }
                        else exit = true;
                        
                    }

                    if (exit) return; //should quit

                    if (ret == 0)
                    {
                        //ok
                        average_spl +=  status.Spl / n_readings;
                        average_cur += peak_value / n_readings;

                        Dispatcher.Invoke(new Action(() => {

                            if (mWaitingDialog != null)
                            {
                                if ( mPhonometerCapture ) mWaitingDialog.PhonometerStatus = status;
                            }
                            
                            status = null;

                        }));
                        

                        if (total_read-- == 0) break;

                    }
                    else if (ret == -1 || ret == -2 || ret == -3 )
                    {
                        Dispatcher.Invoke(new Action(() => {

                            MessageBox.Show("Impossibile leggere da fonometro...");

                            WriteVISACommand(mFunctionGenerator, "OUTPUT1 OFF");
                            //mVisaController.Close();
                            //mVisaController = null;

                            if ( mWaitingDialog != null ) mWaitingDialog.Close();

                        }));
                        
                        return;
                    }
                    
                }
                
                //we have a sample, add to current data
                if (mPhonometerCapture)
                {
                    DataPoint point = new DataPoint(mCurrentFrequency, average_spl);
                    mCurrentData.Add(point);
                    
                }

                if ( mOscilloscopeCapture )
                {
                    DataPoint point = new DataPoint(mCurrentFrequency, average_cur);
                    mOscilloscopeData.Add(point);

                    Dispatcher.Invoke(new Action(() => {

                        if (mWaitingDialog != null)
                        {
                            if (mOscilloscopeCapture) mWaitingDialog.Current = average_cur;
                        }

                        status = null;

                    }));

                }
                

                //check if finished sweep
                if ( mCurrentFrequency >= mStopFrequency )
                {
                    Dispatcher.Invoke(new Action(() => {
                        
                        if ( mWaitingDialog != null ) mWaitingDialog.Close();

                    }));

                    return;
                }

                //update frequency
                mCurrentFrequency += mFrequencyStep;
                

                //restart timer
                mSamplingTimer.Start();

                return;
            }
            else
            {
                Dispatcher.Invoke(new Action(() => {

                    MessageBox.Show("Impossibile leggere da fonometro...");

                    WriteVISACommand(mFunctionGenerator, "OUTPUT1 OFF");
                    //mVisaController.Close();
                    //mVisaController = null;

                    if ( mWaitingDialog != null ) mWaitingDialog.Close();

                }));
            }

            
           
        }

        private void ToneManager_Loaded(object sender, RoutedEventArgs e)
        {
            //tone manager has only one child
            SweepSlot slot = new SweepSlot();
            slot.Manager = toneManager;
            toneManager.AddSlot(slot);
        }

        private void scanButton_Click(object sender, RoutedEventArgs e)
        {
            ScanInstruments();
        }


        private void ScanSerialPorts()
        {
            string[] comPorts = SerialPort.GetPortNames();

            comPortCombo.Items.Clear();


            foreach (string s in comPorts)
            {
                //add an entry to combobox
                comPortCombo.Items.Add(s);
            }

            if (comPorts.Length > 0)
                comPortCombo.SelectedIndex = 0;
        }

        private void ScanInstruments()
        {
            //create visa instance
            //mVisaController = new VISA();
          
            //if (mVisaController == null) return;

            instrumentsCombo.Items.Clear();

            //ArrayList resources = new ArrayList();

            //query all instruments
            //mVisaController.FindResources("?*", out resources);

            string[] local_resources = null;

            try
            {
                local_resources = ResourceManager.GetLocalManager().FindResources("?*");
            }
            catch ( VisaException visa_ex )
            {
                MessageBox.Show("Errore VISA: " + visa_ex.ErrorCode);
                return;
            }
            catch
            {
                MessageBox.Show("Errore di caricamento delle periferiche VISA!");
                return;
            }

            for ( int k = 0; k < local_resources.Length; k++ )
            {
                string instr = local_resources[k];

                VisaInstrumentItem item = new VisaInstrumentItem(instr);

                if ( item.ToString() != null )
                {
                    //add to combobox
                    instrumentsCombo.Items.Add(item);
                }
            }

            instrumentsCombo.SelectedIndex = 0;

            //mVisaController.Close();
            //mVisaController = null;
        }

        private bool WriteVISACommand ( MessageBasedSession instrument, string command )
        {
            if (instrument == null) return false;

            
            try
            {
                instrument.Write(Encoding.ASCII.GetBytes(command));
                return true;
            }
            catch (Exception excp)
            {
                MessageBox.Show("Errore di impostazione dello strumento!");
                
                return false;
            }

        }

        private void StartSweepButton_Click(object sender, RoutedEventArgs e)
        {
            //1. open serial port for phonometer
            //2. start timer to send command to instrument
            //3. open plot window

            mOscilloscopeCapture = (bool)oscilloscopeEnableCheck.IsChecked;
            mPhonometerCapture = (bool)phonoEnableCheck.IsChecked;

            if ( !mOscilloscopeCapture && !mPhonometerCapture )
            {
                MessageBox.Show("Nessuna misura selezionata!");
                return;
            }

            //calculate impedance

            try
            {
                double impedance = double.Parse(impedanceMeasureText.Text);
                //set impedance
                mLoadImpedance = mPlotOscilloscopeWindow.LoadImpedance = impedance;
                
            }
            catch
            {
                MessageBox.Show("Impedenza di carico inserita non valida!");
                return;
            }
            
            if ( probePosCombo.SelectedIndex == probeNegCombo.SelectedIndex || probeNegCombo.SelectedIndex < 0 || probePosCombo.SelectedIndex < 0 )
            {
                MessageBox.Show("Errore nella selezione delle sonde!");
                return;
            }

            if  ( mOscilloscope != null )
            {
                mOscilloscope.Dispose();
                mOscilloscope = null;
            }

            if ( mFunctionGenerator != null )
            {
                mFunctionGenerator.Dispose();
                mFunctionGenerator = null;
            }

            mCurrentData.Clear();
            mOscilloscopeData.Clear();

            SweepSlot slot = toneManager.Slots[0] as SweepSlot;

            mStartFrequency = slot.SweepStartFrequency;
            mStopFrequency = slot.SweepStopFrequency;
            mCurrentFrequency = mStartFrequency;

            //function generator
            try
            {
                VisaInstrumentItem visa_instrument = instrumentsCombo.SelectedItem as VisaInstrumentItem;

                mFunctionGenerator = (MessageBasedSession)ResourceManager.GetLocalManager().Open(visa_instrument.Resource);

                //reset
                mFunctionGenerator.Write("*RST");
            }
            catch
            {
                MessageBox.Show("Impossibile avviare la sessione VISA con il generatore di funzioni selezionato!");
                return;
            }

            //oscilloscope
            try
            {
                mOscilloscope = (MessageBasedSession)ResourceManager.GetLocalManager().Open(mOscilloscopeResource);

                mOscilloscope.Query("*STB?");
            }
            catch
            {
                MessageBox.Show("Impossibile avviare la sessione VISA con l'oscilloscopio selezionato!");
                return;
            }

            ////
            //configure function generator
            ///

            //select sinusoid wave
            if (!WriteVISACommand(mFunctionGenerator, "SOURCE1:FUNCTION:SHAPE SINUSOID")) return;

            //set frequency mode to CW
            if (!WriteVISACommand(mFunctionGenerator, "SOURCE1:FREQUENCY:MODE CW")) return;
            
            //set current frequency to low
            if (!WriteVISACommand(mFunctionGenerator, "SOURCE1:FREQUENCY 100")) return;

            //set amplitude
            int amplitude = amplitudeCombo.SelectedIndex + 1;
            string command = "SOURCE1:VOLTAGE:LEVEL:IMMEDIATE " + amplitude + "Vpp";

            if (!WriteVISACommand(mFunctionGenerator, command)) return;

            /////
            //configure oscilloscope
            ///

            
            string probePos = "C" + (probePosCombo.SelectedIndex + 1).ToString();
            string probeNeg = "C" + (probeNegCombo.SelectedIndex + 1).ToString();

            //show both 
            if (!WriteVISACommand(mOscilloscope, "C1:TRACE OFF")) return;
            if (!WriteVISACommand(mOscilloscope, "C2:TRACE OFF")) return;
            if (!WriteVISACommand(mOscilloscope, "C3:TRACE OFF")) return;
            if (!WriteVISACommand(mOscilloscope, "C4:TRACE OFF")) return;
            if (!WriteVISACommand(mOscilloscope, "F1:TRACE OFF")) return;

            if (!WriteVISACommand(mOscilloscope, probePos + ":TRACE ON")) return;
            if (!WriteVISACommand(mOscilloscope, probeNeg + ":TRACE ON")) return;

            //set trigger select
            string trigger_select = "TRIG_SELECT EDGE,SR," + probePos + ",HT,OFF";
            if (!WriteVISACommand(mOscilloscope, trigger_select)) return;

            //set trigger coupling
            string trigger_coupl = probePos + ":TRIG_COUPLING DC";
            if (!WriteVISACommand(mOscilloscope, trigger_coupl)) return;

            //set trigger level
            string trigger_level = probePos + ":TRIG_LEVEL 0V";
            if (!WriteVISACommand(mOscilloscope, trigger_level)) return;

            //set trigger mode
            string trigger_mode = probePos + ":TRIG_MODE NORMAL";
            if (!WriteVISACommand(mOscilloscope, trigger_mode)) return;

            //set voltage division
            string volt_div_pos = probePos + ":VOLT_DIV 2V";
            string volt_div_neg = probeNeg + ":VOLT_DIV 2V";
            if (!WriteVISACommand(mOscilloscope, volt_div_pos)) return;
            if (!WriteVISACommand(mOscilloscope, volt_div_neg)) return;

            //set time division
            string time_div = "TIME_DIV 0.001";   //1ms could be ok
            if (!WriteVISACommand(mOscilloscope, time_div)) return;
           
            //define math function
            string function_define = "F1:DEFINE EQN,'" + probePos + "-" + probeNeg + "'";
            //show math track
            //if (!WriteVISACommand(mOscilloscope, "F1:TRACE ON")) return;

            //cache command for oscilloscope
            mVoltageSampleCommand = "F1:PAVA? PKPK"; //peak to peak measure
            
            ///////////activate instrument
            ////
            if (!WriteVISACommand(mFunctionGenerator, "OUTPUT1 ON")) return;

            //show a dialog to stop timer
            mSelectedComPort = comPortCombo.SelectedItem as string;

            //open phonometer
            if ( mPhonometerCapture ) OpenPhonometer(mSelectedComPort);

            mTimerStop = false;

            //set lag and freq step
            mFrequencyStep = (freqStepCombo.SelectedIndex + 1) * 10;
            mPhonometerLag = (phonoLagCombo.SelectedIndex + 1) * 500;

            //if phonometer not active we can work faster
            if (!mPhonometerCapture) mPhonometerLag = 200; 

            mSamplingTimer.Start();

            mWaitingDialog = new WaitingDialog();
            mWaitingDialog.Owner = this;
            mWaitingDialog.Controller = this;

            mWaitingDialog.ShowDialog();

            //when it returns, it should stop sweep
            mSamplingTimer.Enabled = false;

            //wait till timer thread over
            lock(mPhonoLock)
            {
                //signal to timer thread
                //c# System.Timers.Timer generate calls on a free thread on threadpool
                //we used a self restart thread so we wont have more thread in parallels but only one at once
                mTimerStop = true;
            }


            //update plot

            SessionInformation info = new SessionInformation();
            info.Name = nameText.Text;
            info.Date = (DateTime)(sessionDate.SelectedDate == null ? DateTime.Now : sessionDate.SelectedDate);
            info.Buzzer = buzzerText.Text;
            info.Description = descriptionText.Text;

            mPlotModel.NewSession(info, mCurrentData, mOscilloscopeData);
            mPlotWindow.Show();
            
            
            mWaitingDialog = null;

            //close phonometer
            ClosePhonometer();

            //stop instrument output
            if (!WriteVISACommand(mFunctionGenerator, "OUTPUT1 OFF")) return;

            if (!WriteVISACommand(mOscilloscope, "C1:TRACE OFF")) return;
            if (!WriteVISACommand(mOscilloscope, "C2:TRACE OFF")) return;
            if (!WriteVISACommand(mOscilloscope, "C3:TRACE OFF")) return;
            if (!WriteVISACommand(mOscilloscope, "C4:TRACE OFF")) return;
            if (!WriteVISACommand(mOscilloscope, "F1:TRACE OFF")) return;

            //close session
            try
            {
                mFunctionGenerator.Dispose();
                mFunctionGenerator = null;

                mOscilloscope.Dispose();
                mOscilloscope = null;
            }
            catch
            {

            }

            //mVisaController.Close();
            //mVisaController = null;
        }

        protected bool OpenPhonometer(string comName)
        {
            
            if ( comName == null && mPhonometerCapture )
            {
                MessageBox.Show("Nessuna porta seriale selezionata!");
                return false;
            }

            try
            {
                mSerialPort.DtrEnable = true;
                mSerialPort.BaudRate = 9600;
                mSerialPort.Parity = System.IO.Ports.Parity.None;
                mSerialPort.PortName = comName;
                
                mSerialPort.Open();
            }
            catch (Exception excp)
            {

                try
                {
                    if (mSerialPort != null && mSerialPort.IsOpen ) mSerialPort.Close();
                }
                catch
                {

                }

                MessageBox.Show("Problema con l'apertura della porta!");
                return false;
            }

            return true;
            
        }

        protected void ClosePhonometer()
        {
            try
            {
                mSerialPort.Close();
            }
            catch
            {
                
            }
            
        }

        //0: ok
        //-1: closed port
        //-2: write timeout
        //-3: read timeout
        
        protected int ReadPhonometer(PhonometerDisplay.PhonometerStatus status)
        {
            byte[] backdata = new byte[100];
            byte[] txdata = new byte[1];
            int rx_position = 0;
            int rxed = 0;
           
            if (!mSerialPort.IsOpen) return -1;
            
            mSerialPort.DiscardInBuffer();
            
            mSerialPort.ReadTimeout = 3000; //timeout reading
            mSerialPort.WriteTimeout = 1000; //timeout writing

            //send command
            txdata[0] = 0x20; //command to read data
            try
            {
                if (mSerialPort.IsOpen) mSerialPort.Write(txdata, 0, 1);
                else return -1;
            }
            catch ( System.ServiceProcess.TimeoutException exc )
            {
                //writing timeout, exit
                return -2;
            }
            
            mSerialPort.DiscardInBuffer();
            
            try
            {
                //read data
                while (true)
                {
                    rxed += mSerialPort.Read(backdata, rx_position, 1);
                    rx_position++;

                    if (rxed >= 5) break;
                }
            }
            catch (System.TimeoutException exc)
            {
                //serial port has not read 5 bytes in 2 second, exit
                return -3;
            }

            PhonometerDisplay.PhonometerStatus.FillStatus(status, backdata);
            
            return 0; //ok
            
        }

        private void rescanButton_Click(object sender, RoutedEventArgs e)
        {
            ScanSerialPorts();
        }

        private void testComButton_Click(object sender, RoutedEventArgs e)
        {
            //check selected port if answer
            string comport = comPortCombo.SelectedItem as string;

            if ( OpenPhonometer(comport) )
            {
                //try to read data
                PhonometerDisplay.PhonometerStatus status = new PhonometerDisplay.PhonometerStatus();

                int ret = ReadPhonometer(status);

                if (ret == 0)
                {
                    phonoDisplay.Status = status;
                    ClosePhonometer();
                }
                else MessageBox.Show("Timeout porta " + comport + "!");

            }
            else
            {
                string error = "Errore in apertura porta ";

                //error
                if (comport != null) error += comport;

                MessageBox.Show(error);
            }


        }

        private void plotButton_Click(object sender, RoutedEventArgs e)
        {
            mPlotWindow.Show();
        }

        private void plotOscilloscopeButton_Click(object sender, RoutedEventArgs e)
        {
            mPlotWindow.Show();
        }

        private void oscilloscopeCheckButton_Click(object sender, RoutedEventArgs e)
        {
            //check ip
            mOscilloscopeResource = null;

            //calculate from ip address
            //check if it is an ip address
            IPAddress ipOscilloscope = null;

            try
            {
                ipOscilloscope = IPAddress.Parse(ipOscilloscopeText.Text);
            }
            catch
            {
                MessageBox.Show("Indirizzo IP non valido!");
                return;
            }

            //string is a valid ip address
            mOscilloscopeResource = "TCPIP0::" + ipOscilloscope.ToString() + "::inst0::INSTR";

            //try to open a session and identify
            try
            {
                MessageBasedSession s = (MessageBasedSession)ResourceManager.GetLocalManager().Open(mOscilloscopeResource);

                string identifier = s.Query("*IDN?");

                MessageBox.Show("Strumento identificato: " + identifier);

                s.Query("*STB?");
            }
            catch
            {
                MessageBox.Show("Errore di connessione allo strumento, riprovare!");
                return;
            }

        }

        private void checkUsbButton_Click(object sender, RoutedEventArgs e)
        {
            if (mFunctionGenerator != null) mFunctionGenerator.Dispose();
            mFunctionGenerator = null;

            //function generator
            VisaInstrumentItem visa_instrument = null;

            try
            {
                visa_instrument = instrumentsCombo.SelectedItem as VisaInstrumentItem;

                MessageBasedSession s = (MessageBasedSession)ResourceManager.GetLocalManager().Open(visa_instrument.Resource);

                //reset
                string identifier = s.Query("*IDN?");
                s.Query("*STB?");

                MessageBox.Show("Strumento identificato: " + identifier);

                if (s != null) s.Dispose();

                return;
            }
            catch
            {
                MessageBox.Show("Errore nell'identificazione dello strumento selezionato!");
                return;
            }
            
        }
        
    }
}
