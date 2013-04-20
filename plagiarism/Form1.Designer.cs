namespace plagiarism
{
    partial class Form1
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                _sr.Dispose();
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.panel1 = new System.Windows.Forms.Panel();
            this.engbutton = new System.Windows.Forms.RadioButton();
            this.rusbutton = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.deletecollect = new System.Windows.Forms.Panel();
            this.deletecheck = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.panel5 = new System.Windows.Forms.Panel();
            this.collectionbutton = new System.Windows.Forms.RadioButton();
            this.googlebutton = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.ShowKeywords = new System.Windows.Forms.Panel();
            this.KeywordsCheck = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.nocompbutton = new System.Windows.Forms.RadioButton();
            this.shinglebutton = new System.Windows.Forms.RadioButton();
            this.kernelbutton = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.panel6 = new System.Windows.Forms.Panel();
            this.filenamelabel = new System.Windows.Forms.Label();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.button1 = new System.Windows.Forms.Button();
            this.start = new System.Windows.Forms.Button();
            this.shapeContainer1 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            this.lineShape1 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.panel3 = new System.Windows.Forms.Panel();
            this.resultbox = new System.Windows.Forms.ListBox();
            this.results = new System.Windows.Forms.Label();
            this.finishlabel = new System.Windows.Forms.Label();
            this.seconds = new System.Windows.Forms.Label();
            this.timerlabel = new System.Windows.Forms.Label();
            this.timer = new System.Windows.Forms.Label();
            this.cur_op = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.deletecollect.SuspendLayout();
            this.panel5.SuspendLayout();
            this.ShowKeywords.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog1_FileOk);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.engbutton);
            this.panel1.Controls.Add(this.rusbutton);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(2, 65);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(248, 31);
            this.panel1.TabIndex = 5;
            // 
            // engbutton
            // 
            this.engbutton.AutoSize = true;
            this.engbutton.Checked = true;
            this.engbutton.Location = new System.Drawing.Point(83, 7);
            this.engbutton.Name = "engbutton";
            this.engbutton.Size = new System.Drawing.Size(85, 17);
            this.engbutton.TabIndex = 6;
            this.engbutton.TabStop = true;
            this.engbutton.Text = "Английский";
            this.engbutton.UseVisualStyleBackColor = true;
            // 
            // rusbutton
            // 
            this.rusbutton.AutoSize = true;
            this.rusbutton.Location = new System.Drawing.Point(174, 7);
            this.rusbutton.Name = "rusbutton";
            this.rusbutton.Size = new System.Drawing.Size(67, 17);
            this.rusbutton.TabIndex = 5;
            this.rusbutton.Text = "Русский";
            this.rusbutton.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Язык текста:";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.deletecollect);
            this.panel2.Controls.Add(this.panel1);
            this.panel2.Controls.Add(this.panel5);
            this.panel2.Controls.Add(this.ShowKeywords);
            this.panel2.Controls.Add(this.panel4);
            this.panel2.Controls.Add(this.panel6);
            this.panel2.Controls.Add(this.splitter1);
            this.panel2.Controls.Add(this.button1);
            this.panel2.Controls.Add(this.start);
            this.panel2.Controls.Add(this.shapeContainer1);
            this.panel2.Location = new System.Drawing.Point(-2, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(706, 320);
            this.panel2.TabIndex = 6;
            // 
            // deletecollect
            // 
            this.deletecollect.Controls.Add(this.deletecheck);
            this.deletecollect.Controls.Add(this.label5);
            this.deletecollect.Location = new System.Drawing.Point(3, 189);
            this.deletecollect.Margin = new System.Windows.Forms.Padding(0);
            this.deletecollect.Name = "deletecollect";
            this.deletecollect.Size = new System.Drawing.Size(248, 31);
            this.deletecollect.TabIndex = 14;
            this.deletecollect.Visible = false;
            // 
            // deletecheck
            // 
            this.deletecheck.AutoSize = true;
            this.deletecheck.Checked = true;
            this.deletecheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.deletecheck.Location = new System.Drawing.Point(185, 9);
            this.deletecheck.Name = "deletecheck";
            this.deletecheck.Size = new System.Drawing.Size(15, 14);
            this.deletecheck.TabIndex = 5;
            this.deletecheck.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(2, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(177, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Удалить имеющуюся коллекцию:";
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.collectionbutton);
            this.panel5.Controls.Add(this.googlebutton);
            this.panel5.Controls.Add(this.label4);
            this.panel5.Location = new System.Drawing.Point(2, 158);
            this.panel5.Margin = new System.Windows.Forms.Padding(0);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(299, 31);
            this.panel5.TabIndex = 12;
            // 
            // collectionbutton
            // 
            this.collectionbutton.AutoSize = true;
            this.collectionbutton.Checked = true;
            this.collectionbutton.Location = new System.Drawing.Point(212, 7);
            this.collectionbutton.Name = "collectionbutton";
            this.collectionbutton.Size = new System.Drawing.Size(80, 17);
            this.collectionbutton.TabIndex = 6;
            this.collectionbutton.TabStop = true;
            this.collectionbutton.Text = "Коллекция";
            this.collectionbutton.UseVisualStyleBackColor = true;
            this.collectionbutton.CheckedChanged += new System.EventHandler(this.collectionbutton_CheckedChanged);
            // 
            // googlebutton
            // 
            this.googlebutton.AutoSize = true;
            this.googlebutton.Location = new System.Drawing.Point(133, 7);
            this.googlebutton.Name = "googlebutton";
            this.googlebutton.Size = new System.Drawing.Size(73, 17);
            this.googlebutton.TabIndex = 5;
            this.googlebutton.Text = "Интернет";
            this.googlebutton.UseVisualStyleBackColor = true;
            this.googlebutton.CheckedChanged += new System.EventHandler(this.googlebutton_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(2, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(125, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Файлы для сравнения:";
            // 
            // ShowKeywords
            // 
            this.ShowKeywords.Controls.Add(this.KeywordsCheck);
            this.ShowKeywords.Controls.Add(this.label2);
            this.ShowKeywords.Location = new System.Drawing.Point(2, 96);
            this.ShowKeywords.Margin = new System.Windows.Forms.Padding(0);
            this.ShowKeywords.Name = "ShowKeywords";
            this.ShowKeywords.Size = new System.Drawing.Size(248, 31);
            this.ShowKeywords.TabIndex = 6;
            // 
            // KeywordsCheck
            // 
            this.KeywordsCheck.AutoSize = true;
            this.KeywordsCheck.Location = new System.Drawing.Point(163, 9);
            this.KeywordsCheck.Name = "KeywordsCheck";
            this.KeywordsCheck.Size = new System.Drawing.Size(15, 14);
            this.KeywordsCheck.TabIndex = 5;
            this.KeywordsCheck.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(2, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(146, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Показать ключевые слова:";
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.nocompbutton);
            this.panel4.Controls.Add(this.shinglebutton);
            this.panel4.Controls.Add(this.kernelbutton);
            this.panel4.Controls.Add(this.label3);
            this.panel4.Location = new System.Drawing.Point(2, 127);
            this.panel4.Margin = new System.Windows.Forms.Padding(0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(340, 31);
            this.panel4.TabIndex = 11;
            // 
            // nocompbutton
            // 
            this.nocompbutton.AutoSize = true;
            this.nocompbutton.Location = new System.Drawing.Point(240, 7);
            this.nocompbutton.Name = "nocompbutton";
            this.nocompbutton.Size = new System.Drawing.Size(93, 17);
            this.nocompbutton.TabIndex = 7;
            this.nocompbutton.Text = "не проверять";
            this.nocompbutton.UseVisualStyleBackColor = true;
            this.nocompbutton.CheckedChanged += new System.EventHandler(this.nocompbutton_CheckedChanged);
            // 
            // shinglebutton
            // 
            this.shinglebutton.AutoSize = true;
            this.shinglebutton.Checked = true;
            this.shinglebutton.Location = new System.Drawing.Point(101, 7);
            this.shinglebutton.Name = "shinglebutton";
            this.shinglebutton.Size = new System.Drawing.Size(65, 17);
            this.shinglebutton.TabIndex = 6;
            this.shinglebutton.TabStop = true;
            this.shinglebutton.Text = "Шинглы";
            this.shinglebutton.UseVisualStyleBackColor = true;
            this.shinglebutton.CheckedChanged += new System.EventHandler(this.shinglebutton_CheckedChanged);
            // 
            // kernelbutton
            // 
            this.kernelbutton.AutoSize = true;
            this.kernelbutton.Location = new System.Drawing.Point(172, 7);
            this.kernelbutton.Name = "kernelbutton";
            this.kernelbutton.Size = new System.Drawing.Size(62, 17);
            this.kernelbutton.TabIndex = 5;
            this.kernelbutton.Text = "Кернел";
            this.kernelbutton.UseVisualStyleBackColor = true;
            this.kernelbutton.CheckedChanged += new System.EventHandler(this.kernelbutton_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(2, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(93, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Метод проверки:";
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.filenamelabel);
            this.panel6.Location = new System.Drawing.Point(3, 34);
            this.panel6.Margin = new System.Windows.Forms.Padding(0);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(480, 31);
            this.panel6.TabIndex = 13;
            // 
            // filenamelabel
            // 
            this.filenamelabel.AutoSize = true;
            this.filenamelabel.Location = new System.Drawing.Point(6, 9);
            this.filenamelabel.Name = "filenamelabel";
            this.filenamelabel.Size = new System.Drawing.Size(80, 13);
            this.filenamelabel.TabIndex = 4;
            this.filenamelabel.Text = "Файл выбран:";
            this.filenamelabel.Visible = false;
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(0, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 320);
            this.splitter1.TabIndex = 9;
            this.splitter1.TabStop = false;
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button1.Location = new System.Drawing.Point(3, 0);
            this.button1.Margin = new System.Windows.Forms.Padding(0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(248, 34);
            this.button1.TabIndex = 8;
            this.button1.Text = "Выбрать файл для проверки";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // start
            // 
            this.start.Font = new System.Drawing.Font("Microsoft Sans Serif", 32F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.start.Location = new System.Drawing.Point(486, 0);
            this.start.Margin = new System.Windows.Forms.Padding(0);
            this.start.Name = "start";
            this.start.Size = new System.Drawing.Size(219, 319);
            this.start.TabIndex = 7;
            this.start.Text = "Запуск";
            this.start.UseVisualStyleBackColor = true;
            this.start.Click += new System.EventHandler(this.start_Click);
            // 
            // shapeContainer1
            // 
            this.shapeContainer1.Location = new System.Drawing.Point(0, 0);
            this.shapeContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.shapeContainer1.Name = "shapeContainer1";
            this.shapeContainer1.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
            this.lineShape1});
            this.shapeContainer1.Size = new System.Drawing.Size(706, 320);
            this.shapeContainer1.TabIndex = 10;
            this.shapeContainer1.TabStop = false;
            // 
            // lineShape1
            // 
            this.lineShape1.Name = "lineShape1";
            this.lineShape1.X1 = 0;
            this.lineShape1.X2 = 75;
            this.lineShape1.Y1 = 0;
            this.lineShape1.Y2 = 23;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.resultbox);
            this.panel3.Controls.Add(this.results);
            this.panel3.Controls.Add(this.finishlabel);
            this.panel3.Controls.Add(this.seconds);
            this.panel3.Controls.Add(this.timerlabel);
            this.panel3.Controls.Add(this.timer);
            this.panel3.Controls.Add(this.cur_op);
            this.panel3.Controls.Add(this.label6);
            this.panel3.Controls.Add(this.progressBar1);
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(704, 320);
            this.panel3.TabIndex = 7;
            this.panel3.Visible = false;
            // 
            // resultbox
            // 
            this.resultbox.FormattingEnabled = true;
            this.resultbox.Location = new System.Drawing.Point(1, 30);
            this.resultbox.Name = "resultbox";
            this.resultbox.Size = new System.Drawing.Size(702, 147);
            this.resultbox.TabIndex = 9;
            // 
            // results
            // 
            this.results.AutoSize = true;
            this.results.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.results.Location = new System.Drawing.Point(305, 9);
            this.results.Name = "results";
            this.results.Size = new System.Drawing.Size(90, 17);
            this.results.TabIndex = 8;
            this.results.Text = "Результаты:";
            // 
            // finishlabel
            // 
            this.finishlabel.AutoSize = true;
            this.finishlabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.finishlabel.Location = new System.Drawing.Point(7, 3);
            this.finishlabel.Name = "finishlabel";
            this.finishlabel.Size = new System.Drawing.Size(0, 13);
            this.finishlabel.TabIndex = 7;
            // 
            // seconds
            // 
            this.seconds.AutoSize = true;
            this.seconds.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.seconds.Location = new System.Drawing.Point(428, 270);
            this.seconds.Name = "seconds";
            this.seconds.Size = new System.Drawing.Size(53, 17);
            this.seconds.TabIndex = 6;
            this.seconds.Text = "секунд";
            // 
            // timerlabel
            // 
            this.timerlabel.AutoSize = true;
            this.timerlabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.timerlabel.Location = new System.Drawing.Point(389, 270);
            this.timerlabel.Name = "timerlabel";
            this.timerlabel.Size = new System.Drawing.Size(16, 17);
            this.timerlabel.TabIndex = 5;
            this.timerlabel.Text = "0";
            // 
            // timer
            // 
            this.timer.AutoSize = true;
            this.timer.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.timer.Location = new System.Drawing.Point(217, 270);
            this.timer.Name = "timer";
            this.timer.Size = new System.Drawing.Size(125, 17);
            this.timer.TabIndex = 4;
            this.timer.Text = "Времени прошло:";
            // 
            // cur_op
            // 
            this.cur_op.AutoSize = true;
            this.cur_op.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cur_op.Location = new System.Drawing.Point(389, 189);
            this.cur_op.Name = "cur_op";
            this.cur_op.Size = new System.Drawing.Size(45, 17);
            this.cur_op.TabIndex = 3;
            this.cur_op.Text = "старт";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label6.Location = new System.Drawing.Point(155, 189);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(138, 17);
            this.label6.TabIndex = 2;
            this.label6.Text = "Текущая операция:";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(134, 219);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(443, 34);
            this.progressBar1.TabIndex = 0;
            this.progressBar1.Click += new System.EventHandler(this.progressBar1_Click);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(704, 319);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Form1";
            this.Text = "Form1";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.deletecollect.ResumeLayout(false);
            this.deletecollect.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.ShowKeywords.ResumeLayout(false);
            this.ShowKeywords.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton engbutton;
        private System.Windows.Forms.RadioButton rusbutton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel ShowKeywords;
        private System.Windows.Forms.CheckBox KeywordsCheck;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button start;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Splitter splitter1;
        private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer1;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape1;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.RadioButton shinglebutton;
        private System.Windows.Forms.RadioButton kernelbutton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton nocompbutton;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.RadioButton collectionbutton;
        private System.Windows.Forms.RadioButton googlebutton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label cur_op;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label timer;
        private System.Windows.Forms.Label seconds;
        private System.Windows.Forms.Label timerlabel;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Label filenamelabel;
        private System.Windows.Forms.Label finishlabel;
        private System.Windows.Forms.Label results;
        private System.Windows.Forms.ListBox resultbox;
        private System.Windows.Forms.Panel deletecollect;
        private System.Windows.Forms.CheckBox deletecheck;
        private System.Windows.Forms.Label label5;
    }
}

