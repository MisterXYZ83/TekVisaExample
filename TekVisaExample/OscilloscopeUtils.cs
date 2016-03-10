using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TekVisaExample
{
    public class WaveFormDescriptor
    {

        protected double[] mTime;
        protected double[] mAmplitude;

        protected int mTotalByteSize;

        //these are in the block list, but i want to cache them to have easier access
        protected int mWaveArray1ByteSize;
        protected int mWaveArray2ByteSize;
        protected int mWaveArrayCount;

        protected int mFirstValidPoint;
        protected int mLastValidPoint;

        protected double mHorizontalOffset;
        protected float mHorizontalInterval; //sampling time
        protected float mMinValue;
        protected float mMaxValue;
        protected float mVerticalGain;
        protected float mVerticalOffset;
         
        protected List<WaveformDataBlock> mBlocks;
        protected int mDataSize; //size of samples
        protected bool mLowFirst;

        public double MaxValue
        {
            get
            {
                return mVerticalGain * mMaxValue;
            }

        }

        public double MinValue
        {
            get
            {
                return mVerticalGain * mMinValue;
            }
        }

        public double[] Amplitude
        {
            get
            {
                return mAmplitude;
            }
        }

        public double[] Time
        {
            get
            {
                return mTime;
            }

        }

        public static bool WaveFormDescriptorFactory(byte[] rawdata, out WaveFormDescriptor waveform)
        {
            bool ret = false;
            int pos = 0;
            string str = null;

            waveform = new WaveFormDescriptor();

            WaveformDataBlock block = null;

            waveform.mBlocks = new List<WaveformDataBlock>();
            List<WaveformDataBlock> list = waveform.mBlocks; //non voglio riscrivere tutto! :D


            bool has_usertext_block = false;
            bool has_trigtime_block = false;
            bool has_ristime_block = false;
            bool has_data1_block = false;
            bool has_data2_block = false;
            bool has_simple_block = false;
            bool has_dual_block = false;
            bool has_resdesc1_block = false;
            bool has_resarray_block = false;
            bool has_wavearray1_block = false;
            bool has_wavearray2_block = false;
            bool has_resarray2_block = false;
            bool has_resarray3_block = false;

            bool low_first = true;

            try
            {
                //oscilloscope will answer with a preamble
                //discard data till we have '#' char
                
                while (true)
                {
                    if (rawdata[pos++] == '#') break;
                }

                //we have the 9+1 byte header GPIB
                //now pos points to the first char of size
                pos++;
                string total_string = Encoding.ASCII.GetString(rawdata, pos, 9); pos += 9;
                waveform.mTotalByteSize = int.Parse(total_string);

                ///0: DESCRIPTOR_NAME: string

                str = Encoding.ASCII.GetString(rawdata, pos, WaveformDataBlock.STRING_LEN);
                if (string.Compare(str, "WAVEDESC") != 0)
                {
                    list.Clear();
                    list = null;
                    waveform = null;
                    return false;
                }
                pos += WaveformDataBlock.CreateBlock(rawdata, "DESCRIPTOR_NAME", "STRING", pos, out block);
                list.Add(block);

                str = Encoding.ASCII.GetString(rawdata, pos, WaveformDataBlock.STRING_LEN);
                if (string.Compare(str, "LECROY_2_3") != 0)
                {
                    list.Clear();
                    list = null;
                    waveform = null;
                    return false;
                }
                pos += WaveformDataBlock.CreateBlock(rawdata, "TEMPLATE_NAME", "STRING", pos, out block);
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "COMM_TYPE", "ENUM", pos, out block);
                list.Add(block);

                if ( block.Value != null )
                {
                    int val = ((Nullable<int>)block.Value).Value;

                    //could be more values, for now only byte and word;
                    if (val == 0) waveform.mDataSize = 1;
                    else if (val == 1) waveform.mDataSize = 2;
                    else
                    {
                        list.Clear();
                        list = null;
                        waveform = null;
                        return false;
                    }
                }

                pos += WaveformDataBlock.CreateBlock(rawdata, "COMM_ORDER", "ENUM", pos, out block);
                list.Add(block);

                if (block.Value != null)
                {
                    //1 => low first
                    //0 => high first
                    Nullable<int> b_val = block.Value as Nullable<int>;

                    waveform.mLowFirst = low_first = (b_val.Value == 1);

                }
                
                //valid, start parse

                ///////////////////////////
                //WAVEDESCRIPTOR

                pos += WaveformDataBlock.CreateBlock(rawdata, "WAVE_DESCRIPTOR", "LONG", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "USER_TEXT", "LONG", pos, out block); block.LowFirst = low_first;
                list.Add(block);
                has_usertext_block = ((((Nullable<int>)block.Value)).Value != 0);

                pos += WaveformDataBlock.CreateBlock(rawdata, "RES_DESC1", "LONG", pos, out block); block.LowFirst = low_first;
                list.Add(block);
                has_resdesc1_block = ((((Nullable<int>)block.Value)).Value != 0);

                ///////////////////////////
                //Arrays

                pos += WaveformDataBlock.CreateBlock(rawdata, "TRIGTIME_ARRAY", "LONG", pos, out block); block.LowFirst = low_first;
                list.Add(block);
                has_trigtime_block = ((((Nullable<int>)block.Value)).Value != 0);

                pos += WaveformDataBlock.CreateBlock(rawdata, "RIS_TIME_ARRAY", "LONG", pos, out block); block.LowFirst = low_first;
                list.Add(block);
                has_ristime_block = ((((Nullable<int>)block.Value)).Value != 0);

                pos += WaveformDataBlock.CreateBlock(rawdata, "RES_ARRAY", "LONG", pos, out block); block.LowFirst = low_first;
                list.Add(block);
                has_resarray_block = ((((Nullable<int>)block.Value)).Value != 0);

                pos += WaveformDataBlock.CreateBlock(rawdata, "WAVE_ARRAY_1", "LONG", pos, out block); block.LowFirst = low_first;
                list.Add(block);
                has_wavearray1_block = ((((Nullable<int>)block.Value)).Value != 0);

                pos += WaveformDataBlock.CreateBlock(rawdata, "WAVE_ARRAY_2", "LONG", pos, out block); block.LowFirst = low_first;
                list.Add(block);
                waveform.mWaveArray1ByteSize = (((Nullable<int>)block.Value)).Value;
                has_wavearray2_block = (waveform.mWaveArray1ByteSize != 0);
                

                pos += WaveformDataBlock.CreateBlock(rawdata, "RES_ARRAY_2", "LONG", pos, out block); block.LowFirst = low_first;
                list.Add(block);
                waveform.mWaveArray2ByteSize = (((Nullable<int>)block.Value)).Value;
                has_resarray2_block = (waveform.mWaveArray2ByteSize != 0);

                pos += WaveformDataBlock.CreateBlock(rawdata, "RES_ARRAY_3", "LONG", pos, out block); block.LowFirst = low_first;
                list.Add(block);
                has_resarray3_block = ((((Nullable<int>)block.Value)).Value != 0);

                ///////////////////////////////
                //instrument description
                pos += WaveformDataBlock.CreateBlock(rawdata, "INSTRUMENT_NAME", "STRING", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "INSTRUMENT_NUMBER", "LONG", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "TRACE_LABEL", "STRING", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "RESERVED1", "WORD", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "RESERVED2", "WORD", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "WAVE_ARRAY_COUNT", "LONG", pos, out block); block.LowFirst = low_first;
                list.Add(block);
                waveform.mWaveArrayCount = (((Nullable<int>)block.Value)).Value;

                pos += WaveformDataBlock.CreateBlock(rawdata, "PNTS_PER_SCREEN", "LONG", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "FIRST_VALID_POINT", "LONG", pos, out block); block.LowFirst = low_first;
                list.Add(block);
                waveform.mFirstValidPoint = (((Nullable<int>)block.Value)).Value;

                pos += WaveformDataBlock.CreateBlock(rawdata, "LAST_VALID_POINT", "LONG", pos, out block); block.LowFirst = low_first;
                list.Add(block);
                waveform.mLastValidPoint = (((Nullable<int>)block.Value)).Value;

                pos += WaveformDataBlock.CreateBlock(rawdata, "FIRST_POINT", "LONG", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "SPARSING_FACTOR", "LONG", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "SEGMENT_INDEX", "LONG", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "SUBARRAY_COUNT", "LONG", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "SWEEPS_PER_ACQ", "LONG", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "POINTS_PER_PAIR", "WORD", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "PAIR_OFFSET", "WORD", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "VERTICAL_GAIN", "FLOAT", pos, out block); block.LowFirst = low_first;
                list.Add(block);
                waveform.mVerticalGain = (((Nullable<float>)block.Value)).Value;

                pos += WaveformDataBlock.CreateBlock(rawdata, "VERTICAL_OFFSET", "FLOAT", pos, out block); block.LowFirst = low_first;
                list.Add(block);
                waveform.mVerticalOffset = (((Nullable<float>)block.Value)).Value;

                pos += WaveformDataBlock.CreateBlock(rawdata, "MAX_VALUE", "FLOAT", pos, out block); block.LowFirst = low_first;
                list.Add(block);
                waveform.mMaxValue = (((Nullable<float>)block.Value)).Value;

                pos += WaveformDataBlock.CreateBlock(rawdata, "MIN_VALUE", "FLOAT", pos, out block); block.LowFirst = low_first;
                list.Add(block);
                waveform.mMinValue = (((Nullable<float>)block.Value)).Value;

                pos += WaveformDataBlock.CreateBlock(rawdata, "NOMINAL_BITS", "WORD", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "NOM_SUBARRAY_COUNT", "WORD", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "HORIZ_INTERVAL", "FLOAT", pos, out block); block.LowFirst = low_first;
                list.Add(block);
                waveform.mHorizontalInterval = (((Nullable<float>)block.Value)).Value;

                pos += WaveformDataBlock.CreateBlock(rawdata, "HORIZ_OFFSET", "DOUBLE", pos, out block); block.LowFirst = low_first;
                list.Add(block);
                waveform.mHorizontalOffset = (((Nullable<double>)block.Value)).Value;

                pos += WaveformDataBlock.CreateBlock(rawdata, "PIXEL_OFFSET", "DOUBLE", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "VERTUNIT", "UNIT", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "HORUNIT", "UNIT", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "HORIZ_UNCERTAINTY", "FLOAT", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "TRIGGER_TIME", "TIMESTAMP", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "ACQ_DURATION", "FLOAT", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "RECORD_TYPE", "ENUM", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "PROCESSING_DONE", "ENUM", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "RESERVED5", "WORD", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "RIS_SWEEPS", "WORD", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "TIMEBASE", "ENUM", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "VERT_COUPLING", "ENUM", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "PROBE_ATT", "FLOAT", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "FIXED_VERT_GAIN", "ENUM", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "BANDWIDTH_LIMIT", "ENUM", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "VERTICAL_VARNIER", "FLOAT", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "ACQ_VERT_OFFSET", "FLOAT", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                pos += WaveformDataBlock.CreateBlock(rawdata, "WAVE_SOURCE", "ENUM", pos, out block); block.LowFirst = low_first;
                list.Add(block);

                /////////////////////////////
                //USERTEXT
                if (has_usertext_block)
                {
                    pos += WaveformDataBlock.CreateBlock(rawdata, "TEXT", "TEXT", pos, out block); block.LowFirst = low_first;
                    list.Add(block);
                }

                /////////////////////////
                ///TRIGTIME
                if (has_trigtime_block)
                {
                    pos += WaveformDataBlock.CreateBlock(rawdata, "TRIGGER_TIME", "DOUBLE", pos, out block); block.LowFirst = low_first;
                    list.Add(block);

                    pos += WaveformDataBlock.CreateBlock(rawdata, "TRIGGER_OFFSET", "DOUBLE", pos, out block); block.LowFirst = low_first;
                    list.Add(block);
                }

                ////////////////////////
                //RIS
                if (has_ristime_block)
                {
                    pos += WaveformDataBlock.CreateBlock(rawdata, "RIS_OFFSET", "DOUBLE", pos, out block); block.LowFirst = low_first;
                    list.Add(block);
                }

                ///////////////////////////
                //WAVE 1
                if (has_wavearray1_block)
                {
                    WaveformDataBlock.CreateBlock(rawdata, "MEASUREMENT", "DATA", pos, out block);
                    list.Add(block);

                    waveform.mTime = new double[waveform.mWaveArrayCount];
                    waveform.mAmplitude = new double[waveform.mWaveArrayCount];

                    //data size is on COMM_TYPE block, cached
                    for ( int idx_sample = waveform.mFirstValidPoint; idx_sample < waveform.mLastValidPoint; idx_sample++ )
                    {
                        //calculate time sample
                        waveform.mTime[idx_sample] = waveform.mHorizontalOffset + waveform.mHorizontalInterval * idx_sample;
                        //calculate amplitude
                        int adc_sample_c2 = 0;

                        if (waveform.mDataSize == 1)
                        {
                            //convert 1 byte unsigned to 2'compl int
                            adc_sample_c2 = (sbyte)rawdata[pos + waveform.mDataSize * idx_sample];
                            
                        }
                        else
                        {
                            byte[] sam = new byte[2];
                            Array.Copy(rawdata, pos + waveform.mDataSize * idx_sample, sam, 0, 2);

                            if (!waveform.mLowFirst) Array.Reverse(sam);

                            adc_sample_c2 = BitConverter.ToInt16(sam, 0);

                            sam = null; 
                            
                        }

                        waveform.mAmplitude[idx_sample] = -waveform.mVerticalOffset + waveform.mVerticalGain * adc_sample_c2;
                    }
                }

                ///////////////////////////
                //WAVE 2
                if (has_wavearray2_block)
                {
                    pos += WaveformDataBlock.CreateBlock(rawdata, "MEASUREMENT", "DATA", pos, out block);
                    list.Add(block);
                }

            }
            catch
            {
                list.Clear();
                list = null;
                waveform.mTime = null;
                waveform.mAmplitude = null;
                waveform = null;
                return false;
            }


            return true;
        }
    }

    public class WaveformDataBlock
    {
        public string Name;
        public string Type;
        public int Length;
        public byte [] RawData;
        public bool LowFirst;

        public static int STRING_LEN = 16;
        public static int ENUM_LEN = 2;
        public static int BYTE_LEN = 1;
        public static int WORD_LEN = 2;
        public static int DOUBLE_LEN = 8;
        public static int FLOAT_LEN = 4;
        public static int TIMESTAMP_LEN = 16;
        public static int TEXT_LEN = 160;
        public static int UNIT_LEN = 48;
        public static int LONG_LEN = 4;

        public static int CreateBlock (byte[] rawdata, string name, string type, int pos, out WaveformDataBlock block)
        {

            if (string.Compare(type, "STRING", true) == 0) block = new StringWaveformDataBlock();
            else if (string.Compare(type, "ENUM", true) == 0) block = new EnumWaveformDataBlock();
            else if (string.Compare(type, "BYTE", true) == 0) block = new ByteWaveformDataBlock();
            else if (string.Compare(type, "WORD", true) == 0) block = new WordWaveformDataBlock();
            else if (string.Compare(type, "LONG", true) == 0) block = new LongWaveformDataBlock();
            else if (string.Compare(type, "DOUBLE", true) == 0) block = new DoubleWaveformDataBlock();
            else if (string.Compare(type, "FLOAT", true) == 0) block = new FloatWaveformDataBlock();
            else if (string.Compare(type, "TIMESTAMP", true) == 0) block = new TimeStampWaveformDataBlock();
            else if (string.Compare(type, "TEXT", true) == 0) block = new TextWaveformDataBlock();
            else if (string.Compare(type, "UNIT", true) == 0) block = new UnitWaveformDataBlock();
            else if (string.Compare(type, "DATA", true) == 0) block = new DataWaveformDataBlock();
            else block = new WaveformDataBlock();

            block.Name = string.Copy(name);
            block.LowFirst = true;

            //datablock is not fixed, should be set when receive a new waveform descriptor
            if (block.Length != 0)
            {
                block.RawData = new byte[block.Length];
                Array.Copy(rawdata, pos, block.RawData, 0, block.Length);
            }

            return block.Length;
        }


        public virtual object Value
        {
            get
            {
                return null;
            }
        }
    }
        

    public class StringWaveformDataBlock : WaveformDataBlock
    {

        public StringWaveformDataBlock()
        {
            Length = STRING_LEN;
            Type = "STRING";
        }

        public override object Value
        {
            get
            {
                return Encoding.ASCII.GetString(RawData);
            }
        }
    }

    public class WordWaveformDataBlock : WaveformDataBlock
    {

        public WordWaveformDataBlock()
        {
            Length = WORD_LEN;
            Type = "WORD";
        }

        public override object Value
        {
            get
            {
                int val = 0;

                if (LowFirst)
                {
                    val = RawData[0] + RawData[1] * 256;
                }
                else
                {
                    val = RawData[1] * RawData[0] * 256;
                }

                return new Nullable<int>(val);
            }
        }
    }

    public class LongWaveformDataBlock : WaveformDataBlock
    {

        public LongWaveformDataBlock()
        {
            Type = "LONG";
            Length = LONG_LEN;
        }

        public override object Value
        {
            get
            {
                int val = 0;

                if (LowFirst)
                {
                    val = RawData[0] + RawData[1] * 256 + RawData[2] * 256 * 256 + RawData[3] * 256 * 256 * 256;
                }
                else
                {
                    val = RawData[3] + RawData[2] * 256 + RawData[1] * 256 * 256 + RawData[0] * 256 * 256 * 256;
                }

                return new Nullable<int>(val);
            }
        }
    }


    public class ByteWaveformDataBlock : WaveformDataBlock
    {
        public ByteWaveformDataBlock()
        {
            Type = "BYTE";
            Length = BYTE_LEN;
        }

        public override object Value
        {
            get
            {
                return RawData[0];
            }
        }
    }

    public class EnumWaveformDataBlock : WaveformDataBlock
    {
        public EnumWaveformDataBlock()
        {
            Type = "ENUM";
            Length = ENUM_LEN;
        }

        public override object Value
        {
            get
            {
                int val = 0;

                if (LowFirst)
                {
                    val = RawData[0] + RawData[1] * 256;
                }
                else
                {
                    val = RawData[1] * RawData[0] * 256;
                }

                return new Nullable<int>(val);
            }
        }
    }

    public class DoubleWaveformDataBlock : WaveformDataBlock
    {
        public DoubleWaveformDataBlock()
        {
            Type = "DOUBLE";
            Length = DOUBLE_LEN;
        }

        public override object Value
        {
            get
            {
                double val = 0.0;
                byte[] tmp = new byte[8];

                Array.Copy(RawData, tmp, 8);

                if (!LowFirst)
                {
                    Array.Reverse(tmp);
                }

                val = BitConverter.ToDouble(tmp, 0);

                return new Nullable<double>(val);
            }
        }
    }

    public class FloatWaveformDataBlock : WaveformDataBlock
    {
        public FloatWaveformDataBlock()
        {
            Type = "FLOAT";
            Length = LONG_LEN;
        }

        public override object Value
        {
            get
            {
                float val = 0.0f;
                byte[] tmp = new byte[4];

                Array.Copy(RawData, tmp, 4);

                if (!LowFirst)
                {
                    Array.Reverse(tmp);
                }

                val = BitConverter.ToSingle(tmp, 0);

                return new Nullable<float>(val);
            }
        }
    }

    public class TimeStampWaveformDataBlock : WaveformDataBlock
    {

        public class TimeStampWave
        {
            public double Seconds;
            public byte Minutes;
            public byte Hours;
            public byte Day;
            public byte Month;
            public UInt16 Year;
            public UInt16 Unused;
        }

        public TimeStampWaveformDataBlock()
        {
            Type = "TIMESTAMP";
            Length = TIMESTAMP_LEN;
        }

        public override object Value
        {
            get
            {
                //seconds 
                TimeStampWave tstamp = new TimeStampWave();

                tstamp.Seconds = BitConverter.ToDouble(RawData, 0);
                tstamp.Minutes = RawData[8];
                tstamp.Hours = RawData[9];
                tstamp.Day = RawData[10];
                tstamp.Month = RawData[11];
                tstamp.Year = BitConverter.ToUInt16(RawData, 12);
                tstamp.Unused = BitConverter.ToUInt16(RawData, 14);

                return tstamp;
            }
        }
    }

    public class TextWaveformDataBlock : WaveformDataBlock
    {
        public TextWaveformDataBlock()
        {
            Type = "TEXT";
            Length = TEXT_LEN;
        }
    }

    public class UnitWaveformDataBlock : WaveformDataBlock
    {
        public UnitWaveformDataBlock()
        {
            Type = "UNIT";
            Length = UNIT_LEN;
        }

        public override object Value
        {
            get
            {
                return Encoding.ASCII.GetString(RawData);
            }
        }
    }

    public class DataWaveformDataBlock : WaveformDataBlock
    {
        public DataWaveformDataBlock()
        {
            Type = "DATA";
            Length = 0;
        }
    }
}
