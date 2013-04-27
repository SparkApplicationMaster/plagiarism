using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
        private const string Delims = " -=.,_{}*\\\n\r\"/?[];:()!\'’”„";
        private static readonly string[] Refs = new string[50];
        private bool _showFullResults;
        private volatile int _checkedfiles;
        private string _collectfilesformat;
        private int _filesneeded;
        private string _inputText = new string('\0', 0);
        private int _index;
        private int _needed;
        private string [] _splitinputtext;
        private string _response;
        private StreamReader _sr;
        private StreamWriter _sw;
        private double _time;
        private Timer _timer;
        private readonly object _resultboxblock = new object();
        private readonly object _inputtextblock = new object();
        private readonly object _progressbarblock = new object();
        public Form1()
        {
            InitializeComponent();
            filesformat.SelectedItem = "pdf";
            filescount.SelectedItem = "10";
        }

        private void ReadInputFile(string fileName)
        {
            _inputText = ReadFile(fileName);
        }

        private static string ReadFile(string fileName)
        {
            string text;

            var fileInfo = new FileInfo(fileName);
            var ext = fileInfo.Extension;
            if (ext == ".pdf")
            {
                var stripper = new PDFTextStripper();
                var doc = PDDocument.load(fileName);
                text = stripper.getText(doc).ToLower();
                doc.close();
            }
            else
            {
                text = File.ReadAllText(fileName).ToLower();
            }
            return text;
        }

        /// <summary>
        /// Строит словарь из уникальных слов и количества их использования
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, int> GetDictionary()
        {
            var stoprd = new StreamReader("./programfiles/stopwords.fail");
            var stop = stoprd.ReadToEnd().ToLower();
            stoprd.Close();
            var words = _inputText.Split(Delims.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var stopwords = stop.Split(Delims.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Array.Sort(words);
            var dictWordCount = new Dictionary<string, int>();
            var count = 1;
            for (var i = 1; i < words.Length; i++)
            {
                var s = words[i - 1];
                if (words[i] == s) count++;
                else
                {
                    if (s.Length > 3 && (Array.FindIndex(stopwords, s.Equals) == -1))
                    {
                        if (!dictWordCount.ContainsKey(words[i - 1]))
                        {
                            dictWordCount.Add(words[i - 1], count);
                        }
                    }
                    count = 1;
                }
            }
            return dictWordCount;
        }

        /// <summary>
        /// Получает самые частоиспользуемые в тексте слова и заявляет,что они ключевые
        /// </summary>
        /// <returns></returns>
        private string GetKeywords()
        {
            var dictWordCount = GetDictionary();
            var count = dictWordCount.Count;
            var keys = new List<string>(count);
            var vals = new List<int>(count);
            var trueVals = new List<int>(count);
            foreach (var i in dictWordCount)
            {
                keys.Add(i.Key);
                vals.Add(i.Value);
                trueVals.Add(i.Value);
            }
            count = dictWordCount.Count;
            var rus = _inputText.IndexOfAny("абвгдеёжзиёклмнопрстуфхцчшщьыъэюя".ToCharArray());
            var stemmer = rus == -1 ? (IStemmer)new EnglishStemmer() : new RussianStemmer();
            for (var i = 1; i < count; i++)
            {
                if (stemmer.Stem(keys[i]) != stemmer.Stem(keys[i - 1])) continue;
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
            var keyWords = new OrderedMultiDictionary<int, string>
            (true, (i, i1) => -i.CompareTo(i1), (s, s1) => String.Compare(s, s1, StringComparison.Ordinal));

            for (int i = 0; i < count; i++)
            {
                keyWords.Add(vals[i], keys[i]);
                if (i > 10)
                {
                    keyWords.Remove(keyWords.Last().Key, keyWords.Last().Value.Last());
                }
            }
            var keywords = keyWords.SelectMany(i => i.Value).Aggregate("", (current, j) => current + (j + "+"));
            _sw = new StreamWriter("./programfiles/top_ten.fail");
            _sw.Write(keywords);
            _sw.Close();
            return keywords;
        }

        /// <summary>
        /// Делает запрос в Google по ключевым словам на получение страниц со ссылками
        /// </summary>
        /// <returns></returns>
        private void GoogleRequest()
        {
            var reqstr = GetKeywords();
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
//             _sw = new StreamWriter("./programfiles/google_response.html");
//             _sw.WriteLine(_response);
//             _sw.Close();
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
        private void DownloadAndCheck(int x = 0)
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
                    client.DownloadFile(reference, "./Collection/" + filename);
                }
                else
                {
                    doWorkEventArgs.Result = new Results(filename, 0, 0, null, "Файл слишком большой или кривой", false);
                    return;
                }
                doWorkEventArgs.Result = ShingleDetect("./Collection/" + filename);
            }
            catch (WebException)
            {
                doWorkEventArgs.Result = new Results(filename, 0, 0, null, "Ошибка связи с веб-ресурсом", false);
            }
            catch (Exception)
            {
                doWorkEventArgs.Result = new Results(filename, 0, 0, null, "Ошибка проверки", false);
            }
        }

        /// <summary>
        /// Оповещение о том,что поток завершил работу
        /// </summary>
        /// <returns></returns>
        private void BackloaderOnRunWorkerCompleted(object sender,
            RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            Monitor.Enter(_resultboxblock);
            if (runWorkerCompletedEventArgs != null)
                resultbox.Items.Insert(0, ((Results) runWorkerCompletedEventArgs.Result).ToFormatString());
            if (progressBar1.Value + (100 - 12) / _needed <= 100)
            {
                progressBar1.Value += (100 - 12) / _needed;
            }
            _checkedfiles++;
            Monitor.Exit(_resultboxblock);
        }

        /// <summary>
        /// Определяет,был ли плагиат в исходном тексте
        /// </summary>
        /// /// <param name="filename"></param>
        /// <returns>Степень совпадения и сплагиаченные предложения</returns>
        private Results ShingleDetect(string filename)
        {
            const double similarityLow1Stage = 48;
            const double similarityLow2Stage = 5.641;
            string compText;
            try
            {
                compText = ReadFile(filename);
            }
            catch (Exception e)
            {
                return new Results(filename, 0, 0, null, "fail", false);
            }
            Monitor.Enter(_inputtextblock);
            var fulltext = _inputText;
            Monitor.Exit(_inputtextblock);
            var similarity = new double[4];
            var info = new FileInfo(filename);
            similarity[0] = Shingles.CompareStrings(ref fulltext, ref compText, 1);
            Results results = null;
            string[] foundPlagiarism = null;
            if (_showFullResults)
            {
                foundPlagiarism = FindPlagiarism(ref fulltext, ref compText);
            }
            if (similarity[0] < similarityLow1Stage)
            {
                results = new Results(info.Name, similarity[0], 1, null, null, false);
                if (_showFullResults)
                {
                    results.PlagiarisedSentences = foundPlagiarism;
                }
                return results;
            }
            similarity[1] = Shingles.CompareStrings(ref fulltext, ref compText, 3);
            if (similarity[1] < similarityLow2Stage)
            {
                results = new Results(info.Name, similarity[1], 2, null, null, false);
                if (_showFullResults)
                {
                    results.PlagiarisedSentences = foundPlagiarism;
                }
                return results;
            }
            if (similarity[1] < 45)
            {
                results = new Results(info.Name, similarity[1], 2, null, null, true);
                if (_showFullResults)
                {
                    results.PlagiarisedSentences = foundPlagiarism;
                }
                return results;
            }

            similarity[2] = Shingles.CompareStrings(ref fulltext, ref compText, 10);
            if (similarity[2] < 60)
            {
                results = new Results(info.Name, similarity[2], 3, null, null, true);
                if (_showFullResults)
                {
                    results.PlagiarisedSentences = foundPlagiarism;
                }
                return results;
            }
            if (similarity[2] < 85)
            {
                results = new Results(info.Name, similarity[2], 3, null, null, true);
                if (_showFullResults)
                {
                    results.PlagiarisedSentences = foundPlagiarism;
                }
                return results;
            }
            results = new Results(info.Name, similarity[2], 3, null, null, true);
            if (_showFullResults)
            {
                results.PlagiarisedSentences = foundPlagiarism;
            }
            return results;
        }


        /// <summary>
        /// Ищет совпадающие абзацы в двух текстах
        /// </summary>
        /// <param name="inputtext"></param>
        /// <param name="comptext"></param>
        /// <returns></returns>
        private string[] FindPlagiarism(ref string inputtext, ref string comptext)
        {
            var separator = new string[5];
            try
            {
                separator[0] = "\n";
                separator[1] = "\t";
                separator[2] = "!";
                separator[3] = "?";
                separator[4] = ".";
                int shift = 0;
                if (testbutton.Checked || _splitinputtext == null)
                {
                    _splitinputtext = inputtext.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                }
                var split2 = comptext.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                var result = new string[_splitinputtext.Length];
                for (int i = 0; i < _splitinputtext.Length; i++)
                {
                    for (int index = 0; index < split2.Length; index++)
                    {
                        var similarity = (int)Shingles.CompareStrings(ref _splitinputtext[i], ref split2[index], 2);
                        if (similarity <= 40) continue;
                        result[shift++] = split2[index];
                        break;
                    }
                }
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Проверяет коллекцию на плагиат
        /// </summary>
        /// <returns></returns>
        private void StatisticCollect()
        {
            var compared = 0;
            var dirInfo = new DirectoryInfo("./Collection");
            var enumeratefiles = dirInfo.EnumerateFiles("*." + _collectfilesformat);
            var fileInfos = enumeratefiles as IList<FileInfo> ?? enumeratefiles.ToList();
            foreach (var fileinfo in fileInfos)
            {
                var comparer = new BackgroundWorker();
                comparer.DoWork += ComparerOnDoWork;
                comparer.RunWorkerCompleted += (sender, args) =>
                {
                    Monitor.Enter(_resultboxblock);
                    compared++;
                    if (!args.Result.Equals(""))
                    {
                        resultbox.Items.Insert(0, args.Result);
                    }
                    if (progressBar1.Value + (100 - 40) / fileInfos.Count() < 100)
                        progressBar1.Value += (100 - 40) / fileInfos.Count();
                    Monitor.Exit(_resultboxblock);
                };
                comparer.RunWorkerAsync(fileinfo.FullName);
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
                _inputText = ReadFile("./Testset/InputFiles/orig_task" + i + ".txt");
                var dirInfo = new DirectoryInfo("./Testset/Collection");
                var enumeratefiles = dirInfo.EnumerateFiles("*" + i + ".txt");
                var fileInfos = enumeratefiles as IList<FileInfo> ?? enumeratefiles.ToList();
                foreach (var fileInfo in enumeratefiles)
                {
                    var comparer = new BackgroundWorker();
                    comparer.DoWork += ComparerOnDoWork;
                    comparer.RunWorkerCompleted += (sender, args) =>
                        {
                            var results = (Results)args.Result;
                            Monitor.Enter(_resultboxblock);
                            compared++;
                                resultbox.Items.Insert(0, results.ToFormatString());
                            if (progressBar1.Value + (100 - 40) / fileInfos.Count() < 100)
                                progressBar1.Value += (100 - 40) / fileInfos.Count();
                            Monitor.Exit(_resultboxblock);
                    };
                    comparer.RunWorkerAsync(fileInfo.FullName);
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
            openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory() + "./Collection";
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
                ReadInputFile(openFileDialog1.FileName);
                backgroundWorker1.ReportProgress(10);
                backgroundWorker1.ReportProgress(12);
                GoogleRequest();
                GetReferences();
                if (googlebutton.Checked)
                {
                    if (deletecheck.Checked)
                    {
                        var dirInfo = new DirectoryInfo("./Collection");
                        var enumeratePDFs = dirInfo.EnumerateFiles("*.pdf");
                        foreach (var fileinfo in enumeratePDFs)
                        {
                            try { fileinfo.Delete(); } catch (IOException) {}
                        }
                    }
                    DownloadAndCheck(_filesneeded);
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
            progressBar1.Value = 100;
            _timer.Stop();
            GC.Collect();
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
            resultbox.Sorted = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            _showFullResults = checkBox1.Checked;
        }
    }
    public class Results
    {
        public Results(string filename, double similarity, int level, string[] sentences, string fail, bool plagiarism)
        {
            Plagiarism = plagiarism;
            Filename = filename;
            Similarity = similarity;
            Level = level;
            PlagiarisedSentences = sentences;
            Fail = fail;
        }
        public string ToFormatString()
        {
            if (Fail == null)
            {
                return Filename + ". Plagiarism: "+ (Plagiarism ? 1 : 0) + ". Similarity: " + Similarity + ". Level: " + Level;
            }
            else
            {
                return Filename + ": " + Fail;
            }
        }

        private readonly bool Plagiarism;
        public string Filename;
        public double Similarity;
        public int Level;
        public string[] PlagiarisedSentences;
        public string Fail;
    }
}