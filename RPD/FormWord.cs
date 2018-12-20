﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Data.OleDb;
using excel = Microsoft.Office.Interop.Excel; // подключение библиотеки excel и создание псевдонима "Alias"
using word = Microsoft.Office.Interop.Word; // подключение библиотеки word и создание псевдонима "Alias"
using System.Diagnostics;

namespace RPD
{
    public partial class FormWord : Form
    {
        connection_to_bd BD = new connection_to_bd();
        DataAccess DA;
        int Leck = 0;
        int Lab = 0;
        int PR = 0;
        private string _FileNaim;
        private string _FileNaim_FOS;
        private string _FileNaim_ANAT;
        private int _ID_Prof;
        public int ID_Prof // id профиля
        {
            get { return _ID_Prof; }
            set { _ID_Prof = value; }
        }
        private int _ID;
        public string FileNaim // путь к шаблону НРП
        {
            get { return _FileNaim; }
            set { _FileNaim = value; }
        }
        public string FileNaim_FOS // путь к шаблону НРП
        {
            get { return _FileNaim_FOS; }
            set { _FileNaim_FOS = value; }
        }
        public string FileNaim_ANAT // путь к шаблону НРП
        {
            get { return _FileNaim_ANAT; }
            set { _FileNaim_ANAT = value; }
        }
        public int ID
        {
            get { return _ID; }
            set { _ID = value; }
        } // id Дисциплины
        private int ID_Napr; // id Направление Подготовки
        public List<int> TemInSem = new List<int>();
        public List<string> ListControl = new List<string>(); // Запись по семестра вида контроля (Экзамен, зачет, диф зачет)



        string Filename_;
        public static bool btn1;
        Tema tems;
        Discipline dis;
        public Dis D = new Dis(); /*Класс*/
        char[] MyChar = { '\f', '\n', '\r', '\t', '\v', '\0', ' ', '2', '3', '.', ')', ';' };
        int CountKFind;  //' счетчик найденных фрагментов, n-сколько надо отсчитать нахождений до нужного
        word.Application WordApp;
        private int sec; // переменная, содержащая значение времени
        public FormWord()
        {
            InitializeComponent();
            sec = 0;
        }


        public string SearchText(string wordText1, string wordText2, int nf) // Поиск между двумя фрагментами - метод поиска 
        {
            Microsoft.Office.Interop.Word.Range r;//Range
            string st;
            st = "";
           // для чего? r = WordApp.ActiveDocument.Range(WordApp.ActiveDocument.TablesOfContents[1].Range.End, WordApp.ActiveDocument.Range().End);// попытка обойти содержание (tableofcontent - содержание)
            r = WordApp.ActiveDocument.Range();
           
            bool f;
            f = false;
            int firstOccurence;
            firstOccurence = 0;
            CountKFind = 0;
            r.Find.ClearFormatting(); //Сброс форматирований из предыдущих операций поиска
            r.Find.Text = wordText1 + "*" + wordText2;
            r.Find.Forward = true;
            r.Find.Wrap = word.WdFindWrap.wdFindContinue; //при достижении конца документа поиск будет продолжаться с начала пока не будет достигнуто положение начала поиска
            r.Find.Format = false;
            r.Find.MatchCase = false;
            r.Find.MatchWholeWord = false;
            r.Find.MatchAllWordForms = false;
            r.Find.MatchSoundsLike = false;
            r.Find.MatchWildcards = true;//подстановочные знаки ВКЛ
            while (r.Find.Execute() == true) // Проверка поиска, если нашёл фрагменты, то...
            {
                CountKFind = CountKFind + 1;// то счётчик найденных фрагментоd увеличивается на 1
                if (f)
                {
                    if (r.Start == firstOccurence)
                    { }
                    else
                    {
                        firstOccurence = r.Start;
                        f = true;
                    }
                }
                st = WordApp.ActiveDocument.Range(r.Start + wordText1.Length, r.End - wordText2.Length).Text; //убираем кл.
                r.Start = r.Start + wordText1.Length;
                r.End = r.End - wordText2.Length;
                if (CountKFind >= nf) // если нужный по счету фрагмент найден
                {
                    // r = WordApp.ActiveDocument.Range(r.Start, r.End);
                    break;
                }
            }

            CountKFind = 0;

            if (r.Text != "")
            {
                if (st != "")
                {
                    r.Copy();
                }
                else //' если текст не найден очистим буфер обмена
                {
                    Clipboard.Clear();
                }
            }
            else
            {
                {
                    Clipboard.Clear();
                }
            }

            return st;
        }

        public void CloseProcess()
        {
            Process[] List;
            List = Process.GetProcessesByName("WORD");
            foreach (Process proc in List)
            {
                proc.Kill();
            }
        }
        private void FormWord_Load(object sender, EventArgs e)
        {

        }

