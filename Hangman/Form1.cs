using System.Diagnostics;
using System.Text;
using HugeHard.Json;
using HugeHard;
using System.Media;

namespace Hangman
{
    public partial class Form1 : Form
    {
        private const int DEFAULT_WEIGHT = 10;
        readonly Random random = new();
        private char[] WordChars = null!;
        private string Word = null!;
        private readonly HashSet<char> GuessedChars = new();
        private bool[] flag = null!;
        //private string[] Vocabulary = null!;
        //private int[] WordWeight = null!;
        private readonly Dictionary<string, int> WordWeight = new();
        readonly StringBuilder labelText = new(25);
        private int life = 7;
        int winTimes;
        int defeatTimes;
        readonly SoundPlayer soundWin;
        readonly SoundPlayer soundDefeat;
        readonly SoundPlayer soundAgain;
        private enum States { Playing, End, EditHint, SetWeight }
        private States state;
        private bool TextBoxEnabled => state == States.SetWeight || state == States.EditHint;

        public Form1()
        {
            InitializeComponent();
            soundWin = new("./resource/2.wav");
            soundDefeat = new("./resource/3.wav");
            soundAgain = new("./resource/1.wav");
            JsonConfigHelper = new JsonHelper<JsonUserConfig, Form1>(this);
            if (!JsonConfigHelper.Load())
                MessageBox.Show("��ȡ�����ļ�����");
            ChangeVocabulary();
            //textBox1.ReadOnly = true;
            //TextBox1.BorderStyle = 0;
            //TextBox1.BackColor = this.BackColor;
            //TextBox1.TabStop = false;
            //TextBox1.Multiline = True; // If needed
            ReadVocabulary(Config.�ʿ�);
            StartAGame();
        }

        private void ChangeVocabulary(bool isNew = false)
        {
            string fileName = Config.�ʿ�;
            if (isNew)
                fileName = "";
            while (!File.Exists(fileName))
            {
                MessageBox.Show("ѡ��һ���ʿ⣬��ʽΪÿ��һ������");
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                    fileName = openFileDialog1.FileName;
                else
                    fileName = Config.�ʿ�;
            }
            Config.�ʿ� = fileName;
        }

        private void SetLifePic()
        {
            pictureBox1.ImageLocation = "./resource/" + life + ".png";
        }

        private void StartAGame()
        {
            state = States.Playing;
            life = 7;
            SetLifePic();
            GuessedChars.Clear();
            PickAWordByWeight();
            //PickAWord();
            flag = new bool[WordChars.Length];
            for (int i = 0; i < WordChars.Length; i++)
                if (!WordChars[i].IsBasicLetter())
                    flag[i] = true;
            GenerateGuessedText();
            GenerateWordText();
            label3.Text = $"Right Times: {winTimes}\nWrong Times: {defeatTimes}";
            richTextBox2.Text = "Hint: ";
            if (Config.������ʾ[Word] == "null")
                richTextBox2.AppendText(Config.������ʾ[Word]);
            else
                richTextBox2.AppendText(Config.������ʾ[Word], Color.Red);
            panel1.Focus();
        }

        private void GenerateWordText()
        {
            labelText.Clear();
            for (int i = 0; i < WordChars.Length; i++)
            {
                if (flag[i])
                {
                    labelText.Append(WordChars[i]);
                    labelText.Append(' ');
                }
                else
                {
                    labelText.Append("_ ");
                }
            }
            label1.Text = labelText.ToString();
        }

        private void GenerateGuessedText()
        {
            richTextBox1.Clear();
            for (char c = 'A'; c <= 'Z'; c++)
            {
                if (GuessedChars.Contains(c))
                    richTextBox1.AppendText(c + " ", Color.Red);
                else
                    richTextBox1.AppendText(c + " ");
            }
        }

        private void SetWeight(string word, int weight)
        {
            Config.����Ȩ��[Word] = weight;
            WordWeight[word] = weight;
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            var c = char.ToUpper(e.KeyChar);
            //Console.WriteLine(c);
            if (TextBoxEnabled)
                return;
            if (state == States.End)
            {
                if (c == (char)Keys.Escape)
                {
                    SetWeight(Word, Math.Max(Config.����Ȩ��[Word], 1000));
                    //Config.����Ȩ��[Word] = MathEx.LimitMin(Config.����Ȩ��[Word], 1000);
                    Process.Start(new ProcessStartInfo($"gdlookup://localhost/{Word}") { UseShellExecute = true });
                }
                else if (c == ' ')
                    Process.Start(new ProcessStartInfo($"gdlookup://localhost/{Word}") { UseShellExecute = true });
                else if (c == 'R')
                    StartAGame();
                else if (c == 'H')
                {
                    state = States.EditHint;
                    richTextBox2.Focus();
                }
                else if (c == 'W')
                {
                    state = States.SetWeight;
                    richTextBox2.Focus();
                }
                else if (c == 'V')
                {
                    ChangeVocabulary(true);
                }
            }
            else if (c.IsBasicLetter())
                CheckChar(c);
        }

