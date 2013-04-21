using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using Iveonik.Stemmers;
using Wintellect.PowerCollections;
using dataAnalyze.Algorithms;
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.util;
using Timer = System.Windows.Forms.Timer;

namespace plagiarism
{
    public partial class Form1 : Form
    {
        private const string Delims = " =.,_{}*\\\n\r\"/?[];:()!\'”";
        private string _fullText = new string('\0', 0);
        private StreamWriter _sw;
        private StreamReader _sr;
        private readonly PDFTextStripper _stripper = new PDFTextStripper();
        private PDDocument _doc;
        private string[] _splitted;
        private string[] _stopwords;
        private IStemmer _stemmer;
        private string _response;
        private int _index;
        private Timer _timer;
        private double _time;
        private int _needed;
        private int _filesneeded;
        private volatile int _checkedfiles;
        private readonly Shingles _shingles = new Shingles(Delims, 5);
        private Dictionary<string, int> _dictWordCount;
        private readonly OrderedMultiDictionary<int, string> _keyWords = new OrderedMultiDictionary<int, string>
                (true, (i, i1) => -i.CompareTo(i1), (s, s1) => String.Compare(s, s1, StringComparison.Ordinal));

        private static readonly string[] Refs = new string[50];

        public Form1()
        {
            InitializeComponent();
            shilglelength.SelectedItem = "5";
            filescount.SelectedItem = "5";
        }

        private void ReadFile()
        {
            var path = openFileDialog1.FileName;
            switch (path.Substring(path.Length - 3))
            {
                case "pdf":
                    _sw = new StreamWriter("./programfiles/suspicious.txt");
                    _doc = PDDocument.load(path);
                    _sw.WriteLine(_fullText = _stripper.getText(_doc).ToLower());
                    _sw.Close();
                    _doc.close();
                    break;
                case "txt":
                    _sr = new StreamReader(path);
                    _fullText = _sr.ReadToEnd().ToLower();
                    _sr.Close();
                    break;
                default:
                    MessageBox.Show(@"Файл должен быть в формате 'pdf' или 'txt'");
                    break;
            }
        }

