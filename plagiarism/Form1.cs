using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Iveonik.Stemmers;
using Wintellect.PowerCollections;
using dataAnalyze.Algorithms;
using Timer = System.Windows.Forms.Timer;

namespace plagiarism
{
    public partial class Form1 : Form
    {
        private const string Delims = " =.,_{}*\\\n\r\"/?[];:()!\'’”„";
        private static readonly string[] Refs = new string[50];

        private readonly OrderedMultiDictionary<int, string> _keyWords = new OrderedMultiDictionary<int, string>
            (true, (i, i1) => -i.CompareTo(i1), (s, s1) => String.Compare(s, s1, StringComparison.Ordinal));

        private volatile int _checkedfiles;
        private string _collectfilesformat;
        private Dictionary<string, int> _dictWordCount;
        private int _filesneeded;
        private string _fullText = new string('\0', 0);
        private int _index;
        private int _needed;
        private string _response;
        private string[] _splitted;
        private StreamReader _sr;
        private IStemmer _stemmer;
        private string[] _stopwords;
        private StreamWriter _sw;
        private double _time;
        private Timer _timer;
        private KeyValuePair<string, int>[] _sequense; 

        public Form1()
        {
            InitializeComponent();
            filesformat.SelectedItem = "txt";
            filescount.SelectedItem = "10";
        }

        private void BackgroundOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            Monitor.Enter(resultbox);
            resultbox.Items.Insert(0, runWorkerCompletedEventArgs.Result);
            _checkedfiles++;
            Monitor.Exit(resultbox);
        }

