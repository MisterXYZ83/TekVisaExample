using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TekVisaExample
{
    
    public class CustomLineSeries : LineSeries
    {

        public override TrackerHitResult GetNearestPoint(ScreenPoint point, bool interpolate)
        {
            TrackerHitResult res = base.GetNearestPoint(point, interpolate);

            if ( res.Item is DataPoint )
            {

                DataPoint pt = (DataPoint)res.Item;

                res.Text += "\nCorrente [mA]: " + Math.Pow(10, pt.Y / 20).ToString("F2") + "mA";
                
            }

            return res;

        }
    }


    //this is the model for the plot view
    //it binds data, axis and everything
    public class FrequencyResponseModel
    {
        
        protected PlotModel mModel;
        protected PlotModel mCurrentModel;
        protected PlotWindow mPlotWindow;

        public FrequencyResponseModel()
        {
            mModel = new PlotModel();
            mCurrentModel = new PlotModel();
        }


        public PlotWindow PlotWindow
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

        private void AddAxis(string key_x, string key_y, double min_freq, double max_freq, double min_db, double max_db, Axis freq_axis, Axis db_axis)
        {
            //freq_axis = new LinearAxis();
            freq_axis.Position = AxisPosition.Bottom;
            freq_axis.Minimum = min_freq;
            freq_axis.Maximum = max_freq;
            freq_axis.Title = "Frequenza";
            freq_axis.Unit = "Hz";
            freq_axis.Key = key_x;

            //db_axis = new LinearAxis();
            db_axis.Position = AxisPosition.Left;
            db_axis.Minimum = min_db;
            db_axis.Maximum = max_db;
            /*db_axis.Title = "Pressione Sonora";
            db_axis.Unit = "dBA";*/
            db_axis.Key = key_y;
            
        }
        

        public PlotModel PlotModel
        {
            get
            {
                return mModel;
            }
        }

        public PlotModel PlotCurrentModel
        {
            get
            {
                return mCurrentModel;
            }
        }

        public bool NewSession (SessionInformation session, List<DataPoint> points, List<DataPoint> currentPoints)
        {
            //create series 
            LineSeries s = new LineSeries();
            //CustomLineSeries s_current = new CustomLineSeries();
            LineSeries s_current = new LineSeries();

            s.Title = session.Name;
            s_current.Title = session.Name;


            //audio graphics
            if ( points != null && points.Count > 0 )
            {
                //there is audio data
                int n_points = points.Count;

                for (int k = 0; k < n_points; k++)
                {
                    s.Points.Add(points[k]);
                }

                double min_freq = points[0].X;;
                double max_freq = points[points.Count - 1].X;;
               
                double min_db = points.Min<DataPoint>(new Func<DataPoint, double>(val => val.Y)) - 10;
                double max_db = points.Max<DataPoint>(new Func<DataPoint, double>(val => val.Y)) + 10;
                
                Guid guid_x = Guid.NewGuid();
                Guid guid_y = Guid.NewGuid();
               
                LinearAxis freq_axis = new LinearAxis();
                LinearAxis db_axis = new LinearAxis();
                
                AddAxis(guid_x.ToString(), guid_y.ToString(), min_freq, max_freq, min_db, max_db, freq_axis, db_axis);

                db_axis.Title = "Pressione Sonora [dBA]";

                mModel.Axes.Add(freq_axis);
                mModel.Axes.Add(db_axis);

                freq_axis.MajorGridlineStyle = LineStyle.Solid;
                freq_axis.MinorGridlineStyle = LineStyle.Dot;

                db_axis.MajorGridlineStyle = LineStyle.Solid;
                db_axis.MinorGridlineStyle = LineStyle.Dot;

                s.XAxisKey = guid_x.ToString();
                s.YAxisKey = guid_y.ToString();

                s.TrackerFormatString = "{0}\n{1}: {2:0.00}Hz\n{3}: {4:0.00}dBmA";

                mModel.Series.Add(s);
               
                session.AudioData = points;
            }

            //current data

            if ( currentPoints != null && currentPoints.Count > 0 )
            {
                int n_points = currentPoints.Count;
                List<DataPoint> currentPoints_db = new List<DataPoint>();

                for (int k = 0; k < n_points; k++)
                {
                    //double f = currentPoints[k].X;
                    //double cur_db = 20 * Math.Log10(Math.Abs(currentPoints[k].Y));

                    //DataPoint point_db = new DataPoint(f, cur_db);

                    //currentPoints_db.Add(point_db);
                    //s_current.Points.Add(point_db);

                    s_current.Points.Add(currentPoints[k]);
                }

                //double min_freq = currentPoints_db[0].X;;
                //double max_freq = currentPoints_db[currentPoints.Count - 1].X;;

                double min_freq = currentPoints[0].X; ;
                double max_freq = currentPoints[currentPoints.Count - 1].X; ;

                //double min_cur = currentPoints_db.Min<DataPoint>(new Func<DataPoint, double>(val => val.Y)) - 10;
                //double max_cur = currentPoints_db.Max<DataPoint>(new Func<DataPoint, double>(val => val.Y)) + 10;
                double min_cur = currentPoints.Min<DataPoint>(new Func<DataPoint, double>(val => val.Y)) - 10;
                double max_cur = currentPoints.Max<DataPoint>(new Func<DataPoint, double>(val => val.Y)) + 10;

                Guid guid_x_cur = Guid.NewGuid();
                Guid guid_y_cur = Guid.NewGuid();

                //LogarithmicAxis freq_axis_cur = new LogarithmicAxis();
                LinearAxis freq_axis_cur = new LinearAxis();
                LinearAxis cur_axis = new LinearAxis();
                
                AddAxis(guid_x_cur.ToString(), guid_y_cur.ToString(), min_freq, max_freq, min_cur, max_cur, freq_axis_cur, cur_axis);

                //cur_axis.Title = "Corrente [dBmA]";
                cur_axis.Title = "Corrente [mA]";

                mCurrentModel.Axes.Add(freq_axis_cur);
                mCurrentModel.Axes.Add(cur_axis);

                freq_axis_cur.MajorGridlineStyle = LineStyle.Solid;
                freq_axis_cur.MinorGridlineStyle = LineStyle.Dot;

                cur_axis.MajorGridlineStyle = LineStyle.Solid;
                cur_axis.MinorGridlineStyle = LineStyle.Dot;
                
                s_current.XAxisKey = guid_x_cur.ToString();
                s_current.YAxisKey = guid_y_cur.ToString();

                //s_current.TrackerFormatString = "{0}\n{1}: {2:0.00}Hz\n{3}: {4:0.00}dBmA";
                s_current.TrackerFormatString = "{0}\n{1}: {2:0.00}Hz\n{3}: {4:0.00}mA";

                mCurrentModel.Series.Add(s_current);

                session.CurrentData = currentPoints;
            }
            
            if (mPlotWindow != null) mPlotWindow.AddSeries(s, s_current, session);

            mModel.InvalidatePlot(true);
            mCurrentModel.InvalidatePlot(true);
            
            return true;
        }

        public void RemoveSeries( LineSeries s, LineSeries s_cur )
        {
            if ( s != null )
            {
                if ( mModel.Series.Contains(s) )
                {
                    mModel.Series.Remove(s);

                    mModel.Axes.Remove(s.XAxis);
                    mModel.Axes.Remove(s.YAxis);

                    mModel.InvalidatePlot(true);
                }
            }

            if (s_cur != null)
            {
                if (mCurrentModel.Series.Contains(s_cur))
                {
                    mCurrentModel.Series.Remove(s_cur);

                    mCurrentModel.Axes.Remove(s_cur.XAxis);
                    mCurrentModel.Axes.Remove(s_cur.YAxis);

                    mCurrentModel.InvalidatePlot(true);
                }
            }
        }
    }
}