        public bool AnalysisPattern(bool Flag)
        {
            Microsoft.Office.Interop.Word.Range r;
            r = WordApp.ActiveDocument.Range();
            r.Find.ClearFormatting(); //Сброс форматирований из предыдущих операций поиска 
            r.Find.Forward = true;
            r.Find.Format = true;
            r.Find.Wrap = word.WdFindWrap.wdFindContinue; //при достижении конца документа поиск будет продолжаться с начала пока не будет достигнуто положение начала поиска
            r.Find.MatchWildcards = true;//подстановочные знаки ВКЛ
            Flag = false;
            r.Find.Text = "#Индекс";
            string SearhWord1 = r.Find.Text;
            if (r.Find.Execute(SearhWord1) == true)
            {
                r.Find.Text = "#Дисциплина";
                string SearhWord2 = r.Find.Text;
                if (r.Find.Execute(SearhWord2) == true)
                {

                    r.Find.Text = "#Направление";
                    string SearhWord3 = r.Find.Text;
                    if (r.Find.Execute(SearhWord3) == true)
                    {
                        r.Find.Text = "#ДатаФГОС";
                        string SearhWord4 = r.Find.Text;
                        if (r.Find.Execute(SearhWord4) == true)
                        {
                            r.Find.Text = "#НомерФГОС";
                            string SearhWord5 = r.Find.Text;
                            if (r.Find.Execute(SearhWord5) == true)
                            {
                                r.Find.Text = "#Цели";
                                string SearhWord6 = r.Find.Text;
                                if (r.Find.Execute(SearhWord6) == true)
                                {
                                    r.Find.Text = "#Задачи";
                                    string SearhWord7 = r.Find.Text;
                                    if (r.Find.Execute(SearhWord7) == true)
                                    {
                                        r.Find.Text = "#Часть";
                                        string SearhWord8 = r.Find.Text;
                                        if (r.Find.Execute(SearhWord8) == true)
                                        {
                                            r.Find.Text = "#ДисциплиныДО";
                                            string SearhWord9 = r.Find.Text;
                                            if (r.Find.Execute(SearhWord9) == true)
                                            {
                                                r.Find.Text = "#ЗнатьДО";
                                                string SearhWord10 = r.Find.Text;
                                                if (r.Find.Execute(SearhWord10) == true)
                                                {
                                                    r.Find.Text = "#УметьДО";
                                                    string SearhWord11 = r.Find.Text;
                                                    if (r.Find.Execute(SearhWord11) == true)
                                                    {
                                                        r.Find.Text = "#ВладетьДО";
                                                        string SearhWord12 = r.Find.Text;
                                                        if (r.Find.Execute(SearhWord12) == true)
                                                        {
                                                            r.Find.Text = "#ДисциплиныПосле";
                                                            string SearhWord13 = r.Find.Text;
                                                            if (r.Find.Execute(SearhWord13) == true)
                                                            {
                                                                r.Find.Text = "#зе";
                                                                string SearhWord14 = r.Find.Text;
                                                                if (r.Find.Execute(SearhWord14) == true)
                                                                {
                                                                    r.Find.Text = "#че";
                                                                    string SearhWord15 = r.Find.Text;
                                                                    if (r.Find.Execute(SearhWord15) == true)
                                                                    {
                                                                        r.Find.Text = "#конт";
                                                                        string SearhWord16 = r.Find.Text;
                                                                        if (r.Find.Execute(SearhWord16) == true)
                                                                        {
                                                                            r.Find.Text = "#аудит";
                                                                            string SearhWord17 = r.Find.Text;
                                                                            if (r.Find.Execute(SearhWord17) == true)
                                                                            {
                                                                                r.Find.Text = "#лек";
                                                                                string SearhWord18 = r.Find.Text;
                                                                                if (r.Find.Execute(SearhWord18) == true)
                                                                                {
                                                                                    r.Find.Text = "#лаб";
                                                                                    string SearhWord19 = r.Find.Text;
                                                                                    if (r.Find.Execute(SearhWord19) == true)
                                                                                    {
                                                                                        r.Find.Text = "#пр";
                                                                                        string SearhWord20 = r.Find.Text;
                                                                                        if (r.Find.Execute(SearhWord20) == true)
                                                                                        {
                                                                                            r.Find.Text = "#инт";
                                                                                            string SearhWord21 = r.Find.Text;
                                                                                            if (r.Find.Execute(SearhWord21) == true)
                                                                                            {
                                                                                                r.Find.Text = "#эл";
                                                                                                string SearhWord22 = r.Find.Text;
                                                                                                if (r.Find.Execute(SearhWord22) == true)
                                                                                                {
                                                                                                    r.Find.Text = "#срс";
                                                                                                    string SearhWord23 = r.Find.Text;
                                                                                                    if (r.Find.Execute(SearhWord23) == true)
                                                                                                    {
                                                                                                        r.Find.Text = "#контр";
                                                                                                        string SearhWord24 = r.Find.Text;
                                                                                                        if (r.Find.Execute(SearhWord24) == true)
                                                                                                        {
                                                                                                            r.Find.Text = "#Основная_л";
                                                                                                            string SearhWord25 = r.Find.Text;
                                                                                                            if (r.Find.Execute(SearhWord25) == true)
                                                                                                            {
                                                                                                                r.Find.Text = "#Дополнит_л";
                                                                                                                string SearhWord26 = r.Find.Text;
                                                                                                                if (r.Find.Execute(SearhWord26) == true)
                                                                                                                {
                                                                                                                    r.Find.Text = "#Посещение балла";
                                                                                                                    string SearhWord27 = r.Find.Text;
                                                                                                                    if (r.Find.Execute(SearhWord27) == true)
                                                                                                                    {
                                                                                                                        rtb_Log.AppendText("Шаблон корректен\n", Color.Green);
                                                                                                                        return Flag = true;
                                                                                                                    }
                                                                                                                    else { rtb_Log.AppendText("Шаблон некорректен\n", Color.Red); }
                                                                                                                }
                                                                                                                else { rtb_Log.AppendText("Шаблон некорректен\n", Color.Red); }
                                                                                                            }
                                                                                                            else { rtb_Log.AppendText("Шаблон некорректен\n", Color.Red); }
                                                                                                        }
                                                                                                        else { rtb_Log.AppendText("Шаблон некорректен\n", Color.Red); }
                                                                                                    }
                                                                                                    else { rtb_Log.AppendText("Шаблон некорректен\n", Color.Red); }
                                                                                                }
                                                                                                else { rtb_Log.AppendText("Шаблон некорректен\n", Color.Red); }
                                                                                            }
                                                                                            else { rtb_Log.AppendText("Шаблон некорректен\n", Color.Red); }
                                                                                        }
                                                                                        else { rtb_Log.AppendText("Шаблон некорректен\n", Color.Red); }
                                                                                    }
                                                                                    else { rtb_Log.AppendText("Шаблон некорректен\n", Color.Red); }
                                                                                }
                                                                                else { rtb_Log.AppendText("Шаблон некорректен\n", Color.Red); }
                                                                            }
                                                                            else { rtb_Log.AppendText("Шаблон некорректен\n", Color.Red); }
                                                                        }
                                                                        else { rtb_Log.AppendText("Шаблон некорректен\n", Color.Red); }
                                                                    }
                                                                    else { rtb_Log.AppendText("Шаблон некорректен\n", Color.Red); }
                                                                }
                                                                else { rtb_Log.AppendText("Шаблон некорректен\n", Color.Red); }
                                                            }
                                                            else { rtb_Log.AppendText("Шаблон некорректен\n", Color.Red); }
                                                        }
                                                        else { rtb_Log.AppendText("Шаблон некорректен\n", Color.Red); }
                                                    }
                                                    else { rtb_Log.AppendText("Шаблон некорректен\n", Color.Red); }
                                                }
                                                else { rtb_Log.AppendText("Шаблон некорректен\n", Color.Red); }
                                            }
                                            else { rtb_Log.AppendText("Шаблон некорректен\n", Color.Red); }
                                        }
                                        else { rtb_Log.AppendText("Шаблон некорректен\n", Color.Red); }
                                    }
                                    else { rtb_Log.AppendText("Шаблон некорректен\n", Color.Red); }
                                }
                                else { rtb_Log.AppendText("Шаблон некорректен\n", Color.Red); }
                            }
                            else { rtb_Log.AppendText("Шаблон некорректен\n", Color.Red); }
                        }
                        else { rtb_Log.AppendText("Шаблон некорректен\n", Color.Red); }
                    }
                    else { rtb_Log.AppendText("Шаблон некорректен\n", Color.Red); }
                }
                else { rtb_Log.AppendText("Шаблон некорректен\n", Color.Red); }
            }
            else { rtb_Log.AppendText("Шаблон некорректен\n", Color.Red); }
            return Flag = false;





        }
        private void AnalysisOldProgramm()
        {

            WordApp = new word.Application(); // создаем объект word;
            WordApp.AutomationSecurity = Microsoft.Office.Core.MsoAutomationSecurity.msoAutomationSecurityLow;
            WordApp.FileValidation = Microsoft.Office.Core.MsoFileValidationMode.msoFileValidationSkip;
            WordApp.Visible = true; // показывает или скрывает файл word;
            openFileDialog1.Filter = "Файлы Word (*.doc;*.docx)|*.doc;*.docx|All files (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            { Filename_ = openFileDialog1.FileName; }
            if (Filename_ == null)
            {
                return;
            }
            // фильтрует, оставляя только ворд файлы
            Filename_ = openFileDialog1.FileName;
            WordApp.Documents.Open(Filename_, ReadOnly: true);
            WordApp.Application.Selection.GoTo(1,5, 5, 4);
            WordApp.Application.Selection.Bookmarks[@"\Page"].Select();
            WordApp.Application.Selection.Delete();
            //WordApp.Documents.Add(Filename_);
           


            Action action1 = () => { btn_OpenWp.Enabled = false; }; Invoke(action1);


            SearchText(textBox2.Text, textBox4.Text, CountKFind);
            int N = 0;
            int i = 0;
            int j = 0;
            progressBar1.Value = j;
            Microsoft.Office.Interop.Word.Range r;//Range
            Microsoft.Office.Interop.Word.ListParagraphs p;
            D.CreateLitera();
            string ss;
            ss = "";
            r = WordApp.ActiveDocument.Range();
            p = WordApp.ActiveDocument.ListParagraphs;
            word.Document document = WordApp.ActiveDocument;
            int NnN = document.ListParagraphs.Count;

            //Поиск литературы
            string str1 = "Основная литература";
            string str2 = "Дополнительная литература";
            string str3 = "Перечень";
            string BoldWordFlag = "";
            string gg1; string gg2;

            if (r.Find.Execute(r.Find.Text = "Основная литература:"))
            {
                str1 = "Основная литература:";
            }
            r = WordApp.ActiveDocument.Range();
            if (r.Find.Execute(r.Find.Text = "Дополнительная литература:"))
            {
                str2 = "Дополнительная литература:";
            }
            r = WordApp.ActiveDocument.Range();
            if (r.Find.Execute(r.Find.Text = "дополнительная литература", true))
            {
                str2 = "дополнительная литература";
            }
            r = WordApp.ActiveDocument.Range();
            // Поиск основной литературы
            r.Find.Text = str1 + "*" + str2;

            r.Find.Forward = true;
            string f1 = r.Find.Text;
            r.Find.Wrap = word.WdFindWrap.wdFindContinue; //при достижении конца документа поиск будет продолжаться с начала пока не будет достигнуто положение начала поиска
            r.Find.MatchWildcards = true;//подстановочные знаки ВКЛ

            if (r.Find.Execute(f1, false))// Проверка поиска, если нашёл фрагменты, то...
            {
                gg1 = WordApp.ActiveDocument.Range(r.Start + str1.Length, r.End - str2.Length).Text; //убираем кл.
                r.Start = r.Start + str1.Length;

                for (int k = 2; k <= r.Paragraphs.Count; k++)
                {
                    if (r.Paragraphs[k].Range.Bold < 0)
                    {

                        BoldWordFlag = r.Paragraphs[k].Range.Text;
                        if (BoldWordFlag != "\r") { break; }

                    }
                }
                r.End = r.End - BoldWordFlag.Length;
                gg1 = WordApp.ActiveDocument.Range(r.Start + str1.Length, r.End - BoldWordFlag.Length).Text;
                r.Find.Text = str1 + "*" + BoldWordFlag;
                D.LiteraBasicNoNum = SearchText(str1, BoldWordFlag, 1);
                if (D.LiteraBasicNoNum != "")
                {
                    rtb_LiteraBasic.Paste();
                    Action Progress = () => { rtb_Log.AppendText("Основная литература считана\n", Color.Green); }; Invoke(Progress);
                }
                else if (D.LiteraBasicNoNum == "")
                {
                    int m21 = r.ListParagraphs.Count;
                    if (m21 != 0)
                    {
                        object Start = r.ListParagraphs[1].Range.Start;
                        object End = r.ListParagraphs[m21].Range.End;
                        word.Range myRange = WordApp.ActiveDocument.Range(Start, End);
                        myRange.Copy();
                        rtb_LiteraBasic.Paste();
                        for (int y = 1; y <= r.ListParagraphs.Count; y++)
                        {
                            string dfs = r.ListParagraphs[y].Range.Text;
                            D.MyListAdd(dfs, false);
                        }
                        Action Progress = () => { rtb_Log.AppendText("Основная литература считана\n", Color.Green); }; Invoke(Progress);

                    }
                }
                else
                {
                    Action Progress = () => { rtb_Log.AppendText("Основная литература не найдена\n", Color.Red); }; Invoke(Progress);
                }

            }
            // поиск дополнительной литературы
            r.Find.Text = str2 + "*" + str3;
            r.Find.Forward = true;

            string f2 = r.Find.Text;
            r.Find.Wrap = word.WdFindWrap.wdFindContinue; //при достижении конца документа поиск будет продолжаться с начала пока не будет достигнуто положение начала поиска
            r.Find.MatchWildcards = true;//подстановочные знаки ВКЛ
            if (r.Find.Execute(f2))// Проверка поиска, если нашёл фрагменты, то...
            {
                gg2 = WordApp.ActiveDocument.Range(r.Start + str2.Length, r.End - str3.Length).Text; //убираем кл.
                r.Start = r.Start + str2.Length;
                for (int k = 2; k <= r.Paragraphs.Count; k++)
                {
                    if (r.Paragraphs[k].Range.Bold < 0)
                    {

                        BoldWordFlag = r.Paragraphs[k].Range.Text;
                        if (BoldWordFlag != "\r")
                        {
                            break;
                        }
                    }
                }
                r.End = r.End - BoldWordFlag.Length;
                gg2 = WordApp.ActiveDocument.Range(r.Start, r.End).Text;
                r.Find.Text = str2 + "*" + BoldWordFlag;

                BoldWordFlag = BoldWordFlag.Split(' ')[0];
                r.Find.Wrap = word.WdFindWrap.wdFindContinue;
                r.Find.MatchWildcards = true;
                //r = WordApp.ActiveDocument.Range(r.Start, r.End);
                D.LiteraAdditionalNoNum = SearchText(str2, BoldWordFlag, 1);
                if (D.LiteraAdditionalNoNum != "")
                {
                    //rtb_Add_Litera.Text = rtb_Add_Litera.Text + gg2;
                    rtb_Add_Litera.Paste();
                    Action Progress = () => { rtb_Log.AppendText("Дополнительная литература считана\n", Color.Green); }; Invoke(Progress);
                }
                else if (D.LiteraAdditionalNoNum == "")
                {
                    int m12 = r.ListParagraphs.Count;
                    if (m12 != 0)
                    {
                        object Start = r.ListParagraphs[1].Range.Start;
                        object End = r.ListParagraphs[m12].Range.End;
                        word.Range myRange = WordApp.ActiveDocument.Range(Start, End);
                        myRange.Copy();
                        rtb_Add_Litera.Paste();
                        for (int x = 1; x <= r.ListParagraphs.Count; x++)
                        {
                            string dsf = r.ListParagraphs[x].Range.Text;
                            D.MyListAdd(dsf, true);
                        }
                        Action Progress = () => { rtb_Log.AppendText("Дополнительная литература считана\n", Color.Green); }; Invoke(Progress);
                    }
                }
                else
                {
                    Action Progress = () => { rtb_Log.AppendText("Дополнительная литература не найдена\n", Color.Red); }; Invoke(Progress);
                }

            } // поиск закончился, литература записана в массив


            //находим цели дисциплины
            ss = SearchText("явля?????", "Учебные задачи дисциплины", 2);
            if (ss == "") //' Если цели не попали в оглавление
            {
                ss = SearchText("явля?????", "Учебные задачи дисциплины", 1); // искомый текст после оглавления
                if (ss == "")
                {
                    Action Progress = () => { rtb_Log.AppendText("Цели дисциплины не найдены\n", Color.Red); }; Invoke(Progress);
                }

            }
            else { Action Progress = () => { rtb_Log.AppendText("Цели дисциплины найдены\n", Color.Green); }; Invoke(Progress); }

            ss = ss.TrimEnd(MyChar);
            N = ss.IndexOf("явля");
            if (N > 0 && N < ss.Length - 9)
            {
                D.Cel = ss.Remove(1, N + 9);
            }
            else
            {
                D.Cel = ss;// записали переменную цель
            }



            //' Находим задачи и оставляем все после слова "является" или "являются:"
            ss = SearchText("Учебные задачи дисциплины", "Место дисциплины", 2);
            if (ss == "")// ' Если задачи не попали в оглавление
            {
                ss = SearchText("Учебные задачи дисциплины", "Место дисциплины", 1);
                if (ss == "")
                {
                    Action Progress = () => { rtb_Log.AppendText("Задачи дисциплины не найдены\n", Color.Red); }; Invoke(Progress);
                }

            }
            else { Action Progress = () => { rtb_Log.AppendText("Задачи дисциплины найдены\n", Color.Green); }; Invoke(Progress); }

            ss = ss.TrimEnd(MyChar);
            N = ss.IndexOf("явля");

            if (N > 0 && N < ss.Length - 9)
            {
                D.Tasks = ss.Remove(1, N + 9);
            }
            else
            {
                D.Tasks = ss; // записали цели
            }

            //Находим знания, умения и владения и оставляем все до знаков препинания и символов перевода, или цифр 2, 3.
            ss = SearchText("Знать:", "Уметь:", 1);
            D.Zn_before = ss.TrimEnd(MyChar);
            ss = SearchText("Уметь:", "Владеть:", 1);
            D.Um_before = ss.TrimEnd(MyChar);
            ss = SearchText("Владеть:", ".", 1);
            D.Vl_before = ss.TrimEnd(MyChar);
            ss = SearchText("Знать:", "Уметь:", 2);
            D.Zn_after = ss.TrimEnd(MyChar);
            ss = SearchText("Уметь:", "Владеть:", 2);
            D.Um_after = ss.TrimEnd(MyChar);
            ss = SearchText("Владеть:", ".", 2);
            D.Vl_after = ss.TrimEnd(MyChar);
            if (ss == "")
            {
                Action Progress = () => { rtb_Log.AppendText("Знания, умения, навыки до не найдены\n", Color.Red); }; Invoke(Progress);
            }

            byte razd = 1;  //'номер раздела
            int CountTems = 0;
            r.Find.Text = "Наименование";
            string texttable = r.Find.Text;
            if (WordApp.ActiveDocument.Tables.Count != 0)
            {
                try
                {
                    for (i = 1; i <= WordApp.ActiveDocument.Tables.Count; i++)
                    {
                        if (WordApp.ActiveDocument.Tables[i].Cell(1, 2).Range.Find.Execute(texttable))
                        {

                            int k = 0; // счетчик кол-во тем
                            Action Progress = () => { rtb_Log.AppendText("Таблица с темами дисциплины считана\n", Color.Green); }; Invoke(Progress);
                            for (int n = 2; n <= WordApp.ActiveDocument.Tables[i].Rows.Count; n++)
                            {

                                if (WordApp.ActiveDocument.Tables[i].Rows[n].Cells.Count >= 5)
                                {
                                    if (WordApp.ActiveDocument.Tables[i].Rows[n].Cells[2].Range.Text.Length > 3) // проверка пустых значений названий тем
                                    {
                                        D.tems[k].Name = WordApp.ActiveDocument.Tables[i].Cell(n, 2).Range.Text;
                                        D.tems[k].Text = WordApp.ActiveDocument.Tables[i].Cell(n, 3).Range.Text;
                                        D.tems[k].Rez = WordApp.ActiveDocument.Tables[i].Cell(n, 5).Range.Text;
                                        D.tems[k].FormZ = WordApp.ActiveDocument.Tables[i].Cell(n, 6).Range.Text;
                                        CountTems++;
                                        k++; // кол-во тем
                                    }
                                }

                            }
                            break;
                        }
                        else
                        {
                            //Action Progress = () => { rtb_Log.AppendText("Таблица с темами дисциплины не найдена\n", Color.Red); }; Invoke(Progress);
                            if (i != 2)
                            {
                                razd += razd;  //' счетчик разделов срабатывает если их больше одного
                            }
                        }
                    }
                }
                catch { Action Progress = () => { rtb_Log.AppendText("Таблица с темами дисциплины не найдена\n", Color.Red); }; Invoke(Progress); }

                D.Nt = CountTems; //Записали количество тем в дисциплине
            }
            else
            {
                Action Progress = () => { rtb_Log.AppendText("Таблица с темами дисциплины не найдена\n", Color.Red); }; Invoke(Progress);
            }

            Clipboard.Clear();


            // считываются темы и их литература, вопросы для самопроверки


            ss = SearchText("Перечень учебно-методического обеспечения для самостоятельной работы обучающихся по дисциплине", "III. ОБРАЗОВАТЕЛЬНЫЕ ТЕХНОЛОГИИ", 2);

            if (ss.Contains("Тема 1.") & ss.Contains("Литература") & ss.Contains("Вопросы для самопроверки"))
            {

                rtb_Tems.Paste();
                rtb_Log.AppendText("Перечень УМО считаны\n", Color.Green);
            }
            else
            {
                ss = SearchText("Перечень учебно-методического обеспечения для самостоятельной работы обучающихся по дисциплине", "III. ОБРАЗОВАТЕЛЬНЫЕ ТЕХНОЛОГИИ", 1);
                if (ss.Contains("Тема 1.") & ss.Contains("Литература") & ss.Contains("Вопросы для самопроверки"))
                {
                    rtb_Tems.Paste();
                    rtb_Log.AppendText("Перечень УМО считаны\n", Color.Green);
                }
                else
                {
                    ss = SearchText("Перечень учебно-методического обеспечения для самостоятельной работы обучающихся по дисциплине", "Рекомендуемые обучающие", 2);

                    if (ss.Contains("Тема 1.") & ss.Contains("Литература") & ss.Contains("Вопросы для самопроверки"))
                    {
                        rtb_Tems.Paste();
                        rtb_Log.AppendText("Перечень УМО считаны\n", Color.Green);
                    }
                    else
                    {
                        ss = SearchText("Перечень учебно-методического обеспечения для самостоятельной работы обучающихся по дисциплине", "Рекомендуемые обучающие", 1);
                        if (ss.Contains("Тема 1.") & ss.Contains("Литература") & ss.Contains("Вопросы для самопроверки"))
                        {
                            rtb_Tems.Paste();
                            rtb_Log.AppendText("Перечень УМО считаны\n", Color.Green);
                        }
                        else
                        {
                            ss = SearchText("Перечень учебно-методического обеспечения для самостоятельной работы обучающихся по дисциплине", "Материально-техническое обеспечение дисциплины", 2);
                            if (ss.Contains("Тема") && ss.Contains("Вопросы для самопроверки"))
                            {
                                rtb_Tems.Paste();
                                rtb_Log.AppendText("Перечень УМО считаны\n", Color.Green);
                            }
                            else
                            {
                                ss = SearchText("Перечень учебно-методического обеспечения для самостоятельной работы обучающихся по дисциплине", "Материально-техническое обеспечение дисциплины", 1);
                                if (ss.Contains("Тема") && ss.Contains("Вопросы для самопроверки"))
                                {
                                    rtb_Tems.Paste();
                                    rtb_Log.AppendText("Перечень УМО считаны\n", Color.Green);
                                }
                                else
                                {
                                    ss = SearchText("Перечень учебно-методического обеспечения для самостоятельной работы обучающихся по дисциплине", "Перечень материально-технической базы", 2);
                                    if (ss.Contains("Тема 1.") & ss.Contains("Литература") & ss.Contains("Вопросы для самопроверки"))
                                    {
                                        rtb_Tems.Paste();
                                        rtb_Log.AppendText("Перечень УМО считаны\n", Color.Green);
                                    }
                                    else
                                    {
                                        ss = SearchText("5.4. Перечень учебно-методического обеспечения для самостоятельной работы обучающихся по дисциплине", "5.5. Перечень материально-технической базы", 1);
                                        if (ss.Contains("Тема 1.") & ss.Contains("Литература") & ss.Contains("Вопросы для самопроверки"))
                                        {
                                            rtb_Tems.Paste();
                                            rtb_Log.AppendText("Перечень УМО считаны\n", Color.Green);
                                        }
                                        else
                                        {
                                            rtb_Log.AppendText("Перечень УМО не считаны\n", Color.Red);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }


            Clipboard.Clear();


            //Поиск вопросов к экзамену/зачёту с учётом итогового контроля
            string exstr1 = "Вопросы к";
            string exstr2 = "VII.  МЕТОДИЧЕСКИЕ УКАЗАНИЯ";
            string exstr3 = "Итоговый контроль";
            string exstr4 = "Примеры тестов для контроля знаний:";
            string exgg1;

            ss = SearchText("Вопросы к", "Примеры тестов для контроля знаний:", 1);
            if (ss != "")
            {
                // Поиск 
                r.Find.Text = exstr1 + "*" + exstr4;
                r.Find.Forward = true;
                string exf1 = r.Find.Text;
                r.Find.Wrap = word.WdFindWrap.wdFindContinue; //при достижении конца документа поиск будет продолжаться с начала пока не будет достигнуто положение начала поиска
                r.Find.MatchWildcards = true;//подстановочные знаки ВКЛ

                if (r.Find.Execute(exf1))// Проверка поиска, если нашёл фрагменты, то...
                {
                    exgg1 = WordApp.ActiveDocument.Range(r.Start + exstr1.Length, r.End - exstr4.Length).Text; //убираем кл.
                    r.Start = r.Start + exstr1.Length;
                    r.End = r.End - exstr4.Length;
                    int exm21 = r.ListParagraphs.Count;
                    D.CountQuestForEx = exm21;
                    if (exm21 != 0)
                    {
                        object Start = r.ListParagraphs[1].Range.Start;
                        object End = r.ListParagraphs[exm21].Range.End;
                        word.Range myRange = WordApp.ActiveDocument.Range(Start, End);
                        myRange.Copy();
                        rtb_ForExam.Paste();
                        rtb_Log.AppendText("Вопросы к экзамену/зачёту считаны\n", Color.Green);
                        for (int y = 1; y <= r.ListParagraphs.Count; y++)
                        {
                            string dfs = r.ListParagraphs[y].Range.Text;
                            D.MyForExamAdd(dfs);
                        }
                    }
                    else
                    {
                        exm21 = r.Paragraphs.Count;
                        D.CountQuestForEx = exm21;
                        if (exm21 != 0)
                        {
                            object Start = r.Paragraphs[1].Range.Start;
                            object End = r.Paragraphs[exm21].Range.End;
                            word.Range myRange = WordApp.ActiveDocument.Range(Start, End);
                            myRange.Copy();
                            rtb_ForExam.Paste();
                            rtb_Log.AppendText("Вопросы к экзамену/зачёту считаны\n", Color.Green);
                            for (int y = 1; y <= r.Paragraphs.Count; y++)
                            {
                                string dfs = r.Paragraphs[y].Range.Text;
                                D.MyForExamAdd(dfs);
                            }
                        }


                    }

                }
            }
            ss = SearchText("Вопросы к", "Итоговый контроль", 1);

            if (ss != "")
            {
                // Поиск 
                r.Find.Text = exstr1 + "*" + exstr3;
                r.Find.Forward = true;
                string exf1 = r.Find.Text;
                r.Find.Wrap = word.WdFindWrap.wdFindContinue; //при достижении конца документа поиск будет продолжаться с начала пока не будет достигнуто положение начала поиска
                r.Find.MatchWildcards = true;//подстановочные знаки ВКЛ

                if (r.Find.Execute(exf1))// Проверка поиска, если нашёл фрагменты, то...
                {
                    exgg1 = WordApp.ActiveDocument.Range(r.Start + exstr1.Length, r.End - exstr3.Length).Text; //убираем кл.
                    r.Start = r.Start + exstr1.Length;
                    r.End = r.End - exstr3.Length;
                    int exm21 = r.ListParagraphs.Count;
                    D.CountQuestForEx = exm21;
                    if (exm21 != 0)
                    {
                        object Start = r.ListParagraphs[1].Range.Start;
                        object End = r.ListParagraphs[exm21].Range.End;
                        word.Range myRange = WordApp.ActiveDocument.Range(Start, End);
                        myRange.Copy();
                        rtb_ForExam.Paste();
                        rtb_Log.AppendText("Вопросы к экзамену/зачёту считаны\n", Color.Green);
                        for (int y = 1; y <= r.ListParagraphs.Count; y++)
                        {
                            string dfs = r.ListParagraphs[y].Range.Text;
                            D.MyForExamAdd(dfs);
                        }
                    }

                }
            }
            else
            {
                r.Find.Text = exstr1 + "*" + exstr2;
                r.Find.Forward = true;
                string exf1 = r.Find.Text;
                r.Find.Wrap = word.WdFindWrap.wdFindContinue; //при достижении конца документа поиск будет продолжаться с начала пока не будет достигнуто положение начала поиска
                r.Find.MatchWildcards = true;//подстановочные знаки ВКЛ
                ss = SearchText("Вопросы к", "VII.  МЕТОДИЧЕСКИЕ УКАЗАНИЯ", 1);
                if (ss == "")
                {
                    rtb_Log.AppendText("Вопросы для зачёта/экзамена не найдены\n", Color.Red);
                }
                if (r.Find.Execute(exf1))// Проверка поиска, если нашёл фрагменты, то...
                {
                    exgg1 = WordApp.ActiveDocument.Range(r.Start + exstr1.Length, r.End - exstr2.Length).Text; //убираем кл.
                    r.Start = r.Start + exstr1.Length;
                    r.End = r.End - exstr2.Length;
                    int exm21 = r.ListParagraphs.Count;
                    D.CountQuestForEx = exm21;
                    if (exm21 != 0)
                    {
                        object Start = r.ListParagraphs[1].Range.Start;
                        object End = r.ListParagraphs[exm21].Range.End;
                        word.Range myRange = WordApp.ActiveDocument.Range(Start, End);
                        myRange.Copy();
                        rtb_ForExam.Paste();
                        rtb_Log.AppendText("Вопросы к экзамену/зачёту считаны\n", Color.Green);
                        for (int y = 1; y <= r.ListParagraphs.Count; y++)
                        {
                            string dfs = r.ListParagraphs[y].Range.Text;
                            D.MyForExamAdd(dfs);
                        }
                    }
                    else
                    {
                        rtb_Log.AppendText("Некоррекнтый список вопросов к экзамену/зачёту\n", Color.Red);
                    }
                }
            }

            ss = SearchText("Итоговый контроль", "VII.  МЕТОДИЧЕСКИЕ УКАЗАНИЯ", 1);
            if (ss != "")
            {
                // Поиск 
                r.Find.Text = exstr3 + "*" + exstr2;
                r.Find.Forward = true;
                string exf1 = r.Find.Text;
                r.Find.Wrap = word.WdFindWrap.wdFindContinue; //при достижении конца документа поиск будет продолжаться с начала пока не будет достигнуто положение начала поиска
                r.Find.MatchWildcards = true;//подстановочные знаки ВКЛ

                if (r.Find.Execute(exf1))// Проверка поиска, если нашёл фрагменты, то...
                {
                    exgg1 = WordApp.ActiveDocument.Range(r.Start, r.End - exstr2.Length).Text; //убираем кл.
                    r.Start = r.Start;
                    r.End = r.End - exstr2.Length;
                    int exm21 = r.Paragraphs.Count;
                    if (exm21 != 0)
                    {
                        object Start = r.Paragraphs[1].Range.Start;
                        object End = r.Paragraphs[exm21].Range.End;
                        word.Range myRange = WordApp.ActiveDocument.Range(Start, End);
                        myRange.Copy();
                        rtb_ForExam.Paste();
                        rtb_Log.AppendText("Итоговый контроль найден\n", Color.Green);
                    }
                }
            }
            else if (SearchText("Примеры тестов для контроля знаний:", "VII.  МЕТОДИЧЕСКИЕ УКАЗАНИЯ", 1) != "")
            {
                rtb_ForExam.Paste();
            }
            else
            {
                rtb_Log.AppendText("Итоговый контроль не найден\n", Color.Red);
            }

            r.Application.ActiveDocument.Close(word.WdSaveOptions.wdDoNotSaveChanges);
            // CloseProcess();


        }
        private void ReplBookmark(string NameBookMark, ref RichTextBox rt, ref Microsoft.Office.Interop.Word.Application Word1) // Замена закладки форматированным текстом из richtextbox-a
        {
            System.Drawing.Font cfont;
            rt.SelectAll();
            cfont = rt.SelectionFont;
            rt.Copy();
            Word1.Selection.Find.ClearFormatting();
            Word1.Selection.Find.Text = NameBookMark;
            Word1.Selection.Find.Execute();
            Word1.Selection.Font.Name = "Times New Roman";
            if (Clipboard.GetText() != "")
            {
                Word1.Selection.Paste();
            }

            //' возвращаем курсор в начало документа
            Word1.Selection.Range.Start = 1;
            Word1.Selection.Range.End = 1;
        }
        private void FindReplace(string str_old, string str_new) // Замена фрагментов текста длинными кусками(больше 246 символ)
        {
            Microsoft.Office.Interop.Word.Range r;//Range
            r = WordApp.ActiveDocument.Range();
            r.Find.Text = str_old; // Находим слово которое нужно заменить
            if (str_new.Length > 246) // Проверка если длинна слова больше 246 символов 
            {
                string Str_long = str_new; // новая переменная для работы с кусками текста
                while (Str_long.Length > 0) // разьбиение строки на фрагменты и добавление в НРП
                {
                    if (Str_long.Length > 246)
                    {
                        r.Find.Replacement.Text = Str_long.Substring(0, 245) + "<Text>";
                        Str_long = Str_long.Substring(245, Str_long.Length - 245);
                        r.Find.Execute(r.Find.Text, Replace: word.WdReplace.wdReplaceAll);
                        r.Find.Text = "<Text>"; // хештег для поиска замены
                    }
                    else // если осталось меньше 246, добавляем последний кусок текста
                    {
                        r.Find.Replacement.Text = Str_long.Substring(0, Str_long.Length);
                        r.Find.Execute(r.Find.Text, Replace: word.WdReplace.wdReplaceAll);
                        break;
                    }
                }
            }
            else
            {
                r.Find.Replacement.Text = str_new;
                r.Find.Execute(r.Find.Text, Replace: word.WdReplace.wdReplaceAll);
            }

        }

        private void CreateNewProgram() // работа с Новой РП
        {
            WordApp = new word.Application(); // создаем объект word;
            FormMain FM = new FormMain();
            string NRP = FileNaim;

            var RPD = WordApp.Documents.Add(FileNaim);

            string Name_NRP = DA.Index + "_" + DA.Naim + "_" + DA.Profile + ".docx"; // Название файла РПД

            /* Сохранение РПД в папку на рабочем столе "РПД" */
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string subpath = @"РПД";
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
            dirInfo.CreateSubdirectory(subpath);
            path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\РПД/";
            object fileName = path + Name_NRP;
            RPD.SaveAs(fileName);
            /*   //////////////////////////////////////////// */


            WordApp.Visible = true;
            FindReplace("#Направление", DA.Napr);
            FindReplace("#Индекс", DA.Index);
            FindReplace("#Дисциплина", DA.Naim);
            FindReplace("#Профиль", DA.Profile);
            FindReplace("#ФГОС", DA.Standart);
            string PartDist = DA.Index.Substring(0, 2);
            if (PartDist == "Б1")
            {
                FindReplace("#Части", "базовой части");
            }
            else
            {
                FindReplace("#Части", "вариативной части");
            }
            FindReplace("#Цели", D.Cel);
            FindReplace("#Задачи", D.Tasks);
            if (DA.PreDis.Count != 0)
            {
                foreach (string s in DA.PreDis)
                {
                    FindReplace("#ДисциплиныДО", s);
                }
            }
            else
            {
                FindReplace("#ДисциплиныДО", " (данная дисциплина является начальной при формировании запланированых компетенций)");
            }

            if (D.Zn_before == null) { FindReplace("#ЗнатьДО", ""); }
            FindReplace("#ЗнатьДО", D.Zn_before);
            if (D.Um_before == null) { FindReplace("#УметьДО", ""); }
            FindReplace("#УметьДО", D.Um_before);
            if (D.Vl_before == null) { FindReplace("#ВладетьДО", ""); }
            FindReplace("#ВладетьДО", D.Vl_before);
            FindReplace("#зе", Convert.ToString(DA.Fact));
            FindReplace("#че", Convert.ToString(DA.AtPlan));
            FindReplace("#конт", Convert.ToString(DA.ContactHours));
            FindReplace("#аудит", Convert.ToString(DA.Aud));
            FindReplace("#срс", Convert.ToString(DA.SR));
            FindReplace("#кконтр", Convert.ToString(DA.Contr));
            FindReplace("#инт", Convert.ToString(DA.InterHours));
            FindReplace("#эл", Convert.ToString(DA.ElectHours));



            for (int i = 0; i <= DA.Lekc.Length - 1; i++)
            {
                Leck += DA.Lekc[i];
            }

            FindReplace("#лек", Convert.ToString(Leck));
            FindReplace("#+Лекции", Convert.ToString(Leck / 2));


            for (int i = 0; i <= DA.Lab.Length - 1; i++)
            {
                Lab += DA.Lab[i];
            }
            FindReplace("#лаб", Convert.ToString(Lab));

            for (int i = 0; i <= DA.Practice.Length - 1; i++)
            {
                PR += DA.Practice[i];
            }
            FindReplace("#пр", Convert.ToString(PR));
            FindReplace("#+Практические", Convert.ToString((Lab + PR) / 2));
            if (DA.AfterDis.Count != 0)
            {
                foreach (string s in DA.AfterDis)
                {
                    FindReplace("#ДисциплиныПосле", s);
                }
            }
            else
            {
                FindReplace("#ДисциплиныПосле", " ");
            }

            string Examen = "";
            string DifZachet = "";
            string Zachet = "";
            for (int i = 0; i <= DA.Examen.Length - 1; i++)
            {
                if (DA.Examen[i] == true)
                {
                    if (DA.KR - 1 == i)
                    { Examen = "в " + Convert.ToString(i + 1) + "семестре - " + "Экзамен, Курсовая работа; " + Environment.NewLine; }
                    else
                    { Examen = "в " + Convert.ToString(i + 1) + "семестре - " + "Экзамен; " + Environment.NewLine; }

                }
                if (DA.Dif_Zachet[i] == true)
                {
                    if (DA.KR - 1 == i)
                    { DifZachet = "в " + Convert.ToString(i + 1) + "семестре - " + "Зачет с оценкой, Курсовая работа; " + Environment.NewLine; }
                    else
                    { DifZachet = "в " + Convert.ToString(i + 1) + "семестре - " + "Зачет с оценкой; " + Environment.NewLine; }

                }
                if (DA.Zachet[i] == true)
                {
                    if (DA.KR - 1 == i)
                    { Zachet = "в " + Convert.ToString(i + 1) + "семестре - " + "Зачет, Курсовая работа; " + Environment.NewLine; }
                    else
                    { Zachet = "в " + Convert.ToString(i + 1) + "семестре - " + "Зачет; " + Environment.NewLine; }

                }
            }
            FindReplace("#Аттестация", Examen + DifZachet + Zachet);
            string Compet = "";
            for (int i = 0; i <= DA.Compet.Count - 1; i++)
            {
                Compet += DA.Compet[i] + ";";
            }
            Compet = Compet.Substring(0, Compet.Length - 1);

            ReplBookmark("#Основная литература", ref rtb_LiteraBasic, ref WordApp);
            ReplBookmark("#Дополнительная литература", ref rtb_Add_Litera, ref WordApp);
            ReplBookmark("#Перечень_УМО", ref rtb_Tems, ref WordApp);

            string FindTable = "Наименование темы дисциплины";

            for (int i = 1; i <= WordApp.ActiveDocument.Tables.Count; i++)
            {
                if (WordApp.ActiveDocument.Tables[i].Cell(1, 2).Range.Find.Execute(FindTable))
                {
                    for (int z = 2; z <= D.Nt + 1; z++) // z - номер строки в таблице с темами
                    {

                        WordApp.ActiveDocument.Tables[i].Cell(z, 2).Range.Text = D.tems[z - 2].Name;
                        WordApp.ActiveDocument.Tables[i].Cell(z, 3).Range.Text = D.tems[z - 2].Text;
                        WordApp.ActiveDocument.Tables[i].Cell(z, 5).Range.Text = D.tems[z - 2].Rez;
                        WordApp.ActiveDocument.Tables[i].Cell(z, 4).Range.Text = Compet;
                        WordApp.ActiveDocument.Tables[i].Cell(z, 6).Range.Text = D.tems[z].FormZ;
                        if (z != D.Nt + 1) WordApp.ActiveDocument.Tables[i].Rows.Add();
                    }
                }

            }
            FindReplace("#Вопрос1", D.ForExam[0]);
            FindReplace("#Вопрос2", D.ForExam[1]);
            ReplBookmark("#Задания", ref rtb_ForExam, ref WordApp);
            if (D.Curs_R == null)
            {
                FindReplace("#Курсовые", "Курсовые работы учебным планом не предусмотренны");
            }
            else
            {
                FindReplace("#Курсовые", D.Curs_R);
            }
            TemPlan();


            RPD.Save();





        }

        private void CreateNewFos() // работа с Новым Фос
        {
            WordApp = new word.Application(); // создаем объект word;
            FormMain FM = new FormMain();

            string FOS = FileNaim_FOS;

            var RPD = WordApp.Documents.Add(FileNaim_FOS);

            string Name_NRPF = DA.Index + "_" + DA.Naim + "_" + DA.Profile + ".docx"; // Название файла РПД

            /* Сохранение РПД в папку на рабочем столе "РПД" */
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string subpath = @"ФОС";
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
            dirInfo.CreateSubdirectory(subpath);
            path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\ФОС/";
            object fileName = path + Name_NRPF;
            RPD.SaveAs(fileName);
            /*   //////////////////////////////////////////// */


            WordApp.Visible = true;
            FindReplace("#Направление", DA.Napr);
            FindReplace("#Индекс", DA.Index);
            FindReplace("#Дисциплина", DA.Naim);
            string PartDist = DA.Index.Substring(0, 2);
            if (PartDist == "Б1")
            {
                FindReplace("#Части", "базовой части");
            }
            else
            {
                FindReplace("#Части", "вариативной части");
            }
            foreach (string s in DA.PreDis)
            {
                FindReplace("#ДисциплиныДО", s);
            }

            FindReplace("#ЗнатьДО", D.Zn_before);
            FindReplace("#УметьДО", D.Um_before);
            FindReplace("#ВладетьДО", D.Vl_before);
            foreach (string s in DA.AfterDis)
            {
                FindReplace("#ДисциплиныПосле", s);
            }
            string Compet = "";
            for (int i = 0; i <= DA.Compet.Count - 1; i++)
            {
                Compet += DA.Compet[i] + ";";
            }
            Compet = Compet.Substring(0, Compet.Length - 1);
            string FindTable = "Наименование темы дисциплины";

            for (int i = 1; i <= WordApp.ActiveDocument.Tables.Count; i++)
            {
                if (WordApp.ActiveDocument.Tables[i].Cell(1, 2).Range.Find.Execute(FindTable))
                {
                    for (int z = 2; z <= D.Nt + 1; z++) // z - номер строки в таблице с темами
                    {

                        WordApp.ActiveDocument.Tables[i].Cell(z, 2).Range.Text = D.tems[z - 2].Name;
                        WordApp.ActiveDocument.Tables[i].Cell(z, 3).Range.Text = D.tems[z - 2].Text;
                        WordApp.ActiveDocument.Tables[i].Cell(z, 5).Range.Text = D.tems[z - 2].Rez;
                        WordApp.ActiveDocument.Tables[i].Cell(z, 4).Range.Text = Compet;
                        WordApp.ActiveDocument.Tables[i].Cell(z, 6).Range.Text = D.tems[z].FormZ;
                        if (z != D.Nt + 1) WordApp.ActiveDocument.Tables[i].Rows.Add();
                    }
                }

            }
            ReplBookmark("Перечень_УМО", ref rtb_Tems, ref WordApp);


            RPD.Save();
        }

        private void CreateNewAnat()
        {
            WordApp = new word.Application(); // создаем объект word;
            FormMain FM = new FormMain();
            string ANAT = FileNaim_ANAT;
            var RPD_ANAT = WordApp.Documents.Add(ANAT);
            if (FileNaim_ANAT != null)
            {
                RPD_ANAT = WordApp.Documents.Add(ANAT);
            }
            string Name_Anat = "Аннотация_" + DA.Index + "_" + DA.Naim + "_" + DA.Profile + ".docx"; // Название файла РПД

            /* Сохранение РПД в папку на рабочем столе "РПД" */
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string subpath = @"РПД";
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
            dirInfo.CreateSubdirectory(subpath);
            path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\РПД/";
            object fileName = path + Name_Anat;
            RPD_ANAT.SaveAs(fileName);
            /*   //////////////////////////////////////////// */

            WordApp.Visible = true;
            FindReplace("#Направление", DA.Napr);
            FindReplace("#Индекс", DA.Index);
            FindReplace("#Дисциплина", DA.Naim);
            FindReplace("#Профиль", DA.Profile);

            string PartDist = DA.Index.Substring(0, 2);
            if (PartDist == "Б1")
            {
                FindReplace("#Части", "базовой части");
            }
            else
            {
                FindReplace("#Части", "вариативной части");
            }
            FindReplace("#Цели", D.Cel);
            FindReplace("#Задачи", D.Tasks);
            foreach (string s in DA.PreDis)
            {
                FindReplace("#ДисциплиныДО", s);
            }

            FindReplace("#ЗнатьДО", D.Zn_before);
            FindReplace("#УметьДО", D.Um_before);
            FindReplace("#ВладетьДО", D.Vl_before);
            foreach (string s in DA.AfterDis)
            {
                FindReplace("#ДисциплиныПосле", s);
            }
            string Examen = "";
            string DifZachet = "";
            string Zachet = "";
            for (int i = 0; i <= DA.Examen.Length - 1; i++)
            {
                if (DA.Examen[i] == true)
                {
                    if (DA.KR - 1 == i)
                    { Examen = "в " + Convert.ToString(i + 1) + "семестре - " + "Экзамен, Курсовая работа; " + Environment.NewLine; }
                    else
                    { Examen = "в " + Convert.ToString(i + 1) + "семестре - " + "Экзамен; " + Environment.NewLine; }

                }
                if (DA.Dif_Zachet[i] == true)
                {
                    if (DA.KR - 1 == i)
                    { DifZachet = "в " + Convert.ToString(i + 1) + "семестре - " + "Зачет с оценкой, Курсовая работа; " + Environment.NewLine; }
                    else
                    { DifZachet = "в " + Convert.ToString(i + 1) + "семестре - " + "Зачет с оценкой; " + Environment.NewLine; }

                }
                if (DA.Zachet[i] == true)
                {
                    if (DA.KR - 1 == i)
                    { Zachet = "в " + Convert.ToString(i + 1) + "семестре - " + "Зачет, Курсовая работа; " + Environment.NewLine; }
                    else
                    { Zachet = "в " + Convert.ToString(i + 1) + "семестре - " + "Зачет; " + Environment.NewLine; }

                }
            }
            FindReplace("#Аттестация", Examen + DifZachet + Zachet);
            string Compet = "";
            for (int i = 0; i <= DA.Compet.Count - 1; i++)
            {
                Compet += DA.Compet[i] + ";";
            }
            Compet = Compet.Substring(0, Compet.Length - 1);
            string FindTable = "Наименование темы дисциплины";

            for (int i = 1; i <= WordApp.ActiveDocument.Tables.Count; i++)
            {
                if (WordApp.ActiveDocument.Tables[i].Cell(1, 2).Range.Find.Execute(FindTable))
                {
                    for (int z = 2; z <= D.Nt + 1; z++) // z - номер строки в таблице с темами
                    {

                        WordApp.ActiveDocument.Tables[i].Cell(z, 2).Range.Text = D.tems[z - 2].Name;
                        WordApp.ActiveDocument.Tables[i].Cell(z, 3).Range.Text = D.tems[z - 2].Text;
                        WordApp.ActiveDocument.Tables[i].Cell(z, 4).Range.Text = Compet;
                        if (z != D.Nt + 1) WordApp.ActiveDocument.Tables[i].Rows.Add();
                    }
                }

            }


            RPD_ANAT.Save();
        }


        private void TemPlan() // Заполнение ТЕМАТИЧЕСКИЙ ПЛАН ИЗУЧЕНИЯ ДИСЦИПЛИНЫ
        {
            List<int> CountSem = new List<int>(); // создать коллекций семестров
            int BegCell = 2; // начальная строка для объединения

            int a = 0; // индекс FindTemPlan
            int b = 0; //индекс FindTemControl
            int c = 0; //индекс FindTemRating

            string FindTemPlan = "Наименование разделов и тем";
            string FindTemControl = "Наименование раздела/ темы, выносимых на контроль";
            string FindTemRating = "Наименование раздела/темы дисциплины";

            for (int i = 1; i <= WordApp.ActiveDocument.Tables.Count; i++)
            {
                if (WordApp.ActiveDocument.Tables[i].Cell(1, 2).Range.Find.Execute(FindTemPlan))
                {
                    a = i;
                }
                if (WordApp.ActiveDocument.Tables[i].Cell(1, 2).Range.Find.Execute(FindTemControl))
                {
                    b = i;
                }
                if (WordApp.ActiveDocument.Tables[i].Cell(1, 2).Range.Find.Execute(FindTemRating))
                {
                    c = i;
                }
            }

            for (int n = 0; n <= DA.Examen.Length - 1; n++) // цикл для определение ФОРМЫ КОНТРОЛЯ для текущего семестра
            {
                if (DA.Examen[n] == true)
                {
                    CountSem.Add(n + 1); // записываю в коллекцию 
                }
                if (DA.Dif_Zachet[n] == true)
                {
                    CountSem.Add(n + 1);
                }
                if (DA.Zachet[n] == true)
                {
                    CountSem.Add(n + 1);
                }

            }
            if (CountSem.Count > 1) // Отсюда идет заполнение таблицы
            {
                CountSem.Sort();
                int DivideDist = D.Nt / CountSem.Count; // деление дисциплин на равное количество
                int RestDist = D.Nt % CountSem.Count; // остаток дисциплин при нечетном вычислении 
                int a1 = 0; // для определение текущей строки в таблице
                int a2 = 0;
                int a3 = 0;
                int resresh = 0; // определение семестра в цикле заполнения 
                int TemSem; // кол-во тем в семестре
                for (int d = 0; d <= CountSem.Count - 1; d++)
                {
                    if (DA.Examen[CountSem[d] - 1] == true) // проверка из видов ФОРМ КОНТРОЛЯ используется в текуем семестре
                    { ListControl.Add("Экзамен"); }
                    else if (DA.Dif_Zachet[CountSem[d] - 1] == true)
                    { ListControl.Add("Зачет с оценкой"); }
                    else if (DA.Zachet[CountSem[d] - 1] == true)
                    { ListControl.Add("Зачет"); }




                    if (d == CountSem.Count - 1) // Если последний семестр, то добавляем остаток тем
                    {
                        TemInSem.Add(DivideDist + RestDist);
                        TemSem = DivideDist + RestDist;
                    }
                    else
                    {
                        TemInSem.Add(DivideDist);
                        TemSem = DivideDist;
                    }

                    int DivideLec = DA.Lekc[CountSem[d] - 1] / TemSem;
                    int RestLec = DA.Lekc[CountSem[d] - 1] % TemSem;
                    int DividePR = DA.Practice[CountSem[d] - 1] / TemSem;
                    int RestPR = DA.Practice[CountSem[d] - 1] % TemSem;
                    int DivideLB = DA.Lab[CountSem[d] - 1] / TemSem;
                    int RestLB = DA.Lab[CountSem[d] - 1] % TemSem;
                    int DivideAUD = (DA.Aud / CountSem.Count) / TemSem;
                    int RestAUD = (DA.Aud / CountSem.Count) % TemSem;
                    int DivideSR = DA._SR[CountSem[d] - 1] / TemSem;
                    int RestSR = DA._SR[CountSem[d] - 1] % TemSem;
                    int DivideEL = DA.Elect[CountSem[d] - 1] / TemSem;
                    int RestEL = DA.Elect[CountSem[d] - 1] % TemSem;
                    int DivideInter = DA.InterHousInSem[CountSem[d] - 1] / TemSem;
                    int RestInter = DA.InterHousInSem[CountSem[d] - 1] % TemSem;


                    for (int y = 0; y <= TemSem - 1; y++) // цикл заполнение тем по семестрам
                    {
                        resresh = d * DivideDist + y;
                        a1 = WordApp.ActiveDocument.Tables[a].Rows.Count;
                        a2 = WordApp.ActiveDocument.Tables[b].Rows.Count;
                        a3 = WordApp.ActiveDocument.Tables[c].Rows.Count;

                        WordApp.ActiveDocument.Tables[a].Cell(a1, 1).Range.Text = Convert.ToString(resresh + 1);
                        WordApp.ActiveDocument.Tables[b].Cell(a2, 1).Range.Text = Convert.ToString(resresh + 1);
                        WordApp.ActiveDocument.Tables[c].Cell(a3, 1).Range.Text = Convert.ToString(resresh + 1);
                        string Text = D.tems[resresh].Name.Replace("\r", "");
                        WordApp.ActiveDocument.Tables[a].Cell(a, 2).Range.Text = Text;
                        WordApp.ActiveDocument.Tables[b].Cell(a2, 2).Range.Text = Text;
                        WordApp.ActiveDocument.Tables[c].Cell(a3, 2).Range.Text = Text;
                        WordApp.ActiveDocument.Tables[b].Cell(a2, 4).Range.Text = Convert.ToDouble(20.0 / TemSem).ToString("0.00");

                        if (y == TemSem - 1)
                        {
                            WordApp.ActiveDocument.Tables[a].Cell(a1, 3).Range.Text = Convert.ToString(DivideLec + RestLec);
                            WordApp.ActiveDocument.Tables[a].Cell(a1, 4).Range.Text = Convert.ToString(DividePR + RestPR);
                            WordApp.ActiveDocument.Tables[a].Cell(a1, 5).Range.Text = Convert.ToString(DivideLB + RestLB);
                            WordApp.ActiveDocument.Tables[a].Cell(a1, 6).Range.Text = Convert.ToString(DivideAUD + RestAUD);
                            WordApp.ActiveDocument.Tables[a].Cell(a1, 7).Range.Text = "Д,МК,ОР,ОТЗ";
                            WordApp.ActiveDocument.Tables[a].Cell(a1, 8).Range.Text = Convert.ToString(DivideInter + RestInter);
                            WordApp.ActiveDocument.Tables[a].Cell(a1, 9).Range.Text = Convert.ToString(DivideEL + RestEL);
                            WordApp.ActiveDocument.Tables[a].Cell(a1, 10).Range.Text = "П,Р,ТЗ,Лит";
                            WordApp.ActiveDocument.Tables[a].Cell(a1, 11).Range.Text = Convert.ToString(DivideSR + RestSR);
                            WordApp.ActiveDocument.Tables[a].Cell(a1, 12).Range.Text = "Оп,КР,Т";
                            WordApp.ActiveDocument.Tables[b].Cell(a2, 3).Range.Text = "Оп,КР,Т";
                            WordApp.ActiveDocument.Tables[c].Cell(a3, 3).Range.Text = "Р,ТЗ,Д";
                        }

                        else
                        {
                            WordApp.ActiveDocument.Tables[a].Cell(a1, 3).Range.Text = Convert.ToString(DivideLec);
                            WordApp.ActiveDocument.Tables[a].Cell(a1, 4).Range.Text = Convert.ToString(DividePR);
                            WordApp.ActiveDocument.Tables[a].Cell(a1, 5).Range.Text = Convert.ToString(DivideLB);
                            WordApp.ActiveDocument.Tables[a].Cell(a1, 6).Range.Text = Convert.ToString(DivideAUD);
                            WordApp.ActiveDocument.Tables[a].Cell(a1, 7).Range.Text = "Д,МК,ОР,ОТЗ";
                            WordApp.ActiveDocument.Tables[a].Cell(a1, 8).Range.Text = Convert.ToString(DivideInter);
                            WordApp.ActiveDocument.Tables[a].Cell(a1, 9).Range.Text = Convert.ToString(DivideEL);
                            WordApp.ActiveDocument.Tables[a].Cell(a1, 10).Range.Text = "П,Р,ТЗ,Лит";
                            WordApp.ActiveDocument.Tables[a].Cell(a1, 11).Range.Text = Convert.ToString(DivideSR);
                            WordApp.ActiveDocument.Tables[a].Cell(a1, 12).Range.Text = "Оп,КР,Т";
                            WordApp.ActiveDocument.Tables[b].Cell(a2, 3).Range.Text = "Оп,КР,Т";
                            WordApp.ActiveDocument.Tables[c].Cell(a3, 3).Range.Text = "Р,ТЗ,Д";
                        }

                        WordApp.ActiveDocument.Tables[a].Rows.Add();
                        WordApp.ActiveDocument.Tables[b].Rows.Add();
                        WordApp.ActiveDocument.Tables[c].Rows.Add();
                    }
                    WordApp.ActiveDocument.Tables[a].Cell(a1 + 1, 12).Range.Text = ListControl[d];// Подводим итоги в каждом семестре
                    if (DA.HoursCont[CountSem[d] - 1] != 0) // если семестр не последний
                    { WordApp.ActiveDocument.Tables[a].Cell(a1 + 1, 11).Range.Text = Convert.ToString(DA.HoursCont[CountSem[d] - 1]); }

                    WordApp.ActiveDocument.Tables[a].Rows.Add();
                    if (CountSem.Count - 1 != d)
                    {
                        WordApp.ActiveDocument.Tables[b].Rows.Add();
                        WordApp.ActiveDocument.Tables[c].Rows.Add();
                    }
                    WordApp.ActiveDocument.Tables[a].Cell(a1 + 2, 2).Range.Text = "Итого:";
                    WordApp.ActiveDocument.Tables[b].Cell(a2 + 1, 2).Range.Text = "Итого:";
                    WordApp.ActiveDocument.Tables[c].Cell(a3 + 1, 2).Range.Text = "Итого:";
                    WordApp.ActiveDocument.Tables[b].Cell(a2 + 2, 4).Range.Text = "20";
                    WordApp.ActiveDocument.Tables[c].Cell(a3 + 2, 4).Range.Text = "20";
                    WordApp.ActiveDocument.Tables[b].Cell(a2 + 1, 4).Range.Text = "20";
                    WordApp.ActiveDocument.Tables[c].Cell(BegCell, 4).Range.Text = "20";
                    WordApp.ActiveDocument.Tables[c].Cell(BegCell, 4).Merge(WordApp.ActiveDocument.Tables[c].Cell(a3, 4));
                    BegCell = a3 + 2;
                    WordApp.ActiveDocument.Tables[a].Cell(a1 + 2, 3).Range.Text = Convert.ToString(DA.Lekc[CountSem[d] - 1]);
                    WordApp.ActiveDocument.Tables[a].Cell(a1 + 2, 4).Range.Text = Convert.ToString(DA.Practice[CountSem[d] - 1]);
                    WordApp.ActiveDocument.Tables[a].Cell(a1 + 2, 5).Range.Text = Convert.ToString(DA.Lab[CountSem[d] - 1]);
                    WordApp.ActiveDocument.Tables[a].Cell(a1 + 2, 6).Range.Text = Convert.ToString(DA.Aud / CountSem.Count);
                    WordApp.ActiveDocument.Tables[a].Cell(a1 + 2, 8).Range.Text = Convert.ToString(DA.InterHousInSem[CountSem[d] - 1]);
                    WordApp.ActiveDocument.Tables[a].Cell(a1 + 2, 9).Range.Text = Convert.ToString(DA.Elect[CountSem[d] - 1]);
                    WordApp.ActiveDocument.Tables[a].Cell(a1 + 2, 11).Range.Text = Convert.ToString(DA._SR[CountSem[d] - 1]);
                    WordApp.ActiveDocument.Tables[a].Rows.Add();


                }
                int EndRows = WordApp.ActiveDocument.Tables[a].Rows.Count; // Добавляем в конце таблице итоги по всей дисциплине
                WordApp.ActiveDocument.Tables[a].Cell(EndRows, 2).Range.Text = "Всего по дисциплине:";
                WordApp.ActiveDocument.Tables[a].Cell(EndRows, 3).Range.Text = Convert.ToString(Leck);
                WordApp.ActiveDocument.Tables[a].Cell(EndRows, 4).Range.Text = Convert.ToString(PR);
                WordApp.ActiveDocument.Tables[a].Cell(EndRows, 5).Range.Text = Convert.ToString(Lab);
                WordApp.ActiveDocument.Tables[a].Cell(EndRows, 6).Range.Text = Convert.ToString(DA.Aud);
                WordApp.ActiveDocument.Tables[a].Cell(EndRows, 8).Range.Text = Convert.ToString(DA.InterHours);
                WordApp.ActiveDocument.Tables[a].Cell(EndRows, 9).Range.Text = Convert.ToString(DA.ElectHours);
                WordApp.ActiveDocument.Tables[a].Cell(EndRows, 11).Range.Text = Convert.ToString(DA.SR);
            }
            else // если 1 семестр
            {
                for (int d = 0; d <= CountSem.Count - 1; d++)
                {
                    if (DA.Examen[CountSem[d] - 1] == true) // проверка из видов ФОРМ КОНТРОЛЯ используется в текуем семестре
                    { ListControl.Add("Экзамен"); }
                    else if (DA.Dif_Zachet[CountSem[d] - 1] == true)
                    { ListControl.Add("Зачет с оценкой"); }
                    else if (DA.Zachet[CountSem[d] - 1] == true)
                    { ListControl.Add("Зачет"); }

                    int DivideLec = DA.Lekc[CountSem[d] - 1] / D.Nt;
                    int RestLec = DA.Lekc[CountSem[d] - 1] % D.Nt;
                    int DividePR = DA.Practice[CountSem[d] - 1] / D.Nt;
                    int RestPR = DA.Practice[CountSem[d] - 1] % D.Nt;
                    int DivideLB = DA.Lab[CountSem[d] - 1] / D.Nt;
                    int RestLB = DA.Lab[CountSem[d] - 1] % D.Nt;
                    int DivideAUD = (DA.Aud / CountSem.Count) / D.Nt;
                    int RestAUD = (DA.Aud / CountSem.Count) % D.Nt;
                    int DivideSR = DA._SR[CountSem[d] - 1] / D.Nt;
                    int RestSR = DA._SR[CountSem[d] - 1] % D.Nt;
                    int DivideEL = DA.Elect[CountSem[d] - 1] / D.Nt;
                    int RestEL = DA.Elect[CountSem[d] - 1] % D.Nt;
                    int DivideInter = DA.InterHousInSem[CountSem[d] - 1] / D.Nt;
                    int RestInter = DA.InterHousInSem[CountSem[d] - 1] % D.Nt;
                    int Index = 0;
                    int a2 = 0;
                    int a3 = 0;
                    for (int z = 0; z <= D.Nt - 1; z++) // z - номер строки в таблице с темами
                    {
                        Index = WordApp.ActiveDocument.Tables[a].Rows.Count;
                        a2 = WordApp.ActiveDocument.Tables[b].Rows.Count;
                        a3 = WordApp.ActiveDocument.Tables[c].Rows.Count;
                        string Text = D.tems[z].Name.Replace("\r", "");
                        WordApp.ActiveDocument.Tables[a].Cell(Index, 2).Range.Text = Text;
                        WordApp.ActiveDocument.Tables[b].Cell(a2, 2).Range.Text = Text;
                        WordApp.ActiveDocument.Tables[c].Cell(a3, 2).Range.Text = Text;
                        WordApp.ActiveDocument.Tables[b].Cell(a2, 4).Range.Text = Convert.ToDouble(20.0 / D.Nt).ToString("0.00");
                        if (z == D.Nt - 1)
                        {
                            WordApp.ActiveDocument.Tables[a].Cell(Index, 3).Range.Text = Convert.ToString(DivideLec + RestLec);
                            WordApp.ActiveDocument.Tables[a].Cell(Index, 4).Range.Text = Convert.ToString(DividePR + RestPR);
                            WordApp.ActiveDocument.Tables[a].Cell(Index, 5).Range.Text = Convert.ToString(DivideLB + RestLB);
                            WordApp.ActiveDocument.Tables[a].Cell(Index, 6).Range.Text = Convert.ToString(DivideAUD + RestAUD);
                            WordApp.ActiveDocument.Tables[a].Cell(Index, 7).Range.Text = "Д,МК,ОР,ОТЗ";
                            WordApp.ActiveDocument.Tables[a].Cell(Index, 8).Range.Text = Convert.ToString(DivideInter + RestInter);
                            WordApp.ActiveDocument.Tables[a].Cell(Index, 9).Range.Text = Convert.ToString(DivideEL + RestEL);
                            WordApp.ActiveDocument.Tables[a].Cell(Index, 10).Range.Text = "П,Р,ТЗ,Лит";
                            WordApp.ActiveDocument.Tables[a].Cell(Index, 11).Range.Text = Convert.ToString(DivideSR + RestSR);
                            WordApp.ActiveDocument.Tables[a].Cell(Index, 12).Range.Text = "Оп,КР,Т";
                            WordApp.ActiveDocument.Tables[b].Cell(a2, 3).Range.Text = "Оп,КР,Т";
                            WordApp.ActiveDocument.Tables[c].Cell(a3, 3).Range.Text = "Р,ТЗ,Д";

                        }

                        else
                        {
                            WordApp.ActiveDocument.Tables[a].Cell(Index, 3).Range.Text = Convert.ToString(DivideLec);
                            WordApp.ActiveDocument.Tables[a].Cell(Index, 4).Range.Text = Convert.ToString(DividePR);
                            WordApp.ActiveDocument.Tables[a].Cell(Index, 5).Range.Text = Convert.ToString(DivideLB);
                            WordApp.ActiveDocument.Tables[a].Cell(Index, 6).Range.Text = Convert.ToString(DivideAUD);
                            WordApp.ActiveDocument.Tables[a].Cell(Index, 7).Range.Text = "Д,МК,ОР,ОТЗ";
                            WordApp.ActiveDocument.Tables[a].Cell(Index, 8).Range.Text = Convert.ToString(DivideInter);
                            WordApp.ActiveDocument.Tables[a].Cell(Index, 9).Range.Text = Convert.ToString(DivideEL);
                            WordApp.ActiveDocument.Tables[a].Cell(Index, 10).Range.Text = "П,Р,ТЗ,Лит";
                            WordApp.ActiveDocument.Tables[a].Cell(Index, 11).Range.Text = Convert.ToString(DivideSR);
                            WordApp.ActiveDocument.Tables[a].Cell(Index, 12).Range.Text = "Оп,КР,Т";
                            WordApp.ActiveDocument.Tables[b].Cell(a2, 3).Range.Text = "Оп,КР,Т";
                            WordApp.ActiveDocument.Tables[c].Cell(a3, 3).Range.Text = "Р,ТЗ,Д";
                        }
                        WordApp.ActiveDocument.Tables[a].Rows.Add();
                        WordApp.ActiveDocument.Tables[b].Rows.Add();
                        WordApp.ActiveDocument.Tables[c].Rows.Add();

                    }
                    int EndRows = WordApp.ActiveDocument.Tables[a].Rows.Count;
                    int EndRowsB = WordApp.ActiveDocument.Tables[b].Rows.Count;
                    int EndRowsC = WordApp.ActiveDocument.Tables[c].Rows.Count;
                    WordApp.ActiveDocument.Tables[a].Cell(EndRows, 12).Range.Text = ListControl[d];
                    if (DA.HoursCont[CountSem[d] - 1] != 0)
                    { WordApp.ActiveDocument.Tables[a].Cell(Index + 1, 11).Range.Text = Convert.ToString(DA.HoursCont[CountSem[d] - 1]); }
                    WordApp.ActiveDocument.Tables[a].Rows.Add();
                    EndRows = WordApp.ActiveDocument.Tables[a].Rows.Count; // Добавляем в конце таблице итоги по всей дисциплине
                    EndRowsB = WordApp.ActiveDocument.Tables[b].Rows.Count;
                    EndRowsC = WordApp.ActiveDocument.Tables[c].Rows.Count;
                    WordApp.ActiveDocument.Tables[a].Cell(EndRows, 2).Range.Text = "Всего по дисциплине:";
                    WordApp.ActiveDocument.Tables[b].Cell(EndRowsB, 2).Range.Text = "Всего по дисциплине:";
                    WordApp.ActiveDocument.Tables[b].Cell(EndRowsB, 4).Range.Text = "20";
                    WordApp.ActiveDocument.Tables[c].Cell(EndRowsC, 2).Range.Text = "Всего по дисциплине:";
                    WordApp.ActiveDocument.Tables[c].Cell(EndRowsC, 4).Range.Text = "20";
                    WordApp.ActiveDocument.Tables[c].Cell(BegCell, 4).Range.Text = "20";
                    WordApp.ActiveDocument.Tables[c].Cell(BegCell, 4).Merge(WordApp.ActiveDocument.Tables[c].Cell(a3, 4));
                    WordApp.ActiveDocument.Tables[a].Cell(EndRows, 3).Range.Text = Convert.ToString(Leck);
                    WordApp.ActiveDocument.Tables[a].Cell(EndRows, 4).Range.Text = Convert.ToString(PR);
                    WordApp.ActiveDocument.Tables[a].Cell(EndRows, 5).Range.Text = Convert.ToString(Lab);
                    WordApp.ActiveDocument.Tables[a].Cell(EndRows, 6).Range.Text = Convert.ToString(DA.Aud);
                    WordApp.ActiveDocument.Tables[a].Cell(EndRows, 8).Range.Text = Convert.ToString(DA.InterHours);
                    WordApp.ActiveDocument.Tables[a].Cell(EndRows, 9).Range.Text = Convert.ToString(DA.ElectHours);
                    WordApp.ActiveDocument.Tables[a].Cell(EndRows, 11).Range.Text = Convert.ToString(DA.SR);

                }

            }
            FindReplace("#Посещение", Convert.ToDouble(20.0 * CountSem.Count / ((Leck / 2) + (PR + Lab) / 2)).ToString("0.00"));
        }




        private void Clear_Old_RP() // Очищает Анализ старой рп
        {
            Clipboard.Clear();
            rtb_Log.Clear();
            rtb_LiteraBasic.Clear();
            rtb_Add_Litera.Clear();
            rtb_ForExam.Clear();
            rtb_Tems.Clear();

            btn_OpenWp.Enabled = true;


        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (sec == 2)
            {
                sec = 0;
                btn_OpenWp.Enabled = true;
                timer1.Stop();
            }
            else
                sec++;

        }

        private void btn_OpenWp_Click(object sender, EventArgs e)
        {
            AnalysisOldProgramm();
            WordApp.Quit();
            if (btn_OpenWp.Enabled == false)
            {
                btn_Clear.Enabled = true;

            }
            else
            {
                btn_Clear.Enabled = false;
            }
            if (btn_OpenWp.Enabled == false)
            {
                bt_create_newrp.Enabled = true;
            }
            else
            {
                bt_create_newrp.Enabled = false;
            }
            if (btn_OpenWp.Enabled == false)
            {
                Create_Ticket.Enabled = true;
            }
            else
            {
                Create_Ticket.Enabled = false;
            }

        }

        private void bt_create_newrp_Click(object sender, EventArgs e)
        {
            CreateNewProgram();


            bt_create_newrp.Enabled = false;

            //if (AnalysisPattern(true))
            //{
            //    /*Если шаблон вернёт значение true, то он корректен и мы можем приступить к замене слов(для замены создан специальный метод выше)*/
            //}
        }

        private void rtb_Tems_TextChanged(object sender, EventArgs e)
        {

        }

        private void rtb_ForExam_TextChanged(object sender, EventArgs e)
        {

        }
        private void Ticket_For_Exam()
        {
            int c = 0;
            WordApp = new word.Application();
            WordApp.Visible = false;
            var Doc = WordApp.Documents.Add(Application.StartupPath + "/Билет_образец_спец.rtf");
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            object fileName = path + "/Билет_образец_спец(Новый).rtf";
            Doc.SaveAs(ref fileName);

            Microsoft.Office.Interop.Word.Range r1;
            r1 = WordApp.ActiveDocument.Range();

            int CountTickets = D.CountQuestForEx;
            if (CountTickets % 2 != 0)
            {
                CountTickets = CountTickets + 1;
                CountTickets = D.CountQuestForEx / 2;
            }
            else
            {
                CountTickets = D.CountQuestForEx / 2;
            }



            int m1 = D.ForExam.Count;
            int[] NumberT = new int[CountTickets];

            int m21 = r1.Paragraphs.Count;
            object Start = r1.Paragraphs[1].Range.Start;
            object End = r1.Paragraphs[m21].Range.End;
            word.Range myRange = WordApp.ActiveDocument.Range(Start, End);
            myRange.Copy();

            if (r1.Find.Execute(r1.Find.Text = "БИЛЕТ № 1"))
            {
                FindReplace("БИЛЕТ № 1", "БИЛЕТ № n");
            }
            for (int i = 1; i <= CountTickets; i++)
            {
                //NumberT[i] = new int [2];
                FindReplace("БИЛЕТ № n", "БИЛЕТ № " + i);
                FindReplace("#Дисциплина", DA.Naim);
                FindReplace("#Направление", DA.Napr);
                if (c <= m1)
                {
                    FindReplace("#Вопрос1", D.ForExam[c]);
                    FindReplace("#Вопрос2", D.ForExam[c + 1]);

                }
                c = c + 2;
                myRange.Start = myRange.End;
                myRange.InsertBreak(Microsoft.Office.Interop.Word.WdBreakType.wdPageBreak);
                myRange.Paste();
            }

            MessageBox.Show("Билеты на рабочем столе");
            Doc.SaveAs(ref fileName);
            CloseProcess();




        }

        public void fillingMainData() // загрузка информации из БД
        {
            // Обьявление массивов
            DA.CreateList();
            DA.initStruct();


            BD.Connect();
            BD.command.CommandText = "SELECT Дисциплины_профиля.Дисциплины, Дисциплины_профиля.Индекс,Дисциплины_профиля.Код_направления_подготовки, Дисциплины_профиля.Факт_по_зет, Дисциплины_профиля.По_плану, Дисциплины_профиля.Контакт_часы, Дисциплины_профиля.Аудиторные, Дисциплины_профиля.Самостоятельная_работа, Дисциплины_профиля.Контроль, Дисциплины_профиля.Элект_часы, Дисциплины_профиля.Интер_часы, Дисциплины_профиля.Закрепленная_кафедра, Дисциплины_профиля.Код_профиля, Дисциплины_профиля.Код FROM Дисциплины_профиля WHERE (((Дисциплины_профиля.Код)=" + ID + "));";
            BD.reader = BD.command.ExecuteReader();
            while (BD.reader.Read())
            {
                DA.Id_disp = Convert.ToInt32(BD.reader["Код"]);
                DA.Naim = BD.reader["Дисциплины"].ToString();
                DA.Index = BD.reader["Индекс"].ToString();
                DA.Fact = Convert.ToInt32(BD.reader["Факт_по_зет"]);
                DA.AtPlan = Convert.ToInt32(BD.reader["По_плану"]);
                DA.ContactHours = Convert.ToInt32(BD.reader["Контакт_часы"]);
                DA.Aud = Convert.ToInt32(BD.reader["Аудиторные"]);
                DA.SR = Convert.ToInt32(BD.reader["Самостоятельная_работа"]);
                DA.Contr = Convert.ToInt32(BD.reader["Контроль"]);
                DA.ElectHours = Convert.ToInt32(BD.reader["Элект_часы"]);
                DA.InterHours = Convert.ToInt32(BD.reader["Интер_часы"]);
                DA.Kafedra = BD.reader["Закрепленная_кафедра"].ToString();
                DA.ID = Convert.ToInt32(BD.reader["Код_профиля"]);
                ID_Napr = Convert.ToInt32(BD.reader["Код_направления_подготовки"]);
            }
            BD.reader.Close();
            // Запись направление подготовки и стандарт
            BD.command.CommandText = "SELECT Направление_подготовки.Код, Направление_подготовки.Направление_подготовки, Направление_подготовки.Станд FROM Направление_подготовки WHERE (((Направление_подготовки.Код)=" + ID_Napr + "));";
            BD.reader = BD.command.ExecuteReader();
            while (BD.reader.Read())
            {
                DA.Napr = BD.reader["Направление_подготовки"].ToString();
                DA.Standart = BD.reader["Станд"].ToString();
            }
            BD.reader.Close();
            // Запись профиль и год
            BD.command.CommandText = "SELECT Профиль.Название_профиля, Профиль.Год_профиля FROM Профиль WHERE (((Профиль.Код)=" + ID_Prof + "));";
            BD.reader = BD.command.ExecuteReader();
            while (BD.reader.Read())
            {
                DA.Profile = BD.reader["Название_профиля"].ToString();
                DA.Year = BD.reader["Год_профиля"].ToString();
            }
            BD.reader.Close();
            // Запись "Виды деятельности"
            BD.command.CommandText = "SELECT Виды_дейтельности.Список_дейтельности FROM Виды_дейтельности WHERE (((Виды_дейтельности.Код_направления_подготовки)=" + ID_Napr + "));";
            BD.reader = BD.command.ExecuteReader();
            while (BD.reader.Read())
            {
                DA.MyList(BD.reader["Список_дейтельности"].ToString());
            }
            BD.reader.Close();
            // Запись часов по СЕМЕСТРАМ
            BD.command.CommandText = "SELECT Семестр.Номер_семестра, Семестр.ZET, Семестр.Итого, Семестр.Лек, Семестр.Лек_инт, Семестр.Лаб, Семестр.Лаб_инт, Семестр.ПР, Семестр.ПР_инт, Семестр.Элек, Семестр.СР, Семестр.Часы_конт, Семестр.Часы_конт_электр, Семестр.Экзамен, Семестр.Зачет, Семестр.Зачет_с_оценкой, Семестр.Курсовая FROM Семестр WHERE (((Семестр.Код_дисциплины)=" + DA.Id_disp + "));";
            BD.reader = BD.command.ExecuteReader();
            while (BD.reader.Read())
            {
                DA.LS = Convert.ToInt32(BD.reader["Номер_семестра"]);
                DA._ZET(DA.LS, Convert.ToInt32(BD.reader["ZET"]));
                DA._Itogo(DA.LS, Convert.ToInt32(BD.reader["Итого"]));
                DA._Lekc(DA.LS, Convert.ToInt32(BD.reader["Лек"]));
                DA._LekcInter(DA.LS, Convert.ToInt32(BD.reader["Лек_инт"]));
                DA._Lab(DA.LS, Convert.ToInt32(BD.reader["Лаб"]));
                DA._LabInter(DA.LS, Convert.ToInt32(BD.reader["Лаб_инт"]));
                DA._Practice(DA.LS, Convert.ToInt32(BD.reader["ПР"]));
                DA._PractInter(DA.LS, Convert.ToInt32(BD.reader["ПР_инт"]));
                DA._Elect(DA.LS, Convert.ToInt32(BD.reader["Элек"]));
                DA._SR1(DA.LS, Convert.ToInt32(BD.reader["СР"]));
                DA._HoursCont(DA.LS, Convert.ToInt32(BD.reader["Часы_конт"]));
                DA._HoursContElect(DA.LS, Convert.ToInt32(BD.reader["Часы_конт_электр"]));

                if (Convert.ToBoolean(BD.reader["Экзамен"]) == true)
                {
                    DA._Examen(DA.LS);
                }
                if (Convert.ToBoolean(BD.reader["Зачет"]) == true)
                {
                    DA._Zachet(DA.LS);
                }
                if (Convert.ToBoolean(BD.reader["Зачет_с_оценкой"]) == true)
                {
                    DA._Dif_Zachet(DA.LS);
                }

                DA.KR = Convert.ToInt32(BD.reader["Курсовая"]);

            }
            BD.reader.Close();
            // Запись компетенций дисцп
            BD.command.CommandText = "SELECT Компетенции_дисциплины.Код_дисциплины, Компетенции.Компетенция, Компетенции.Содержание FROM Компетенции INNER JOIN Компетенции_дисциплины ON Компетенции.Код = Компетенции_дисциплины.Код_компетенции WHERE (((Компетенции_дисциплины.Код_дисциплины)=" + DA.Id_disp + "));";
            BD.reader = BD.command.ExecuteReader();
            while (BD.reader.Read())
            {
                DA.AddCompet(BD.reader["Компетенция"].ToString());
                DA._InfoCompet(BD.reader["Содержание"].ToString());
            }
            BD.reader.Close();
            // Запись Дисцп ДО
            BD.command.CommandText = "SELECT Дисциплина_до.Дисциплина_до FROM Дисциплина_до WHERE (((Дисциплина_до.Код_дисциплины)=" + DA.Id_disp + "));";
            BD.reader = BD.command.ExecuteReader();
            while (BD.reader.Read())
            {
                DA.AddPreDis(BD.reader["Дисциплина_до"].ToString());
            }
            BD.reader.Close();
            // Запись Дисцп ПОСЛЕ
            BD.command.CommandText = "SELECT Дисциплина_после.Дисциплина_после FROM Дисциплина_после WHERE (((Дисциплина_после.Код_дисциплины)=" + DA.Id_disp + "));";
            BD.reader = BD.command.ExecuteReader();
            while (BD.reader.Read())
            {
                DA.AddAfterDis(BD.reader["Дисциплина_после"].ToString());
            }
            BD.reader.Close();
            DA._InterHousInSem();

        }

        private void btn_Clear_Click(object sender, EventArgs e)
        {
            Clear_Old_RP();
            btn_Clear.Enabled = false;
            if (btn_Clear.Enabled == false)
            {
                Create_Ticket.Enabled = false;
                bt_create_newrp.Enabled = false;
            }
        }

        private void Create_Ticket_Click(object sender, EventArgs e)
        {
            Ticket_For_Exam();
            WordApp.Quit();
            Create_Ticket.Enabled = false;

        }

        private void bt_create_newfos_Click(object sender, EventArgs e)
        {
            CreateNewFos();
        }

        private void Create_ANOT_Click(object sender, EventArgs e)
        {
            CreateNewAnat();
        }

    }

}

