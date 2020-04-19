using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemandForecastImportCSV
{
    public partial class Form1 : Form
    {
        private bool btnClicked = false;

        Stopwatch timePerParse;

        public Form1()
        {
            InitializeComponent();
            button2.Visible = false;
            label1.Text = string.Empty;
            label2.Text = string.Empty;
            label4.Text = string.Empty;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            btnClicked = true;
            bool errorstatus = false;
            try
            {
                string path = string.Empty;// ConfigurationSettings.AppSettings["filePath"].ToString();

                DataTable dt = new DataTable();
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Text files | *.csv"; // file types, that will be allowed to upload
                dialog.Multiselect = false; // allow/deny user to upload more than one file at a time
                if (dialog.ShowDialog() == DialogResult.OK) // if user clicked OK
                {
                    path = dialog.FileName; // get name of file                    
                    label2.Text = path;
                    label3.Text = "File Selected";
                    dt = GetDataTable_Text(path);
                    string ConnString = Convert.ToString(ConfigurationSettings.AppSettings["ConnString"]);
                    string location = Convert.ToString(ConfigurationSettings.AppSettings["location"]);
                    string meterid = Convert.ToString(ConfigurationSettings.AppSettings["meterid"]);
                    string strquery = string.Empty;
                    int rowIndex = 0;
                    int tblRowCount = dt.Rows.Count - 1;
                    timePerParse = Stopwatch.StartNew();
                    foreach (DataRow row in dt.Rows)
                    {
                        Application.DoEvents();
                        if (rowIndex > 0)
                        {
                            string dtcol = string.Empty;
                            string blockno = string.Empty;
                            string kw = string.Empty;
                            int colIndex = 0;
                            string dateCol = string.Empty;
                            string date = string.Empty;
                            string time = string.Empty;
                            string tempItem = string.Empty;
                            foreach (var item in row.ItemArray)
                            {
                                if (colIndex > 0)
                                {
                                    if (colIndex == 1 && item != null && item.ToString() != "")
                                    {
                                        tempItem = item.ToString();
                                        date += Convert.ToDateTime(item).ToString("yyyy-MM-dd");
                                    }
                                    if (colIndex == 2 && item != null && item.ToString() != "")
                                    {
                                        string[] words = item.ToString().Split('-');
                                        time += words[1];
                                    }
                                    if (colIndex == 3 && item != null && item.ToString() != "")
                                    {
                                        blockno = item.ToString();
                                        if (blockno == "96")
                                        {
                                            //dateCol += Convert.ToDateTime(tempItem).AddDays(1).ToString("yyyy-MM-dd") + " " + time;
                                            dateCol += date + " " + time;
                                        }
                                        else
                                        {
                                            dateCol += date + " " + time;
                                        }
                                    }
                                    if (colIndex == 4 && item != null && item.ToString() != "")
                                    {
                                        kw = item.ToString();
                                    }

                                }
                                colIndex++;
                            }
                            if (dateCol != string.Empty && blockno != string.Empty && kw != string.Empty)
                            {
                                dtcol = Convert.ToDateTime(dateCol).ToString("yyyy-MM-dd HH:mm:ss");
                                strquery = "insert into inputdemandforecasts (meterid,tstamp,blockno,kw,location,param1,param2) values ('" + meterid + "','" + dtcol + "','" + blockno + "','" + kw + "'," + location + ",0,0);";
                                try
                                {
                                    InformixHelper.ExecuteNonQuery(ConnString, CommandType.Text, strquery);
                                }
                                catch (Exception ex)
                                {
                                    errorstatus = true;
                                    WriteLog(ex.ToString());
                                    //throw ex;
                                }

                            }
                            else
                            {
                                errorstatus = true;
                                WriteLog("Invalid format please check row number " + rowIndex);
                            }
                            label1.Text = "Processing " + rowIndex + " row from " + tblRowCount + " rows";
                        }
                        rowIndex++;

                    }
                }
                else
                {
                    button1.Enabled = true;
                }
                if (!errorstatus)
                    label1.Text = "Processing Done.";
                else
                    label1.Text = "Processing done with errors please check log file";

                timePerParse.Stop();

                label4.Text = timePerParse.Elapsed.TotalSeconds.ToString() + " seconds.";
            }
            catch (Exception ex)
            {
                label1.Text = "Processing done with errors please check log file";
                WriteLog(ex.ToString());
            }
            btnClicked = false;

            button2.Visible = true;

        }


        public DataTable GetDataTable_Text(string StrFilePath)
        {

            string[] strTemp;
            string TmpLineStr;
            DataTable DtInput = new DataTable();
            DataTable DtResult = new DataTable();
            StreamReader strReader;
            string str;
            strReader = new StreamReader(StrFilePath);
            try
            {


                do
                {
                    TmpLineStr = strReader.ReadLine();

                    strTemp = GetInArrayByPipe(TmpLineStr);
                    AddColumnToTable(ref DtInput, strTemp.Length);
                    DtInput.Rows.Add(strTemp);
                }

                while (strReader.EndOfStream == false);

                DtResult = DtInput.Copy();
            }

            catch (Exception ex)
            {
                WriteLog(ex.ToString());
            }

            finally
            {
                if (strReader != null)
                {
                    strReader.Close();
                    strReader.Dispose();
                }

                strReader = null;

                if (DtInput != null)
                {
                    DtInput.Dispose();
                }
                DtInput = null;


            }

            return DtResult;
        }

        private string[] GetInArrayByPipe(string pStrValue)
        {
            try
            {
                string Tmpstr = "";
                int Index_S, Index_E, TmpIndex;

                Index_E = pStrValue.IndexOf((char)34);

                if (Index_E > 0)
                {
                    Index_S = 0;
                    Tmpstr = "";
                    while (true)
                    {

                        Index_E = pStrValue.IndexOf(pStrValue, (char)34, Index_S + 1);


                        if (Index_E > 0)
                        {

                            Tmpstr += pStrValue.Substring(Index_S, Index_E - Index_S - 1).Replace(",", "|");
                            Index_S = Index_E;
                            Index_E = pStrValue.IndexOf(pStrValue, (char)34, Index_E + 1);
                            Tmpstr += pStrValue.Substring(Index_S, (Index_E - Index_S) - 1);
                            Index_S = Index_E;
                        }

                        else
                        {
                            Tmpstr += pStrValue.Substring(Index_S, pStrValue.Length - Index_S).Replace(",", "|");

                            return Tmpstr.Split(new char[] { '|' });

                            break;
                        }

                    }
                }

                else
                {

                    pStrValue.Split(new char[] { ',' });

                }
            }

            catch (Exception ex)
            {

            }
            return pStrValue.Split(new char[] { ',' });
        }

        private void AddColumnToTable(ref DataTable pDt, int pCols)
        {
            if (pDt == null)
            {
                pDt = new DataTable("Input");
            }

            if (pDt.Columns.Count < pCols)
            {
                //pDt.Columns.Add(New DataColumn(ColumnName(pDt.Columns.Count)))
                pDt.Columns.Add(new DataColumn("Column_" + pDt.Columns.Count));
                AddColumnToTable(ref pDt, pCols);
            }

        }

        public static void WriteLog(string message)
        {
            StreamWriter sw = null;
            try
            {
                //string path = AppDomain.CurrentDomain.BaseDirectory + "Logs";
                string path = ConfigurationSettings.AppSettings["LogPath"].ToString();
                ////path = "C:\\logs";
                bool exists = System.IO.Directory.Exists(path);
                if (!exists)
                    System.IO.Directory.CreateDirectory(path);
                sw = new StreamWriter(path + "\\LogFile_" + DateTime.Today.ToString("ddMMMyyyy") + ".txt", true);
                //sw = new StreamWriter(ConfigurationSettings.AppSettings["LogPath"].ToString() + "\\LogFile_" + DateTime.Today.ToString("ddMMMyyyy") + ".txt", true);
                sw.WriteLine(DateTime.Now.ToString() + " : " + message);
                sw.WriteLine("___________________________________________________________________________________________________");
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
            catch
            {
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (btnClicked)
            {
                MessageBox.Show("Data is Processing.......Please wait!");
                e.Cancel = true;
            }
            else
                e.Cancel = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