        //private void PickAWord()
        //{
        //    int i = random.Next(Vocabulary.Length);
        //    Word = Vocabulary[i];
        //    WordChars = Vocabulary[i].ToCharArray();
        //}

        private void PickAWordByWeight()
        {
            //int i = random.NextFromSumWeight(WordWeight);
            Word = random.NextFromDict(WordWeight);
            WordChars = Word.ToCharArray();
        }

        private void CheckChar(char c)
        {
            if (GuessedChars.Contains(c))
            {
                soundAgain.Play();
                return;
            }
            GuessedChars.Add(c);
            bool match = false;
            for (int i = 0; i < WordChars.Length; i++)
            {
                if (char.ToUpper(WordChars[i]) == c)
                {
                    flag[i] = true;
                    match = true;
                }
            }
            if (match)
                GenerateWordText();
            else
            {
                life--;
                SetLifePic();
            }
            GenerateGuessedText();
            if (flag.All(x => x))
            {
                winTimes++;
                SetWeight(Word, Config.����Ȩ��[Word] / 2);
                //Config.����Ȩ��[Word] = Config.����Ȩ��[Word] / 2;
                soundWin.Play();
                state = States.End;
                Config.�ɹ�����++;
                richTextBox2.Text = "You win!\r\n";
                ShowTextBoxTip();
            }
            else if (life == 0)
            {
                defeatTimes++;
                SetWeight(Word, Config.����Ȩ��[Word] + 500);
                //Config.����Ȩ��[Word] += 500;
                soundDefeat.Play();
                state = States.End;
                Config.ʧ�ܴ���++;
                richTextBox2.Text = $"You lose, the answer is: {Word}!\r\n";
                ShowTextBoxTip();
            }
        }

        private void ShowTextBoxTip()
        {
            richTextBox2.Text += $"Weight: {Config.����Ȩ��[Word]} Hint: {Config.������ʾ[Word]}";
            richTextBox2.Text += "\r\nEsc: Lookup   R: New game   H: edit the hint";
            richTextBox2.Text += "\r\nW: set the weight   V: Change vocabulary of Hangman";
        }

        private void ReadVocabulary(string path)
        {
            foreach (var word in File.ReadAllLines(path).Distinct())
                WordWeight.Add(word, Config.����Ȩ��.TryGetValue(word));
            //WordWeight = new int[Vocabulary.Length];
            //WordWeight[0] = Config.����Ȩ��[Vocabulary[0]];
            //for (int i = 1; i < Vocabulary.Length; i++)
            //    WordWeight[i] = WordWeight[i - 1] + Config.����Ȩ��[Vocabulary[i]];
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Config.����Ȩ��.RemoveRedundant();
            Config.������ʾ.RemoveRedundant();
            //foreach (var item in Config.����Ȩ��.Keys)
            //{
            //    if (Config.����Ȩ��[item] == DEFAULT_WEIGHT)
            //        Config.����Ȩ��.Remove(item);
            //}
            JsonConfigHelper.Save();
        }

        private void RichTextBox1_Enter(object sender, EventArgs e)
        {
            panel1.Focus();
        }

        private void TextBox1_Enter(object sender, EventArgs e)
        {
            if (state == States.SetWeight)
                richTextBox2.Text = Config.����Ȩ��[Word].ToString();
            else if (state == States.EditHint)
                richTextBox2.Text = Config.������ʾ[Word];
            else
                panel1.Focus();
            richTextBox2.SelectAll();
        }

        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (state == States.SetWeight)
                {
                    if (int.TryParse(richTextBox2.Text, out int t))
                        SetWeight(Word, t);
                    else
                        return;
                }
                else if (state == States.EditHint)
                {
                    Config.������ʾ[Word] = richTextBox2.Text.Trim();
                }
                state = States.End;
                richTextBox2.Clear();
                ShowTextBoxTip();
                e.SuppressKeyPress = true;
                panel1.Focus();
            }
        }
    }

    public static class Extensions
    {
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }

        public static bool IsBasicLetter(this char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }
    }

}