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
    public partial class Main : Form
    {
        private const string Delims = " -=.,_{}*\\\n\r\"/?[];:()!\'’”„";
        private string _inputText;
        private string [] _splitinputtext;
        private double _time;
        private Timer _timer;
        private readonly object _resultboxblock = new object();
        private readonly object _inputtextblock = new object();
        public Main()
        {
            InitializeComponent();
            filesformat.SelectedItem = "pdf";
            filescount.SelectedItem = "10";
        }

        /// <summary>
        /// Заносит результат чтения файла в поле _inputText
        /// </summary>
        private void ReadInputFile(string fileName)
        {
            _inputText = ReadFile(fileName);
        }

        /// <summary>
        /// Читает текстовый или PDF-файл
        /// </summary>
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
            var stemmer = _inputText.IndexOfAny("абвгдеёжзиёклмнопрстуфхцчшщьыъэюя".ToCharArray()) == -1 
                ? (IStemmer)new EnglishStemmer() : new RussianStemmer();
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
                trueVals.RemoveAt(i-- - 1);
                count--;
            }
            var keyWords = new OrderedMultiDictionary<int, string>
            (true, (i, i1) => -i.CompareTo(i1), (s, s1) => String.Compare(s, s1, StringComparison.Ordinal));

            for (var i = 0; i < count; i++)
            {
                keyWords.Add(vals[i], keys[i]);
                if (i > 10) keyWords.Remove(keyWords.Last().Key, keyWords.Last().Value.Last());
            }
            var keywords = keyWords.SelectMany(i => i.Value).Aggregate("", (current, j) => current + (j + "+"));
            File.CreateText("./programfiles/top_ten.fail").Write(keywords);
            return keywords;
        }

        /// <summary>
        /// Делает запрос в Google на получение ссылок на PDF-файлы
        /// </summary>
        /// <returns></returns>
        private List<string> GetReferences(int filesToDownloadCount)
        {
            var reqstr = GetKeywords();
            var webPages = "";
            for (var i = 0; i < filesToDownloadCount +
                (filesToDownloadCount % 10 == 0 ? 0 : 10 - filesToDownloadCount); i += 10)
            {
                var request = WebRequest.Create("http://www.google.com/search?q="
                                                + reqstr + "filetype:pdf" + "&start=" + i);
                request.Method = "GET";
                var response = request.GetResponse();
                var dataStream = response.GetResponseStream();
                if (dataStream == null) return null;
                var reader = new StreamReader(dataStream);
                webPages += reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();
            }
            File.CreateText("./programfiles/google_response.html").Write(webPages);
            var references = new List<string>(filesToDownloadCount);
            for (var i = 0; i < webPages.Length - 6; i++)
            {
                if (webPages.Substring(i, 6) != "url?q=") continue;
                int j;
                for (j = i + 6; j < webPages.Length && webPages[j] != '&'; j++)
                {
                }
                if (webPages.Substring(j - 3, 3) != "pdf") continue;
                references.Add(HttpUtility.UrlDecode(webPages.Substring(i + 6, j - i - 6)));
                i = j;
            }
            return references;
        }

        /// <summary>
        /// Многопоточно скачивает нужное количество файлов и проверяет каждый на сходство с исходным
        /// </summary>
        /// <param name="filesToDownloadCount"></param>
        /// <returns></returns>
        private void DownloadAndCheck(int filesToDownloadCount)
        {
            var references = GetReferences(filesToDownloadCount);
            var backloader = new BackgroundWorker[references.Count()];
            int index = 0;
            foreach (var reference in references)
            {
                backloader[index] = new BackgroundWorker();
                backloader[index].DoWork += BackloaderOnDoWork;
                backloader[index].RunWorkerCompleted += (sender, args) =>
                    {
                        Monitor.Enter(_resultboxblock);
                        if (args != null) resultbox.Items.Insert(0, ((Results) args.Result).ToText());
                        progressBar1.Value += (100 - progressBar1.Value)/Convert.ToInt32(filescount.SelectedItem);
                        Monitor.Exit(_resultboxblock);
                    };
                try
                {
                    backloader[index].RunWorkerAsync(reference);
                }
                catch (Exception)
                {
                }
                index++;
            }
            Thread.SpinWait(100);
            while (!Equals(backloader.Count(worker => worker.IsBusy), 0))
            {
            }
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
                    doWorkEventArgs.Result = new Results(filename, 0, 0, "Файл слишком большой или кривой", false);
                    return;
                }
                doWorkEventArgs.Result = ShingleDetect("./Collection/" + filename);
            }
            catch (WebException)
            {
                doWorkEventArgs.Result = new Results(filename, 0, 0, "Ошибка связи с веб-ресурсом", false);
            }
            catch (Exception)
            {
                doWorkEventArgs.Result = new Results(filename, 0, 0, "Ошибка проверки", false);
            }
        }

        private void WriteResultsToFile()
        {
            using (var sw = new StreamWriter("./programfiles/results.fail"))
            {
                foreach (var stat in resultbox.Items)
                {
                    sw.WriteLine(stat);
                }
                sw.Close();
            }
            resultbox.Items.Insert(0, "Результаты были скопированы в файл results.fail в папке programfiles");
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
            catch
            {
                return new Results(filename, 0, 0, "fail", false);
            }
            Monitor.Enter(_inputtextblock);
            var fulltext = _inputText;
            Monitor.Exit(_inputtextblock);
            var similarity = new double[2];
            var info = new FileInfo(filename);
            similarity[0] = Shingles.CompareStrings(fulltext, compText, 1);
            Results results;
            string[] foundPlagiarism = null;
            var fullResults = showFullResults.Checked;
            if (fullResults)
            {
                foundPlagiarism = FindPlagiarism(fulltext, compText);
            }
            if (similarity[0] < similarityLow1Stage)
            {
                results = new Results(info.Name, similarity[0], 1, null, false);
                if (fullResults)
                {
                    results.PlagiarisedSentences = foundPlagiarism;
                }
                return results;
            }
            similarity[1] = Shingles.CompareStrings(fulltext, compText, 3);
            if (similarity[1] < similarityLow2Stage)
            {
                results = new Results(info.Name, similarity[1], 2, null, false);
                if (fullResults)
                {
                    results.PlagiarisedSentences = foundPlagiarism;
                }
                return results;
            }
            results = new Results(info.Name, similarity[1], 2, null, true);
            if (fullResults)
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
        private string[] FindPlagiarism(string inputtext, string comptext)
        {
            var separator = new string[5];
            try
            {
                separator[0] = "\n";
                separator[1] = "\t";
                separator[2] = "!";
                separator[3] = "?";
                separator[4] = ".";
                var shift = 0;
                if (testbutton.Checked || _splitinputtext == null)
                {
                    _splitinputtext = inputtext.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                }
                var split2 = comptext.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                var result = new string[_splitinputtext.Length];
                foreach (string t in _splitinputtext)
                {
                    foreach (string t1 in split2.Where(t1 => !(Shingles.CompareStrings(t, t1, 2) <= 40)))
                    {
                        result[shift++] = t1;
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
            var enumeratefiles = dirInfo.EnumerateFiles("*." + filesformat.SelectedItem);
            var fileInfos = enumeratefiles as IList<FileInfo> ?? enumeratefiles.ToList();
            foreach (var fileinfo in fileInfos)
            {
                var comparer = new BackgroundWorker();
                comparer.DoWork += ComparerOnDoWork;
                comparer.RunWorkerCompleted += (sender, args) =>
                {
                    Monitor.Enter(_resultboxblock);
                    compared++;
                    resultbox.Items.Insert(0, args.Result);
                    progressBar1.Value += (100 - 40) / fileInfos.Count();
                    Monitor.Exit(_resultboxblock);
                };
                comparer.RunWorkerAsync(fileinfo.FullName);
            }
            while (compared < fileInfos.Count())
            {
            }
        }

        /// <summary>
        /// Проверяет тестсет
        /// </summary>
        /// <returns></returns>
        private void Test()
        {
            for (var i = 'a'; i <= 'e'; i++)
            {
                var compared = 0;
                _inputText = ReadFile("./Testset/InputFiles/orig_task" + i + ".txt");
                var dirInfo = new DirectoryInfo("./Testset/Collection");
                var fileInfos = dirInfo.EnumerateFiles("*" + i + ".txt").ToList();
                foreach (var fileInfo in fileInfos)
                {
                    var comparer = new BackgroundWorker();
                    comparer.DoWork += ComparerOnDoWork;
                    comparer.RunWorkerCompleted += (sender, args) =>
                        {
                            var results = (Results)args.Result;
                            Monitor.Enter(_resultboxblock);
                            compared++;
                                resultbox.Items.Insert(0, results.ToText());
                                progressBar1.Value += (100 - progressBar1.Value) / fileInfos.Count();
                            Monitor.Exit(_resultboxblock);
                    };
                    comparer.RunWorkerAsync(fileInfo.FullName);
                }
                while (compared < 19)
                {
                }
            }
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
            mainWorker.ProgressChanged += OnChanged;
            mainWorker.RunWorkerCompleted += OnCompleted;
            _timer = new Timer {Interval = 1000};
            _timer.Tick += TimerOnTick;
            _timer.Start();
            panel3.Visible = true;
            mainWorker.RunWorkerAsync();
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
                mainWorker.ReportProgress(0);
                ReadInputFile(openFileDialog1.FileName);
                mainWorker.ReportProgress(10);
                mainWorker.ReportProgress(12);
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
                    DownloadAndCheck(Convert.ToInt32(filescount.SelectedItem));
                }
                else
                {
                    StatisticCollect();
                }
            }
            WriteResultsToFile();
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

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            filenamelabel.Visible = true;
            filenamelabel.Text = @"Файл выбран: " + new FileInfo(openFileDialog1.FileName).Name;
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
    }
    public class Results
    {
        public Results(string filename, double similarity, int level, string fail, bool plagiarism)
        {
            _plagiarism = plagiarism;
            _filename = filename;
            _similarity = similarity;
            _level = level;
            _fail = fail;
        }
        public string ToText()
        {
            if (_fail == null)
            {
                return _filename + ". Plagiarism: "+ (_plagiarism ? 1 : 0) + ". Similarity: " + _similarity + ". Level: " + _level;
            }
            return _filename + ": " + _fail;
        }

        private readonly bool _plagiarism;
        private readonly string _filename;
        private readonly double _similarity;
        private readonly int _level;
        public string[] PlagiarisedSentences;
        private readonly string _fail;
    }
}