        private void GetDictionary()
        {
            var stoprd = new StreamReader("./programfiles/stopwords.txt", Encoding.GetEncoding(1251));
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
                        _dictWordCount.Add(_splitted[i - 1], count);
                    }
                    count = 1;
                }
            }
            _sw = new StreamWriter("./programfiles/unstemmed_with_count.txt");
            foreach (var i in _dictWordCount)
            {
                _sw.WriteLine(i);
            }
            _sw.Close();
        }

        private void GetKeywords()
        {
            int count = _dictWordCount.Count;
            var keys = new List<string>(count);
            var vals = new List<int>(count);
            var trueVals = new List<int>(count);
            foreach (var i in _dictWordCount)
            {
                keys.Add(i.Key);
                vals.Add(i.Value);
                trueVals.Add(i.Value);
            }
            count = _dictWordCount.Count;
            for (var i = 1; i < count; i++)
            {
                if (_stemmer.Stem(keys[i]) != _stemmer.Stem(keys[i - 1])) continue;
                if (keys[i].Length > keys[i - 1].Length || trueVals[i] < trueVals[i - 1])
                {
                    keys[i] = keys[i - 1];
                    trueVals[i] = trueVals[i - 1];
                }
                vals[i] += vals[i - 1];
                keys.RemoveAt(i - 1);
                vals.RemoveAt(i - 1);
                trueVals.RemoveAt(i - 1);
                i--;
                count--;
            }
            for (int i = 0; i < count; i++)
            {
                _keyWords.Add(vals[i], keys[i]);
                if (i > 10)
                {
                    _keyWords.Remove(_keyWords.Last().Key, _keyWords.Last().Value.Last());
                }
            }
            _sw = new StreamWriter("./programfiles/top_ten.txt");
            foreach (var i in _keyWords)
            {
                _sw.WriteLine(i);
            }
            foreach (var i in _keyWords)
            {
                foreach (var j in i.Value)
                {
                    _sw.Write(j + " ");
                }
            }
            _sw.Close();
        }

        private void GoogleRequest()
        {
            var reqstr = new string('\0', 0);
            reqstr = _keyWords.SelectMany(i => i.Value).Aggregate(reqstr, (current, s) => 
                current.Insert(current.Length, s + "+"));
            _response = "";
            for (int i = 0; i < 30; i += 10)
            {
                WebRequest request = WebRequest.Create("http://www.google.com/search?q=" 
                                                       + reqstr + "filetype:pdf" + "&start=" + i);
                request.Method = "GET";
                var response = request.GetResponse();
                var dataStream = response.GetResponseStream();
                if (dataStream == null) continue;
                var reader = new StreamReader(dataStream, Encoding.UTF8);
                _response += reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();
            }
            _sw = new StreamWriter("./programfiles/google_response.html");
            _sw.WriteLine(_response);
            _sw.Close();
        }

        private void GetReferences()
        {
            _sw = new StreamWriter("./programfiles/refs.txt",false, Encoding.UTF8);
            _index = 0;
            for (var i = 0; i < _response.Length - 6; i++)
            {
                if (_response.Substring(i, 6) != "url?q=") continue;
                int j;
                for (j = i + 6; j < _response.Length && _response[j] != '&'; j++)
                {
                }
                if (_response.Substring(j - 3, 3) != "pdf") continue;
                Refs[_index] = _response.Substring(i + 6, j - i - 6);
                Refs[_index] = HttpUtility.UrlDecode(Refs[_index]);
                _sw.WriteLine(Refs[_index]);
                _index++;
                i = j;
            }
            _sw.Close();
        }

        private void DownloadFiles(int x = 0)
        {
            var foundFilesCount = x == 0 ? _index : x;
            _checkedfiles = 0;
            _sr = new StreamReader("./programfiles/refs.txt", Encoding.UTF8);
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
            {}
            _sw = new StreamWriter("./programfiles/results.txt");
            foreach (var stat in resultbox.Items)
            {
                _sw.WriteLine(stat);                
            }
            _sw.Close();
            resultbox.Items.Insert(0, "Результаты были скопированы в файл results.txt в папке programfiles");
        }

        private void BackloaderOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            var reference = (string)doWorkEventArgs.Argument;
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
            var stripper = new PDFTextStripper();
            const string somecopy = "Немного было скопировано из ";
            const string copypaste = "Весь текст был скопирован из ";
            const string nocopy = "Ничего не копировалось из ";
            const string alotcopy = "Много копипаста из ";
            var result = "";
            PDDocument doc;
            try
            {
                doc = PDDocument.load("./programfiles/" + filename);
            }
            catch (Exception)
            {
                return filename + " fail";
            }
            string compText = stripper.getText(doc).ToLower();
            doc.close();
            Monitor.Enter(_fullText);
            var fulltext = _fullText;
            Monitor.Exit(_fullText);
            var similarity = (int)_shingles.CompareStrings(fulltext, compText);
            result += (similarity + "% совпадения - ");
            if (similarity < 7)
            {
                result += (nocopy);
            }
            else if (similarity < 40)
            {
                result += (somecopy);
            }
            else if (similarity < 80)
            {
                result += (alotcopy);
            }
            else
            {
                result += (copypaste);
            }
            result += filename;
            return result;
        }

        private void BackloaderOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
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
            var compared = 0;
            var dirInfo = new DirectoryInfo("./programfiles");
            var enumeratePDFs = dirInfo.EnumerateFiles("*.pdf");
            var fileInfos = enumeratePDFs as IList<FileInfo> ?? enumeratePDFs.ToList();
            foreach (var fileinfo in fileInfos)
            {
                var comparer = new BackgroundWorker();
                comparer.DoWork += ComparerOnDoWork;
                comparer.RunWorkerCompleted += (sender, args) =>
                    {
                        Monitor.Enter(results);
                        compared++;
                        if (!args.Result.Equals(""))
                        {
                            resultbox.Items.Insert(0, args.Result);
                        }
                        if (progressBar1.Value + (100 - 40) / fileInfos.Count() <= 100) 
                            progressBar1.Value += (100 - 40) / fileInfos.Count();
                        Monitor.Exit(results);
                    };
                comparer.RunWorkerAsync(fileinfo.Name);
            }
            while (compared < fileInfos.Count())
            {
            }
        }

        private void ComparerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            doWorkEventArgs.Result = ShingleDetect((string)doWorkEventArgs.Argument);
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
            backgroundWorker1.ReportProgress(0);
            ReadFile();
            backgroundWorker1.ReportProgress(10);
            GetDictionary();
            GetKeywords();
            backgroundWorker1.ReportProgress(12);
            if (googlebutton.Checked)
            {
                GoogleRequest();
                GetReferences();
                if (deletecheck.Checked)
                {
                    var dirInfo = new DirectoryInfo("./programfiles");
                    var enumeratePDFs = dirInfo.EnumerateFiles("*.pdf");
                    foreach (var fileinfo in enumeratePDFs)
                    {
                        fileinfo.Delete();
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
            shlenpalen.Visible = true;
            deletecollect.Visible = googlebutton.Checked;
        }

        private void kernelbutton_CheckedChanged(object sender, EventArgs e)
        {
            panel5.Visible = true;
            shlenpalen.Visible = false;
            deletecollect.Visible = googlebutton.Checked;
        }

        private void nocompbutton_CheckedChanged(object sender, EventArgs e)
        {
            panel5.Visible = deletecollect.Visible = false;
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
            deletecollect.Visible = true;
        }

        private void collectionbutton_CheckedChanged(object sender, EventArgs e)
        {
            deletecollect.Visible = false;
        }

        private void filescount_SelectedIndexChanged(object sender, EventArgs e)
        {
            _filesneeded = Convert.ToInt32(filescount.SelectedItem);
        }

        private void shilglelength_SelectedIndexChanged(object sender, EventArgs e)
        {
            _shingles.ShingleLength = Convert.ToInt32(shilglelength.SelectedItem);
        }
    }
}
