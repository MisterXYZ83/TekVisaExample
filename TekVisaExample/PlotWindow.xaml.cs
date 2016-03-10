using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Series;
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
using OfficeOpenXml;
using System.IO;
using System.Globalization;

namespace TekVisaExample
{
    /// <summary>
    /// Logica di interazione per PlotWindow.xaml
    /// </summary>
    public partial class PlotWindow : Window
    {

        protected class SeriesComboItem
        {
            
            public LineSeries Series;
            public LineSeries CurrentSeries;
            public SessionInformation Information;

            public override string ToString()
            {
                return Series != null ? Series.Title : string.Empty;
            }
        }

        protected FrequencyResponseModel mPlotModel;

        public PlotWindow( FrequencyResponseModel model )
        {
            InitializeComponent();

            mPlotModel = model;

            audioPlot.Model = mPlotModel.PlotModel;
            currentPlot.Model = mPlotModel.PlotCurrentModel;

            mPlotModel.PlotWindow = this;
            
            Loaded += PlotWindow_Loaded;
            Closing += PlotWindow_Closing;

        }
        
        private void PlotWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void PlotWindow_Loaded(object sender, RoutedEventArgs e)
        {
            audioPlot.InvalidatePlot(true);
            currentPlot.InvalidatePlot(true);
        }

       
        public PlotModel FrequencyResponsePlotModel
        {
            get
            {
                return mPlotModel.PlotModel;
            }
        }

        public void AddSeries (LineSeries s, LineSeries s_cur, SessionInformation info)
        {
            if ( s != null || s_cur != null )
            {
                SeriesComboItem item = new SeriesComboItem();

                //item.Series = s;
                item.Series = s;
                item.CurrentSeries = s_cur;

                item.Information = info;

                seriesCombo.Items.Add(item);

                seriesCombo.SelectedIndex = 0;
            }
        }
        

        private void removeSeries_Click(object sender, RoutedEventArgs e)
        {

            MessageBoxResult res = MessageBox.Show("Vuoi eliminare la risposta selezionata?", "Conferma", MessageBoxButton.YesNo);

            if ( res == MessageBoxResult.Yes )
            {
                SeriesComboItem item = seriesCombo.SelectedItem as SeriesComboItem;

                if (item != null)
                {
                    mPlotModel.RemoveSeries(item.Series, item.CurrentSeries);

                    seriesCombo.Items.Remove(item);
                }
            }
            
        }