        private void BackgroundOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            var filename = (string)doWorkEventArgs.Argument;
            ShingleDetect(filename);
        }

        private void GetDictionary()
        {
            var stoprd = new StreamReader("./programfiles/stopwords.fail", Encoding.GetEncoding(1251));
            var stop = stoprd.ReadToEnd().ToLower();
            if (rusbutton.Checked)
            {
                _stemmer = new RussianStemmer();
            }
            else if (engbutton.Checked)
            {
                _stemmer = new EnglishStemmer();
            }
            _splitted = _fullText.Split(Delims.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            _stopwords = stop.Split(Delims.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var unique = new HashSet<string>();
            _sequense = new KeyValuePair<string, int>[_splitted.Length];
            var index = 0;
            for (var i = 0; i < _splitted.Length; i++)
            {
                var s = _stemmer.Stem(_splitted[i]);
                if (unique.Contains(s)) continue;
                unique.Add(s);
                _sequense[index++] = new KeyValuePair<string, int>(s, i + 1);
            }
            Array.Resize(ref _sequense, index);
            Array.Sort(_splitted);
            Array.Sort(_stopwords);
            _dictWordCount = new Dictionary<string, int>();
            var count = 1;
            for (var i = 1; i < _splitted.Length; i++)
            {
                var s = _splitted[i - 1];
                if (_splitted[i] == s) count++;
                else
                {
                    if (s.Length > 3 && (Array.FindIndex(_stopwords, s.Equals) == -1))
                    {
                        if (!_dictWordCount.ContainsKey(_splitted[i - 1]))
                        {
                            _dictWordCount.Add(_splitted[i - 1], count);
                        }
                    }
                    count = 1;
                }
            }
            _sw = new StreamWriter("./programfiles/unstemmed_with_count.fail");
            foreach (var i in _dictWordCount)
            {
                _sw.WriteLine(i);
            }
            _sw.Close();
        }

        private void DownloadFiles(int x = 0)
        {
            var foundFilesCount = x > _index ? _index : x;
            _checkedfiles = 0;
            _sr = new StreamReader("./programfiles/refs.fail", Encoding.UTF8);
            for (var i = 0; i < foundFilesCount; i++)
            {
                var backloader = new BackgroundWorker
                    {WorkerSupportsCancellation = true, WorkerReportsProgress = true};
                backloader.DoWork += BackloaderOnDoWork;
                backloader.RunWorkerCompleted += BackloaderOnRunWorkerCompleted;
                try
                {
                    var refenence = _sr.ReadLine();
                    backloader.RunWorkerAsync(refenence);
                    _needed++;
                }
                catch (Exception)
                {
                    _needed--;
                }
            }
            _sr.Close();
            while (_checkedfiles < _needed)
            {
            }
            _sw = new StreamWriter("./programfiles/results.fail");
            foreach (var stat in resultbox.Items)
            {
                _sw.WriteLine(stat);
            }
            _sw.Close();
            resultbox.Items.Insert(0, "Результаты были скопированы в файл results.fail в папке programfiles");
        }

        private void BackloaderOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            var reference = (string) doWorkEventArgs.Argument;
            var filename = reference.Substring(reference.LastIndexOf('/') + 1);
            var client = new WebClient();
            try
            {
                var request = (HttpWebRequest) WebRequest.Create(new Uri(reference));
                var response = (HttpWebResponse) request.GetResponse();
                response.Close();
                var filesize = (int) response.ContentLength/1024;
                if (filesize > 1 && filesize <= 10*1024)
                {
                    client.DownloadFile(reference, "./programfiles/" + filename);
                }
                if (shinglebutton.Checked)
                {
                    doWorkEventArgs.Result += ShingleDetect(filename);
                }
                else if (kernelbutton.Checked)
                {
                    doWorkEventArgs.Result = "Проверка не проводилась";
                }
                else
                {
                    doWorkEventArgs.Result = "Проверка не проводилась";
                }
            }
            catch (WebException)
            {
                doWorkEventArgs.Result = "Файл " + filename + " не скачался";
            }
            catch (Exception)
            {
                doWorkEventArgs.Result = "Проверка " + filename + " не удалась";
            }
        }

        private string ShingleDetect(string filename)
        {
            const string heavy = "heavy: ";
            const string cut = "cut: ";
            const string non = "non: ";
            const string light = "light: ";
            var result = "";
            var shingles = new Shingles(Delims, 1);
            string compText;
            try
            {
                compText = ReadFile("./programfiles/" + filename, false);
            }
            catch (Exception e)
            {
                return filename + " fail " + e.Message;
            }
            Monitor.Enter(_fullText);
            var fulltext = _fullText;
            Monitor.Exit(_fullText);
            var similarity = new int[4];
            result += filename + " - ";
            similarity[0] = (int)shingles.CompareStrings(fulltext, compText, 1);
            if (similarity[0] < 50)
            {
                result += non + (similarity[0] + "% совпадения на " + 1 + " этапе");
                return result;
            }
            similarity[1] = (int)shingles.CompareStrings(fulltext, compText, 5);
            if (similarity[1] < 29)
            {
                result += non + (similarity[1] + "% совпадения на " + 2 + " этапе");
                return result;
            }
            if (similarity[1] < 45)
            {
                result += heavy + (similarity[1] + "% совпадения на " + 2 + " этапе");
                return result;
            }

            similarity[2] = (int)shingles.CompareStrings(fulltext, compText, 7);
            if (similarity[2] < 60)
            {
                result += heavy + (similarity[2] + "% совпадения на " + 3 + " этапе");
                return result;
            }
            if (similarity[2] < 85)
            {
                result += light + (similarity[2] + "% совпадения на " + 3 + " этапе");
                return result;
            }
            result += cut + (similarity[2] + "% совпадения на " + 3 + " этапе");
            return result;
        }

        private void BackloaderOnRunWorkerCompleted(object sender,
            RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            Monitor.Enter(resultbox);
            resultbox.Items.Insert(0, runWorkerCompletedEventArgs.Result);
            if (progressBar1.Value + (100 - 12)/_needed <= 100)
            {
                progressBar1.Value += (100 - 12)/_needed;
            }
            _checkedfiles++;
            Monitor.Exit(resultbox);
        }

        private void Statistic()
        {
            for (char i = 'a'; i <= 'e'; i++)
            {
                var compared = 0;
                _fullText = ReadFile("./userfiles/orig_task" + i + ".txt", true);
                var dirInfo = new DirectoryInfo("./programfiles");
                var enumeratefiles = dirInfo.EnumerateFiles("*" + i + ".txt");
                var fileInfos = enumeratefiles as IList<FileInfo> ?? enumeratefiles.ToList();
                foreach (var fileInfo in enumeratefiles)
                {
                    var comparer = new BackgroundWorker();
                    comparer.DoWork += ComparerOnDoWork1;
                    comparer.RunWorkerCompleted += (sender, args) =>
                    {
                        Monitor.Enter(results);
                        compared++;
                        if (!args.Result.Equals(""))
                        {
                            resultbox.Items.Insert(0, args.Result);
                        }
                        if (progressBar1.Value + (100 - 40) / fileInfos.Count() < 100)
                            progressBar1.Value += (100 - 40) / fileInfos.Count();
                        Monitor.Exit(results);
                    };
                    comparer.RunWorkerAsync(fileInfo.Name);
                }
                while (compared < 19)
                {
                }
//                 var sr_orig = new StreamReader("./programfiles/original.fail");
//                 var sr_res = new StreamReader("./programfiles/results.fail");
// 
//                 var original = new Dictionary<string, string>();
//                 var results = new Dictionary<string, string>();
               
            }
//             var dirInfo = new DirectoryInfo("./programfiles");
//             var enumeratefiles = dirInfo.EnumerateFiles("*." + _collectfilesformat);
//             var fileInfos = enumeratefiles as IList<FileInfo> ?? enumeratefiles.ToList();
//             foreach (var fileinfo in fileInfos)
//             {
//                 var comparer = new BackgroundWorker();
//                 comparer.DoWork += ComparerOnDoWork;
//                 comparer.RunWorkerCompleted += (sender, args) =>
//                     {
//                         Monitor.Enter(results);
//                         compared++;
//                         if (!args.Result.Equals(""))
//                         {
//                             resultbox.Items.Insert(0, args.Result);
//                         }
//                         if (progressBar1.Value + (100 - 40)/fileInfos.Count() < 100)
//                             progressBar1.Value += (100 - 40)/fileInfos.Count();
//                         Monitor.Exit(results);
//                     };
//                 comparer.RunWorkerAsync(fileinfo.Name);
//             }
//             while (compared < fileInfos.Count())
//             {
//             }
            _sw = new StreamWriter("./programfiles/results.fail");
            foreach (var stat in resultbox.Items)
            {
                _sw.WriteLine(stat);
            }
            _sw.Close();
            resultbox.Items.Insert(0, "Результаты были скопированы в файл results.fail в папке programfiles");
        }

        private void ComparerOnDoWork1(object sender, DoWorkEventArgs e)
        {
            e.Result = ShingleDetect((string)e.Argument);
        }

        private void ComparerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            doWorkEventArgs.Result = ShingleDetect((string) doWorkEventArgs.Argument);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory =
                @"C:\Users\Евгений\documents\visual studio 2012\Projects\plagiarism\plagiarism\userfiles";
            openFileDialog1.ShowDialog();
        }

        private void start_Click(object sender, EventArgs e)
        {
            panel2.Visible = false;
            backgroundWorker1.ProgressChanged += OnChanged;
            backgroundWorker1.RunWorkerCompleted += OnCompleted;
            _timer = new Timer {Interval = 1000};
            _timer.Tick += TimerOnTick;
            timerlabel.Click += (o, args) => _time--;
            _timer.Start();
            panel3.Visible = true;
            backgroundWorker1.RunWorkerAsync();
        }

        private void OnChanged(object o, ProgressChangedEventArgs args)
        {
            progressBar1.Value = args.ProgressPercentage;
            switch (args.ProgressPercentage)
            {
                case 0:
                    cur_op.Text = @"Чтение файла";
                    break;
                case 10:
                    cur_op.Text = @"Получение ключевых слов";
                    break;
                case 11:
                    cur_op.Text = @"Запрос в Google";
                    break;
                default:
                    if (args.ProgressPercentage >= 12)
                    {
                        cur_op.Text = @"Подсчёт статистики";
                    }
                    break;
            }
        }

        private void OnCompleted(object o, RunWorkerCompletedEventArgs args)
        {
            cur_op.Text = @"Завершено";
            Monitor.Enter(progressBar1);
            progressBar1.Value = 100;
            Monitor.Exit(progressBar1);
            _timer.Stop();
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            _time++;
            timerlabel.ResetText();
            timerlabel.Text += _time;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (googlebutton.Checked)
            {
                backgroundWorker1.ReportProgress(0);
                _fullText = ReadFile(openFileDialog1.FileName, true);
                backgroundWorker1.ReportProgress(10);
                GetDictionary();
                GetKeywords();
                backgroundWorker1.ReportProgress(12);
                GoogleRequest();
                GetReferences();
                if (deletecheck.Checked)
                {
                    var dirInfo = new DirectoryInfo("./programfiles");
                    var enumeratePDFs = dirInfo.EnumerateFiles("*.pdf");
                    foreach (var fileinfo in enumeratePDFs)
                    {
                        try
                        {
                            fileinfo.Delete();
                        }
                        catch(IOException)
                        {
                        }
                    }
                }
                DownloadFiles(_filesneeded);
            }
            else
            {
                Statistic();
            }
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {
            if (progressBar1.Value < 100)
            {
                progressBar1.Value++;
            }
        }

        private void shinglebutton_CheckedChanged(object sender, EventArgs e)
        {
            panel5.Visible = true;
            collectionsettings.Visible = collectionbutton.Checked;
            googlesettings.Visible = googlebutton.Checked;
        }

        private void kernelbutton_CheckedChanged(object sender, EventArgs e)
        {
            panel5.Visible = true;
            collectionsettings.Visible = collectionbutton.Checked;
            googlesettings.Visible = googlebutton.Checked;
        }

        private void nocompbutton_CheckedChanged(object sender, EventArgs e)
        {
            collectionsettings.Visible = false;
            panel5.Visible = googlesettings.Visible = false;
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            filenamelabel.Visible = true;
            var filename = openFileDialog1.FileName;
            filename = filename.Substring(filename.LastIndexOf('\\') + 1);
            filenamelabel.Text = @"Файл выбран: " + filename;
        }

        private void googlebutton_CheckedChanged(object sender, EventArgs e)
        {
            collectionsettings.Visible = false;
            googlesettings.Visible = true;
        }

        private void collectionbutton_CheckedChanged(object sender, EventArgs e)
        {
            collectionsettings.Visible = true;
            googlesettings.Visible = false;
        }

        private void filescount_SelectedIndexChanged(object sender, EventArgs e)
        {
            _filesneeded = Convert.ToInt32(filescount.SelectedItem);
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            _collectfilesformat = (string) filesformat.SelectedItem;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            Shingles.Checkinputfile = true;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            Shingles.Checkinputfile = false;
        }
    }
}