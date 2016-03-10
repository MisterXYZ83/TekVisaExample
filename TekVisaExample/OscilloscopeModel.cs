using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TekVisaExample
{
    public class OscilloscopeModel
    {
        protected PlotModel mModel;
        protected OscilloscopeWindow mPlotWindow;

        public OscilloscopeModel()
        {
            mModel = new PlotModel();
        }


        public OscilloscopeWindow PlotWindow
        {
            get
            {
                return mPlotWindow;
            }

            set
            {
                mPlotWindow = value;
            }
        }


        public PlotModel PlotModel
        {
            get
            {
                return mModel;
            }
        }

        public LineSeries DrawSpectrum ( List<DataPoint> points )
        {
            LineSeries s = new LineSeries();

            LinearAxis freq_axis = new LinearAxis();
            freq_axis.Position = AxisPosition.Bottom;
            freq_axis.Minimum = points[0].X;
            freq_axis.Maximum = points[points.Count-1].X;
            freq_axis.Title = "Frequenza";
            freq_axis.Unit = "Hz";

            double min_v = points.Min<DataPoint>(new Func<DataPoint, double>(val => val.Y));
            double max_v = points.Max<DataPoint>(new Func<DataPoint, double>(val => val.Y));

            LinearAxis v_axis = new LinearAxis();
            v_axis.Position = AxisPosition.Left;
            v_axis.Minimum = min_v;
            v_axis.Maximum = max_v;
            v_axis.Title = "Ampiezza";
            v_axis.Unit = "A";

            mModel.Axes.Add(freq_axis);
            mModel.Axes.Add(v_axis);

            freq_axis.MajorGridlineStyle = LineStyle.Solid;
            freq_axis.MinorGridlineStyle = LineStyle.Dot;

            v_axis.MajorGridlineStyle = LineStyle.Solid;
            v_axis.MinorGridlineStyle = LineStyle.Dot;

            s.TrackerFormatString = "{0}\n{1}: {2:0.00}Hz\n{3}: {4:0.00}A";

            mModel.Series.Add(s);

            mModel.InvalidatePlot(true);

            return s;

        }

        //draw a waveform with descriptor
        /*public void DrawWaveform(WaveFormDescriptor descr)
        {
            //create series 
            LineSeries s = new LineSeries();

            double[] time = descr.Time;
            double[] amplitude = descr.Amplitude;

            int n_points = time.Length;

            for (int k = 0; k < n_points-1; k++)
            {
                s.Points.Add(new DataPoint(time[k], amplitude[k]));
            }

            double min_t = time[0];
            double max_t = time[time.Length - 2];

            double min_v = descr.MinValue;
            double max_v = descr.MaxValue;

            LinearAxis time_axis = null;
            LinearAxis v_axis = null;

            time_axis = new LinearAxis();
            time_axis.Position = AxisPosition.Bottom;
            time_axis.Minimum = min_t;
            time_axis.Maximum = max_t;
            time_axis.Title = "Tempo";
            time_axis.Unit = "s";

            v_axis = new LinearAxis();
            v_axis.Position = AxisPosition.Left;
            v_axis.Minimum = min_v;
            v_axis.Maximum = max_v;
            v_axis.Title = "Ampiezza";
            v_axis.Unit = "V";

            mModel.Axes.Add(time_axis);
            mModel.Axes.Add(v_axis);

            time_axis.MajorGridlineStyle = LineStyle.Solid;
            time_axis.MinorGridlineStyle = LineStyle.Dot;

            v_axis.MajorGridlineStyle = LineStyle.Solid;
            v_axis.MinorGridlineStyle = LineStyle.Dot;

            s.TrackerFormatString = "{0}\n{1}: {2:0.00}s\n{3}: {4:0.00}V";

            mModel.Series.Add(s);

            mModel.InvalidatePlot(true);
        }*/

        public void RemoveSeries(LineSeries s)
        {
            if (s != null)
            {
                if (mModel.Series.Contains(s))
                {
                    mModel.Series.Remove(s);

                    mModel.Axes.Remove(s.XAxis);
                    mModel.Axes.Remove(s.YAxis);

                    mModel.InvalidatePlot(true);
                }
            }
        }
    }
}