        private void exportXls_Click(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".xlsx";
            dlg.Filter = "Excel Files (*.xlsx)|*.xlsx";
            dlg.CheckPathExists = false;
            dlg.CheckFileExists = false;

            Nullable<bool> result = dlg.ShowDialog();

            if (result == false) return;

            //create excel file
            FileInfo f = new FileInfo(dlg.FileName);

            SessionInformation info = (seriesCombo.SelectedItem as SeriesComboItem).Information;

            List<DataPoint> audio_data = info.AudioData;
            List<DataPoint> cur_data = info.CurrentData;

            ExcelPackage excelFile = null;

            try
            {
                do
                {
                    try
                    {
                        excelFile = new ExcelPackage(f);
                    }
                    catch (Exception excp)
                    {
                        MessageBoxResult res = MessageBox.Show("File Aperto! Chiudere il file e riprovare", "Conferma", MessageBoxButton.YesNo);

                        if (res == MessageBoxResult.Yes)
                        {
                            continue;
                        }
                        else return;
                    }

                    //se non becco l'eccezione esco
                    break;
                }
                while (true);

                //open a worksheet to export new data
                ExcelWorksheet worksheet = null;

                try
                {
                    worksheet = excelFile.Workbook.Worksheets.Add(info.Name + " - " + info.Date.ToShortDateString());
                }
                catch (Exception exc)
                {
                    //gia esistente, creo uno nuovo

                    string pratica = "Misura-";
                    pratica += info.Name + "-" + info.Date.ToLongTimeString() + "-" + info.Date.ToLongDateString();

                    worksheet = excelFile.Workbook.Worksheets.Add(pratica);

                }

                //now configure sheet
                //information
                worksheet.Cells[1, 1].Value = "Nome Sessione:";
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 2, 1, 10].Merge = true;
                worksheet.Cells[1, 2].Value = info.Name;

                worksheet.Cells[2, 1].Value = "Data:";
                worksheet.Cells[2, 1].Style.Font.Bold = true;
                worksheet.Cells[2, 2, 2, 10].Merge = true;
                worksheet.Cells[2, 2].Value = info.Date.ToShortDateString();

                worksheet.Cells[3, 1].Value = "Buzzer:";
                worksheet.Cells[3, 1].Style.Font.Bold = true;
                worksheet.Cells[3, 2, 3, 10].Merge = true;
                worksheet.Cells[3, 2].Value = info.Buzzer;

                worksheet.Cells[4, 1].Value = "Descrizione";
                worksheet.Cells[4, 1].Style.Font.Bold = true;
                worksheet.Cells[4, 2, 4, 10].Merge = true;
                worksheet.Cells[4, 2].Value = info.Description;

                worksheet.Cells[6, 1, 5, 2].Style.Font.Bold = true;

                worksheet.Cells[6, 1].Value = "Frequenza [Hz]";
                worksheet.Column(1).Style.Numberformat.Format = "0.00";
                worksheet.Column(1).Width = 20;

                worksheet.Cells[6, 2].Value = "Pressione sonora [dBA]";
                worksheet.Column(2).Style.Numberformat.Format = "0.00";
                worksheet.Column(2).Width = 20;

                worksheet.Cells[6, 3].Value = "Corrente [A]";
                worksheet.Column(3).Style.Numberformat.Format = "0.00";
                worksheet.Column(3).Width = 20;

                //write data on cells
                int num_samples = 0;

                //audio serie
                if ( info.AudioData != null && info.AudioData.Count > 0 )
                {
                    num_samples = info.AudioData.Count;

                    for (int k = 0; k < num_samples; k++)
                    {
                        double freq = 0;
                        double spl = 0;
                        double cur = 0;

                        freq = info.AudioData[k].X;
                        spl = info.AudioData[k].Y;
                        
                        worksheet.Cells[7 + k, 1].Value = freq;
                        worksheet.Cells[7 + k, 2].Value = spl;

                    }
                }

                //current data

                if (info.CurrentData != null && info.CurrentData.Count > 0)
                {
                    num_samples = info.CurrentData.Count;

                    for (int k = 0; k < num_samples; k++)
                    {
                        double freq = 0;
                        double cur = 0;

                        freq = info.CurrentData[k].X;
                        cur = info.CurrentData[k].Y;

                        worksheet.Cells[7 + k, 1].Value = freq; //in case there are both, overwrite (audio and current has same frequency points)
                        worksheet.Cells[7 + k, 3].Value = cur;

                    }
                }
                
                excelFile.Save();

            }
            catch (Exception exp)
            {
                MessageBox.Show("Problema con esportazione Excel, salvo in formato TXT");

                StreamWriter writer = new StreamWriter(dlg.FileName);

                writer.Write("Nome Prova: ");
                writer.WriteLine(info.Name);

                writer.Write("Data: ");
                writer.WriteLine(info.Date.ToShortDateString());

                writer.Write("Buzzer: ");
                writer.WriteLine(info.Buzzer);

                writer.Write("Descrizione: ");
                writer.WriteLine(info.Description);

                writer.WriteLine("DATI:");
                writer.WriteLine("Frequenza\t dBA\t Corrente");

                int num_samples = 0;
                if (info.AudioData != null && info.AudioData.Count > 0) num_samples = info.AudioData.Count;
                else if (info.CurrentData != null && info.CurrentData.Count > 0) num_samples = info.CurrentData.Count;

                for (int k = 0; k < num_samples; k++)
                {

                    double freq = 0;
                    double spl = 0;
                    double cur = 0;

                    if (info.AudioData != null && info.AudioData.Count > 0)
                    {
                        freq = info.AudioData[k].X;
                        spl = info.AudioData[k].Y;
                    }
                    else if (info.CurrentData != null && info.CurrentData.Count > 0)
                    {
                        freq = info.CurrentData[k].X;
                        cur = info.CurrentData[k].Y;
                    }
                    
                    writer.WriteLine(freq.ToString("F2", CultureInfo.InvariantCulture) + "\t" + spl.ToString("F2", CultureInfo.InvariantCulture) + "\t" + cur.ToString("F2", CultureInfo.InvariantCulture));

                }

                writer.Close();
            }
        
            
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
