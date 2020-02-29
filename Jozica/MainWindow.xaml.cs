using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace Jozica
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private List<String> listBox1;
        //private ProgressBar progressBar1;
        //private Button fileDialogButton;
        private List<Message> list = new List<Message>();
        //private Label path;
        //private Label realPath;
        private string exportPath = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFileDialog()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = true
            };
            bool? dialogResult = dialog.ShowDialog();
            switch (dialogResult)
            {
                case true:
                    list.Clear();
                    listBox1.Items.Clear();
                    path.Content = "";
                    path.Refresh();
                    realPath.Content = "";
                    realPath.Refresh();
                    exportPath = "";
                    progressBar1.Value = 0;
                    progressBar1.Refresh();
                    String newName = "";
                    int progressBarMultiplayer = 50 / dialog.FileNames.Length;
                    foreach (var path in dialog.FileNames)
                    {
                        if (!string.IsNullOrEmpty(path))
                        {
                            newName = string.IsNullOrEmpty(newName) ? path : newName;

                            StreamReader reader = new StreamReader(path);
                            string contents = reader.ReadToEnd();
                            if (!string.IsNullOrEmpty(contents))
                            {
                                try
                                {
                                    var something = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Message>>(contents);
                                    foreach (var item in something)
                                    {
                                        if (!String.IsNullOrEmpty(item.connectionEvent))
                                        {
                                            list.Add(item);
                                        }
                                    }

                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show(e.Message);
                                }
                            }
                        }
                        listBox1.Items.Add(path);
                        progressBar1.Value += progressBarMultiplayer;
                        progressBar1.Refresh();
                        if (string.IsNullOrEmpty(exportPath))
                        {
                            exportPath = path;
                        }
                    }
                    ExportSelectedToCvv();
                    break;
                case false:
                    MessageBox.Show("You have to select files");
                    break;
                default:
                    // Indeterminate
                    break;
            }
        }
        private void ExportSelectedToCvv()
        {
            int progressBarMultiplayer;
            if (list.Count == 0)
            {
                progressBar1.Value = 0;
                progressBar1.Refresh();
                MessageBox.Show("Cannot find contents");
                return;
            }

            progressBarMultiplayer = 50 / list.Count;
            StringBuilder csv = new StringBuilder();

            if (list.Count > 0)
            {
                DateTime contime;
                DateTime distime;
                var avgdistimesum = new List<TimeSpan>();
                var avgcontimesum = new List<TimeSpan>();
                var evperday = new List<ConEvents>();
                int cons = 0;
                int diss = 0;
                csv.AppendLine("Date; Event; Connected; Disconnected");
                if (list.Count > 1)
                {
                    string anchordt = DateTime.ParseExact(list[1].EvtDateTime, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal).ToShortDateString();
                }
                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i].connectionEvent.Equals("disconnection"))
                    {
                        diss++;
                        distime = DateTime.ParseExact(list[i].EvtDateTime, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                        if (i > 0 && list[i - 1].connectionEvent.Equals("connection"))
                        {
                            contime = DateTime.ParseExact(list[i - 1].EvtDateTime, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                        }
                        else
                        {
                            contime = distime;
                        }
                        var newLine = string.Format("{0};{1};{2};;", list[i].EvtDateTime, list[i].connectionEvent, (distime - contime).ToString());
                        csv.AppendLine(newLine);

                        if (!(distime - contime).Equals(TimeSpan.Zero))
                        {
                            avgcontimesum.Add(distime - contime);
                        }
                    }
                    else if (list[i].connectionEvent.Equals("connection") && (i == 0 || list[i - 1].connectionEvent.Equals("disconnection")))
                    {
                        cons++;
                        contime = DateTime.ParseExact(list[i].EvtDateTime, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                        if (i > 0 && list[i - 1].connectionEvent.Equals("disconnection"))
                        {
                            distime = DateTime.ParseExact(list[i - 1].EvtDateTime, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                        }
                        else
                        {
                            distime = contime;
                        }
                        var newLine = string.Format("{0};{1};;{2}", list[i].EvtDateTime, list[i].connectionEvent, (contime - distime).ToString());
                        csv.AppendLine(newLine);

                        if (!(contime - distime).Equals(TimeSpan.Zero))
                        {
                            avgdistimesum.Add(contime - distime);
                        }
                    }
                    else
                    {
                        cons++;
                        csv.AppendLine(string.Format("{0};{1};;", list[i].EvtDateTime, list[i].connectionEvent));
                    }
                    if (i == list.Count - 1 || !DateTime.ParseExact(list[i].EvtDateTime, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal).ToShortDateString().Equals(DateTime.ParseExact(list[i + 1].EvtDateTime, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal).ToShortDateString()))
                    {
                        evperday.Add(new ConEvents { EventDate = DateTime.ParseExact(list[i].EvtDateTime, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal).ToShortDateString(), DisCount = diss, ConCount = cons });
                        diss = 0;
                        cons = 0;
                    }
                    //Refresh();

                }
                if (avgcontimesum.Count > 0 && avgdistimesum.Count > 0)
                {
                    TimeSpan timecaverage = TimeSpan.FromTicks(Convert.ToInt64(avgcontimesum.Average(ts => ts.Ticks)));
                    TimeSpan timedaverage = TimeSpan.FromTicks(Convert.ToInt64(avgdistimesum.Average(ts => ts.Ticks)));
                    TimeSpan timecmin = TimeSpan.FromTicks(Convert.ToInt64(avgcontimesum.Min(ts => ts.Ticks)));
                    TimeSpan timedmin = TimeSpan.FromTicks(Convert.ToInt64(avgdistimesum.Min(ts => ts.Ticks)));
                    TimeSpan timecmax = TimeSpan.FromTicks(Convert.ToInt64(avgcontimesum.Max(ts => ts.Ticks)));
                    TimeSpan timedmax = TimeSpan.FromTicks(Convert.ToInt64(avgdistimesum.Max(ts => ts.Ticks)));
                    csv.AppendLine(string.Format("Average time;;{0};{1}", timecaverage.ToString(), timedaverage.ToString()));
                    csv.AppendLine(string.Format("Minimum time;;{0};{1}", timecmin.ToString(), timedmin.ToString()));
                    csv.AppendLine(string.Format("Maximum time;;{0};{1}", timecmax.ToString(), timedmax.ToString()));
                    csv.AppendLine("Date;Connections;Disconnections");
                    foreach (var item in evperday)
                    {
                        csv.AppendLine(string.Format("{0};{1};{2}", item.EventDate, item.ConCount, item.DisCount));
                    }
                }

                SaveFileDialog dialogSave = new SaveFileDialog();
                string fname = System.IO.Path.ChangeExtension(exportPath, "csv");
                dialogSave.DefaultExt = "scv";
                dialogSave.FileName = fname;

                dialogSave.Filter = "Comma-separated values (*.csv*)|*.csv";

                bool? dialogResult = dialogSave.ShowDialog();
                switch (dialogResult)
                {
                    case true:
                        File.WriteAllText(dialogSave.FileName, csv.ToString());
                        path.Content = "Path: ";
                        realPath.Content = dialogSave.FileName;
                        progressBar1.Value = 100;
                        realPath.Refresh();
                        path.Refresh();
                        realPath.Refresh();
                        progressBar1.Refresh();
                        try
                        {
                            using Process openCSV = new Process();
                            openCSV.StartInfo.UseShellExecute = true;
                            openCSV.StartInfo.FileName = @dialogSave.FileName;
                            openCSV.StartInfo.CreateNoWindow = false;
                            openCSV.Start();
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }
                        break;
                    case false:
                        MessageBox.Show("Operation has been canceled");
                        break;
                    default:
                        // Indeterminate
                        break;
                }
                //MessageBox.Show("Path :" + dialogSave.FileName);
            }
        }

        class ConEvents
        {
            public string EventDate { get; set; }
            public int ConCount { get; set; }
            public int DisCount { get; set; }
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

    public static class ExtensionMethods
    {
        private static Action EmptyDelegate = delegate () { };

        public static void Refresh(this UIElement uiElement)

        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }
    }
}
