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
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.util;
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

        /// <summary>
        /// Считывает файл в строку, принимаются файлы PDF и TXT
        /// </summary>
        /// <param name="path"></param>
        /// <param name="inputfile"></param>
        /// <returns></returns>
        private static string ReadFile(string path, bool inputfile)
        {
            string resultstr;
            switch (path.Substring(path.Length - 3))
            {
                case "pdf":
                    var stripper = new PDFTextStripper();
                    var doc = PDDocument.load(path);

                    if (inputfile)
                    {
                        var sw = new StreamWriter("./programfiles/suspicious.fail");
                        sw.WriteLine(resultstr = stripper.getText(doc).ToLower());
                        sw.Close();
                    }
                    else
                    {
                        resultstr = stripper.getText(doc).ToLower();
                    }
                    doc.close();
                    break;
                case "txt":
                    var sr = new StreamReader(path);
                    resultstr = sr.ReadToEnd().ToLower();
                    sr.Close();
                    break;
                default:
                    MessageBox.Show(@"Файл должен быть в формате 'pdf' или 'txt'");
                    return "";
            }
            return resultstr;
        }

        /// <summary>
        /// Строит словарь из уникальных слов и количества их использования
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Получает самые частоиспользуемые в тексте слова и заявляет,что они ключевые
        /// </summary>
        /// <returns></returns>
        private void GetKeywords()
        {
            var count = _dictWordCount.Count;
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
            _sw = new StreamWriter("./programfiles/top_ten.fail");
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

        /// <summary>
        /// Делает запрос в Google по ключевым словам на получение страниц со ссылками
        /// </summary>
        /// <returns></returns>
        private void GoogleRequest()
        {
            var reqstr = new string('\0', 0);
            reqstr = _keyWords.SelectMany(i => i.Value).Aggregate(reqstr, (current, s) =>
                                                                          current.Insert(current.Length, s + "+"));
            _response = "";
            for (var i = 0; i < 30; i += 10)
            {
                var request = WebRequest.Create("http://www.google.com/search?q="
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

        /// <summary>
        /// Выдирает из HTML кода ссылки на PDF-файлы
        /// </summary>
        /// <returns></returns>
        private void GetReferences()
        {
            _sw = new StreamWriter("./programfiles/refs.fail", false, Encoding.UTF8);
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

        /// <summary>
        /// Многопоточно скачивает файлы и проверяет каждый на сходство с исходным
        /// </summary>
        /// /// <param name="x"></param>
        /// <returns></returns>
        private void DownloadFiles(int x = 0)
        {
            var foundFilesCount = x > _index ? _index : x;
            _checkedfiles = 0;
            _sr = new StreamReader("./programfiles/refs.fail", Encoding.UTF8);
            for (var i = 0; i < foundFilesCount; i++)
            {
                var backloader = new BackgroundWorker();
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

        /// <summary>
        /// Скачивание и проверка каждого файла в отдельном потоке
        /// </summary>
        /// <returns></returns>
        private void BackloaderOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            var reference = (string)doWorkEventArgs.Argument;
            var filename = reference.Substring(reference.LastIndexOf('/') + 1);
            var client = new WebClient();
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(new Uri(reference));
                var response = (HttpWebResponse)request.GetResponse();
                response.Close();
                var filesize = (int)response.ContentLength / 1024;
                if (filesize > 1 && filesize <= 8 * 1024)
                {
                    client.DownloadFile(reference, "./programfiles/" + filename);
                }
                else
                {
                    doWorkEventArgs.Result += filename + ": битая ссылка или файл слишком большой";
                    return;
                }
                if (shinglebutton.Checked)
                {
                    doWorkEventArgs.Result += ShingleDetect(filename);
                }
            }
            catch (WebException web)
            {
                doWorkEventArgs.Result = filename + ": " + web.Response;
            }
            catch (Exception)
            {
                doWorkEventArgs.Result = "Проверка " + filename + " не удалась";
            }
        }

        /// <summary>
        /// Оповещение о том,что поток завершил работу
        /// </summary>
        /// <returns></returns>
        private void BackloaderOnRunWorkerCompleted(object sender,
            RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            Monitor.Enter(resultbox);
            resultbox.Items.Insert(0, runWorkerCompletedEventArgs.Result);
            if (progressBar1.Value + (100 - 12) / _needed <= 100)
            {
                progressBar1.Value += (100 - 12) / _needed;
            }
            _checkedfiles++;
            Monitor.Exit(resultbox);
        }

        /// <summary>
        /// Определяет,был ли плагиат в исходном тексте
        /// </summary>
        /// /// <param name="filename"></param>
        /// <returns>Степень совпадения и сплагиаченные абзацы</returns>
        private string ShingleDetect(string filename)
        {
            const string heavy = "1: ";
            const string cut = "1: ";
            const string non = "0: ";
            const string light = "1: ";
            var result = "";
            var shingles = new Shingles();
            string compText;
            try
            {
                compText = ReadFile("./programfiles/" + filename, false);
            }
            catch (Exception e)
            {
                return filename + " fail " + e.Message + " plagiarism:0";
            }
            Monitor.Enter(_fullText);
            var fulltext = _fullText;
            Monitor.Exit(_fullText);
            var similarity = new int[4];
            result += filename + " - ";
            similarity[0] = (int)shingles.CompareStrings(fulltext, compText, 1);
            if (similarity[0] < 48)
            {
                result += non + (similarity[0] + "% совпадения на этапе " + 1);
                return result;
            }
            similarity[1] = (int)shingles.CompareStrings(fulltext, compText, 3);
            if (similarity[1] < 5)
            {
                result += non + (similarity[1] + "% совпадения на этапе " + 2);
                return result;
            }
            if (similarity[1] < 45)
            {
                result += heavy + (similarity[1] + "% совпадения на этапе " + 2);
                result += FindPlagiarism(fulltext, compText);
                return result;
            }

            similarity[2] = (int)shingles.CompareStrings(fulltext, compText, 10);
            if (similarity[2] < 60)
            {
                result += heavy + (similarity[2] + "% совпадения на этапе " + 3);
                result += FindPlagiarism(fulltext, compText);
                return result;
            }
            if (similarity[2] < 85)
            {
                result += light + (similarity[2] + "% совпадения на этапе " + 3);
                result += FindPlagiarism(fulltext, compText);
                return result;
            }
            result += cut + (similarity[2] + "% совпадения на этапе " + 3);
            result += FindPlagiarism(fulltext, compText);
            return result;
        }


        /// <summary>
        /// Ищет совпадающие абзацы в двух текстах
        /// </summary>
        /// <param name="inputtext"></param>
        /// <param name="comptext"></param>
        /// <returns></returns>
        private string FindPlagiarism(string inputtext, string comptext)
        {
            var shingles = new Shingles();
            var result = "";
            var separator = new string[1];
            separator[0] = "\n\n";
            var split1 = inputtext.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            var split2 = comptext.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s1 in split1)
            {
                foreach (var s2 in split2)
                {
                    var similarity = (int)shingles.CompareStrings(s1, s2, 1);
                    if (similarity <= 70) continue;
                    result += s2 + "\n";
                    break;
                }
            }
            return result.Length == 0 ? "" : "; found plagiarism: " + result;
        }

        /// <summary>
        /// Проверяет коллекцию на плагиат
        /// </summary>
        /// <returns></returns>
        private void StatisticCollect()
        {
            var compared = 0;
            var dirInfo = new DirectoryInfo("./programfiles");
            var enumeratefiles = dirInfo.EnumerateFiles("*." + _collectfilesformat);
            var fileInfos = enumeratefiles as IList<FileInfo> ?? enumeratefiles.ToList();
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
                    if (progressBar1.Value + (100 - 40) / fileInfos.Count() < 100)
                        progressBar1.Value += (100 - 40) / fileInfos.Count();
                    Monitor.Exit(results);
                };
                comparer.RunWorkerAsync(fileinfo.Name);
            }
            while (compared < fileInfos.Count())
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

        /// <summary>
        /// Проверяет тестсет
        /// </summary>
        /// <returns></returns>
        private void Test()
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
                    comparer.DoWork += ComparerOnDoWork;
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
            }
            _sw = new StreamWriter("./programfiles/results.fail");
            foreach (var stat in resultbox.Items)
            {
                _sw.WriteLine(stat);
            }
            _sw.Close();
            resultbox.Items.Insert(0, "Результаты были скопированы в файл results.fail в папке programfiles");
        }

        /// <summary>
        /// Проверка каждого файла из коллекции/тестсета в отдельном потоке
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Основная часть программы
        /// </summary>
        /// <returns></returns>
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (testbutton.Checked)
            {
                Test();
            }
            else
            {
                backgroundWorker1.ReportProgress(0);
                _fullText = ReadFile(openFileDialog1.FileName, true);
                backgroundWorker1.ReportProgress(10);
                GetDictionary();
                GetKeywords();
                backgroundWorker1.ReportProgress(12);
                GoogleRequest();
                GetReferences();
                if (googlebutton.Checked)
                {
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
                            catch (IOException)
                            {
                            }
                        }
                    }
                    DownloadFiles(_filesneeded);
                }
                else
                {
                    StatisticCollect();
                }
            }
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

        private void testbutton_CheckedChanged(object sender, EventArgs e)
        {
            Shingles.Checkinputfile = false;
        }
    }
}