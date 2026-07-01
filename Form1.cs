using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileRenamer
{
    public class Form1 : Form
    {
        private int fileno;
        private ArrayList subArray = new ArrayList();
        private ArrayList shoboArray = new ArrayList();
        private ListViewItemComparer listViewItemSorter;
        private IContainer components;

        private ListView ListView1;
        private Button renamebtn;
        private Button clearbtn;
        private Label sortkind;
        private TextBox titletb;
        private Label label1;
        private TextBox daitb;
        private TextBox watb;
        private NumericUpDown stud;
        private CheckBox symbolcb;
        private CheckBox stoucb;
        private Button subtibtn;
        private TextBox subtitb;
        private CheckBox subticb;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem ヘルプToolStripMenuItem;
        private ToolStripMenuItem アップデート確認ToolStripMenuItem;
        private ToolStripMenuItem ブラウザToolStripMenuItem;
        private Label Copyright;
        private Label Verlabel;
        private Button checkbtn;
        private TextBox tidtb;
        private Label label2;
        private Button getbtn;
        private Label label3;
        private RadioButton fileselectrbtn;
        private RadioButton shoborbtn;
        private Label label4;
        private Label shobosubkazu;
        private CheckBox shobotitlecb;
        private Button deletebtn;
        private Button undobtn;
        private CheckBox autotidcb;

        private WebBrowser webBrowser1;
        private Button browserBackBtn;
        private Button browserForwardBtn;
        private Button browserResetBtn;
        private Button browserTidBtn;
        private Panel browserPanel;
        private bool browserVisible;

        private TextBox keywordtb;
        private Button searchbtn;
        private ListBox searchResultlb;

        private static readonly string[] VideoExts = { ".mp4", ".m2ts", ".ts", ".mkv", ".avi", ".wmv", ".mov", ".flv", ".webm", ".mpg", ".mpeg", ".divx", ".m4v", ".3gp" };

        public Form1()
        {
            InitializeComponent();
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            string text = "-";
            object[] customAttributes = entryAssembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), inherit: false);
            if (customAttributes != null && customAttributes.Length != 0)
            {
                text = ((AssemblyCopyrightAttribute)customAttributes[0]).Copyright;
            }
            string productVersion = Application.ProductVersion;
            Copyright.Text = text;
            Verlabel.Text = "Version " + productVersion;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            subtitb.ReadOnly = true;
            ListView1.FullRowSelect = true;
            ListView1.MultiSelect = true;
            ListView1.View = View.Details;
            ListView1.ColumnClick += ListView1_ColumnClick;
            ListView1.Columns.Add("No.", 30, HorizontalAlignment.Right);
            ListView1.Columns.Add("ファイル名", 200, HorizontalAlignment.Left);
            ListView1.Columns.Add("変更後ファイル名", 150, HorizontalAlignment.Left);
            ListView1.Columns.Add("更新日時", 150, HorizontalAlignment.Left);
            ListView1.Columns.Add("場所", 500, HorizontalAlignment.Left);
            listViewItemSorter = new ListViewItemComparer();
            listViewItemSorter.ColumnModes = new ListViewItemComparer.ComparerMode[5]
            {
                ListViewItemComparer.ComparerMode.Integer,
                ListViewItemComparer.ComparerMode.String,
                ListViewItemComparer.ComparerMode.String,
                ListViewItemComparer.ComparerMode.DateTime,
                ListViewItemComparer.ComparerMode.String
            };
            ListView1.ListViewItemSorter = listViewItemSorter;
            ListView1.KeyDown += ListView1_KeyDown;

            subticb.Checked = true;
            shobotitlecb.Checked = true;
        }

        private void ListView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            listViewItemSorter.Column = e.Column;
            if (e.Column == 0)
                sortkind.Text = "ソート：読み込み順";
            else if (e.Column == 1)
                sortkind.Text = "ソート：ファイル名順";
            else if (e.Column == 3)
                sortkind.Text = "ソート：更新日順";
            else if (e.Column == 4)
                sortkind.Text = "ソート:場所順";
            ListView1.Sort();
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop, autoConvert: false);
            bool hasFolder = false;
            var files = new System.Collections.Generic.List<string>();
            foreach (string path in paths)
            {
                if (File.Exists(path))
                {
                    string ext = Path.GetExtension(path).ToLowerInvariant();
                    if (VideoExts.Contains(ext))
                        files.Add(path);
                }
                else if (Directory.Exists(path))
                {
                    hasFolder = true;
                    CollectVideoFiles(path, files);
                }
            }
            if (hasFolder)
            {
                ListView1.Items.Clear();
                fileno = 0;
                string dirName = Path.GetFileName(paths[0]);
                var m = System.Text.RegularExpressions.Regex.Match(dirName, @"^(?:\d+_)?(?:\[[^\]]*\]\s*)?(.+?)(?:\s*\[[^\]]+\])?[\s\u3000]*$");
                if (m.Success)
                {
                    keywordtb.Text = m.Groups[1].Value.Trim();
                    searchbtn.PerformClick();
                }
            }
            files.Sort(StringComparer.CurrentCultureIgnoreCase);
            foreach (string file in files)
            {
                fileno++;
                string[] items = new string[5]
                {
                    fileno.ToString(),
                    Path.GetFileName(file),
                    "",
                    File.GetLastWriteTime(file).ToString(),
                    file
                };
                ListView1.Items.Add(new ListViewItem(items));
            }
        }

        private void CollectVideoFiles(string dir, System.Collections.Generic.List<string> files)
        {
            try
            {
                foreach (string f in Directory.GetFiles(dir))
                {
                    string ext = Path.GetExtension(f).ToLowerInvariant();
                    if (VideoExts.Contains(ext))
                        files.Add(f);
                }
                foreach (string d in Directory.GetDirectories(dir))
                    CollectVideoFiles(d, files);
            }
            catch { }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private Dictionary<string, string> undoMap = new Dictionary<string, string>();

        private void button1_Click(object sender, EventArgs e)
        {
            string title = titletb.Text;
            string dai = daitb.Text;
            string wa = watb.Text;
            int num = (int)stud.Value;
            int count = ListView1.Items.Count;
            bool symbol = symbolcb.Checked;
            bool stou = stoucb.Checked;
            bool usesub = subticb.Checked;
            bool useshobo = shoborbtn.Checked;
            bool usefile = fileselectrbtn.Checked;
            Rename rename = new Rename();
            int num2 = 0;
            string text = null;
            undoMap.Clear();
            for (int i = 0; i < count; i++)
            {
                string oldPath = ListView1.Items[i].SubItems[4].Text;
                text = rename.makeriname(title, dai, wa, stou, symbol, num - 1, usesub, useshobo, usefile, shoboArray, subArray, i + 1, count);
                string newPath = rename.renamefile(oldPath, text);
                if (newPath != null)
                    undoMap[newPath] = oldPath;
                else
                    num2++;
            }
            fintask(num2);
        }

        private void fintask(int error)
        {
            if (error == 0)
            {
                MessageBox.Show("ファイルリネームが完了しました。", "完了");
                ListView1.Items.Clear();
                fileno = 0;
            }
            else
            {
                undoMap.Clear();
                MessageBox.Show("エラーが" + error + "個有りました", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void clear_Click(object sender, EventArgs e)
        {
            ListView1.Items.Clear();
            fileno = 0;
            undoMap.Clear();
        }

        private void deletebtn_Click(object sender, EventArgs e)
        {
            var selected = ListView1.SelectedItems;
            if (selected.Count == 0) return;
            for (int i = selected.Count - 1; i >= 0; i--)
                ListView1.Items.Remove(selected[i]);
            for (int i = 0; i < ListView1.Items.Count; i++)
                ListView1.Items[i].SubItems[0].Text = (i + 1).ToString();
            fileno = ListView1.Items.Count;
        }

        private void ListView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                e.SuppressKeyPress = true;
                deletebtn_Click(null, null);
            }
            else if (e.KeyCode == Keys.F2)
            {
                e.SuppressKeyPress = true;
                if (ListView1.SelectedItems.Count != 1) return;
                var item = ListView1.SelectedItems[0];
                string oldFullPath = item.SubItems[4].Text;
                string oldName = Path.GetFileName(oldFullPath);
                string dir = Path.GetDirectoryName(oldFullPath);
                string input = Microsoft.VisualBasic.Interaction.InputBox("新しいファイル名を入力してください", "ファイル名の変更", oldName, -1, -1);
                if (string.IsNullOrEmpty(input) || input == oldName) return;
                string newPath = Path.Combine(dir, input);
                try
                {
                    File.Move(oldFullPath, newPath);
                    item.SubItems[1].Text = input;
                    item.SubItems[4].Text = newPath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("リネームできませんでした: " + ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
            else if (e.Control && e.KeyCode == Keys.A)
            {
                e.SuppressKeyPress = true;
                foreach (ListViewItem item in ListView1.Items)
                    item.Selected = true;
            }
        }

        private void undobtn_Click(object sender, EventArgs e)
        {
            if (undoMap.Count == 0)
            {
                MessageBox.Show("元に戻せるリネームがありません", "情報");
                return;
            }
            int errors = 0;
            foreach (var kv in undoMap)
            {
                try
                {
                    if (File.Exists(kv.Key))
                        File.Move(kv.Key, kv.Value);
                    else
                        errors++;
                }
                catch
                {
                    errors++;
                }
            }
            if (errors == 0)
            {
                MessageBox.Show("リネームを元に戻しました", "完了");
                undoMap.Clear();
            }
            else
            {
                MessageBox.Show(errors + "個のファイルを元に戻せませんでした", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e) { }

        private void subtibtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "";
            openFileDialog.Filter = "txtファイル(*.TXT;*.txt)|*.TEX;*.txt";
            openFileDialog.FilterIndex = 2;
            openFileDialog.Title = "開くファイルを選択してください";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Console.WriteLine(openFileDialog.FileName);
                Readsubfile readsubfile = new Readsubfile();
                subArray.Clear();
                subArray = readsubfile.readsubfile(openFileDialog.FileName);
                subtitb.Text = openFileDialog.FileName;
            }
        }

        private void アップデート確認ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Update().updatecheck();
        }

        private void checkbtn_Click(object sender, EventArgs e)
        {
            string title = titletb.Text;
            string dai = daitb.Text;
            string wa = watb.Text;
            int num = (int)stud.Value;
            int count = ListView1.Items.Count;
            bool symbol = symbolcb.Checked;
            bool stou = stoucb.Checked;
            bool usesub = subticb.Checked;
            bool useshobo = shoborbtn.Checked;
            bool usefile = fileselectrbtn.Checked;
            Rename rename = new Rename();
            string text = null;
            for (int i = 0; i < count; i++)
            {
                text = rename.makeriname(title, dai, wa, stou, symbol, num - 1, usesub, useshobo, usefile, shoboArray, subArray, i + 1, count);
                ListView1.Items[i].SubItems[2].Text = text;
            }
        }

        private async void getbtn_Click_1(object sender, EventArgs e)
        {
            string tid = tidtb.Text.Trim();
            if (string.IsNullOrEmpty(tid))
            {
                MessageBox.Show("TIDを入力してください", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            getbtn.Enabled = false;
            await FetchSyobocalDataAsync(tid);
            getbtn.Enabled = true;
        }

        private async Task FetchSyobocalDataAsync(string tid)
        {
            Httpgetmaker httpgetmaker = new Httpgetmaker();
            string url = "http://cal.syoboi.jp/db.php?Command=TitleLookup&TID=" + tid;
            string response = await httpgetmaker.gethttpAsync(url);

            string title = httpgetmaker.GetBetweenStrings("<Title>", "</Title>", response);
            if (title != null && title != "取得不可" && shobotitlecb.Checked)
            {
                titletb.Text = WebUtility.HtmlDecode(title);
            }

            string subTitles = httpgetmaker.GetBetweenStrings("<SubTitles>", "</SubTitles>", response);
            shoboArray.Clear();
            subTitles = subTitles.Replace("*", "<Stitle>");

            while (true)
            {
                string tag = "<Stitle>";
                int idx = subTitles.IndexOf(tag);
                if (idx == -1)
                {
                    if (shoboArray.Count == 0)
                        MessageBox.Show("しょぼかるからデータを取得できませんでした");
                    return;
                }
                int startIdx = idx + tag.Length;
                subTitles = subTitles.Substring(startIdx);
                idx = subTitles.IndexOf(tag);
                subTitles = subTitles.Substring(idx + 8);
                idx = subTitles.IndexOf(tag);
                if (idx == -1)
                    break;
                string stitle = subTitles.Substring(0, idx);
                stitle = stitle.Replace("\r\n", "");
                shoboArray.Add(stitle);
            }
            subTitles = subTitles.Replace("\r\n", "");
            shoboArray.Add(subTitles);

            shobosubkazu.Text = shoboArray.Count.ToString();
        }

        private async void searchbtn_Click(object sender, EventArgs e)
        {
            string keyword = keywordtb.Text.Trim();
            if (string.IsNullOrEmpty(keyword))
            {
                MessageBox.Show("検索キーワードを入力してください", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            searchResultlb.SelectedIndexChanged -= searchResultlb_SelectedIndexChanged;
            searchResultlb.Items.Clear();
            searchResultlb.Items.Add("検索中...");
            searchbtn.Enabled = false;

            Httpgetmaker httpgetmaker = new Httpgetmaker();
            string url = "http://cal.syoboi.jp/find?kw=" + Uri.EscapeDataString(keyword) + "&exec=search";
            string response = await httpgetmaker.gethttpAsync(url);
            response = WebUtility.HtmlDecode(response);

            searchResultlb.Items.Clear();
            var results = new System.Collections.Generic.List<System.Tuple<int, string>>();
            string remaining = response;
            while (true)
            {
                string startTag = "href=\"/tid/";
                int idx = remaining.IndexOf(startTag);
                if (idx == -1) break;

                remaining = remaining.Substring(idx + startTag.Length);
                int endIdx = remaining.IndexOf("\"");
                string tid = (endIdx > 0) ? remaining.Substring(0, endIdx) : "";

                int gtIdx = remaining.IndexOf(">");
                if (gtIdx == -1) continue;
                string afterGt = remaining.Substring(gtIdx + 1);
                int ltIdx = afterGt.IndexOf("<");
                string title = (ltIdx > 0) ? afterGt.Substring(0, ltIdx) : "(タイトル不明)";
                title = title.Trim();

                int tidNum;
                if (int.TryParse(tid, out tidNum))
                    results.Add(System.Tuple.Create(tidNum, tid + ": " + title));
            }

            results.Sort((a, b) => a.Item1.CompareTo(b.Item1));
            foreach (var r in results)
                searchResultlb.Items.Add(r.Item2);

            if (results.Count == 0)
                searchResultlb.Items.Add("(該当するタイトルが見つかりません)");
            searchResultlb.SelectedIndexChanged += searchResultlb_SelectedIndexChanged;
            searchbtn.Enabled = true;

            if (autotidcb.Checked && results.Count > 0)
                searchResultlb.SelectedIndex = 0;
        }

        private void searchResultlb_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selected = searchResultlb.SelectedItem as string;
            if (string.IsNullOrEmpty(selected)) return;
            if (selected.StartsWith("(") || selected.StartsWith("検索")) return;

            int colonIdx = selected.IndexOf(':');
            if (colonIdx <= 0) return;

            string tidCandidate = selected.Substring(0, colonIdx).Trim();
            int numeric;
            if (!int.TryParse(tidCandidate, out numeric)) return;

            tidtb.Text = tidCandidate;
            string titlePart = selected.Substring(colonIdx + 1).Trim();
            if (!string.IsNullOrEmpty(titlePart))
                titletb.Text = titlePart;
        }

        private void ブラウザToolStripMenuItem_Click(object sender, EventArgs e)
        {
            browserVisible = !browserVisible;
            browserPanel.Visible = browserVisible;
            if (browserVisible)
                webBrowser1.Navigate("http://cal.syoboi.jp/find?sd=0&kw=%E6%A4%9C%E7%B4%A2&ch=&st=&cm=&r=0&rd=&v=0");
        }

        private async void browserTidBtn_Click(object sender, EventArgs e)
        {
            string url = webBrowser1.Url.ToString();
            string tid = ExtractTidFromUrl(url);
            if (tid != "取得不可" && tid != null)
            {
                tidtb.Text = tid;
                browserVisible = false;
                browserPanel.Visible = false;
                browserTidBtn.Enabled = false;
                await FetchSyobocalDataAsync(tid);
                browserTidBtn.Enabled = true;
            }
            else
            {
                MessageBox.Show("TIDを取得できませんでした", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private string ExtractTidFromUrl(string url)
        {
            string prefix = "http://cal.syoboi.jp/tid/";
            int idx = url.IndexOf(prefix);
            if (idx == -1) return "取得不可";
            string remaining = url.Substring(idx + prefix.Length);
            int endIdx = remaining.IndexOf("/");
            if (endIdx > 0)
                return remaining.Substring(0, endIdx);
            return remaining;
        }

        private void browserBackBtn_Click(object sender, EventArgs e)
        {
            if (webBrowser1.CanGoBack) webBrowser1.GoBack();
        }

        private void browserForwardBtn_Click(object sender, EventArgs e)
        {
            if (webBrowser1.CanGoForward) webBrowser1.GoForward();
        }

        private void browserResetBtn_Click(object sender, EventArgs e)
        {
            webBrowser1.Navigate("http://cal.syoboi.jp/find?sd=0&kw=%E6%A4%9C%E7%B4%A2&ch=&st=&cm=&r=0&rd=&v=0");
        }

        private void keywordtb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                searchbtn.PerformClick();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            ListView1 = new ListView();
            renamebtn = new Button();
            clearbtn = new Button();
            sortkind = new Label();
            titletb = new TextBox();
            label1 = new Label();
            daitb = new TextBox();
            watb = new TextBox();
            stud = new NumericUpDown();
            symbolcb = new CheckBox();
            stoucb = new CheckBox();
            subtibtn = new Button();
            subtitb = new TextBox();
            subticb = new CheckBox();
            menuStrip1 = new MenuStrip();
            ヘルプToolStripMenuItem = new ToolStripMenuItem();
            アップデート確認ToolStripMenuItem = new ToolStripMenuItem();
            ブラウザToolStripMenuItem = new ToolStripMenuItem();
            Copyright = new Label();
            Verlabel = new Label();
            checkbtn = new Button();
            tidtb = new TextBox();
            label2 = new Label();
            getbtn = new Button();
            label3 = new Label();
            fileselectrbtn = new RadioButton();
            shoborbtn = new RadioButton();
            label4 = new Label();
            shobosubkazu = new Label();
            shobotitlecb = new CheckBox();
            undobtn = new Button();
            autotidcb = new CheckBox();

            keywordtb = new TextBox();
            searchbtn = new Button();
            searchResultlb = new ListBox();

            browserPanel = new Panel();
            webBrowser1 = new WebBrowser();
            browserBackBtn = new Button();
            browserForwardBtn = new Button();
            browserResetBtn = new Button();
            browserTidBtn = new Button();

            Label keywordLabel = new Label();

            ((ISupportInitialize)stud).BeginInit();
            menuStrip1.SuspendLayout();
            browserPanel.SuspendLayout();
            SuspendLayout();

            ListView1.Location = new Point(13, 42);
            ListView1.Name = "ListView1";
            ListView1.Size = new Size(774, 290);
            ListView1.TabIndex = 0;
            ListView1.UseCompatibleStateImageBehavior = false;

            // ===== RIGHT COLUMN: rename / clear / check / browser =====
            renamebtn.Location = new Point(712, 352);
            renamebtn.Name = "renamebtn";
            renamebtn.Size = new Size(75, 23);
            renamebtn.TabIndex = 1;
            renamebtn.Text = "リネーム";
            renamebtn.UseVisualStyleBackColor = true;
            renamebtn.Click += button1_Click;

            clearbtn.Location = new Point(712, 382);
            clearbtn.Name = "clearbtn";
            clearbtn.Size = new Size(75, 23);
            clearbtn.TabIndex = 2;
            clearbtn.Text = "クリア";
            clearbtn.UseVisualStyleBackColor = true;
            clearbtn.Click += clear_Click;

            deletebtn = new Button();
            deletebtn.Location = new Point(712, 412);
            deletebtn.Name = "deletebtn";
            deletebtn.Size = new Size(75, 23);
            deletebtn.TabIndex = 33;
            deletebtn.Text = "削除";
            deletebtn.UseVisualStyleBackColor = true;
            deletebtn.Click += deletebtn_Click;

            checkbtn.Location = new Point(712, 442);
            checkbtn.Name = "checkbtn";
            checkbtn.Size = new Size(75, 23);
            checkbtn.TabIndex = 17;
            checkbtn.Text = "確認";
            checkbtn.UseVisualStyleBackColor = true;
            checkbtn.Click += checkbtn_Click;

            undobtn.Location = new Point(712, 466);
            undobtn.Name = "undobtn";
            undobtn.Size = new Size(75, 23);
            undobtn.TabIndex = 34;
            undobtn.Text = "元に戻す";
            undobtn.UseVisualStyleBackColor = true;
            undobtn.Click += undobtn_Click;

            // ===== LEFT COLUMN: title / episode / source =====
            sortkind.AutoSize = true;
            sortkind.Location = new Point(453, 27);
            sortkind.Name = "sortkind";
            sortkind.Size = new Size(96, 12);
            sortkind.TabIndex = 3;
            sortkind.Text = "ソート：読み込み順";

            label1.AutoSize = true;
            label1.Location = new Point(12, 355);
            label1.Name = "label1";
            label1.Size = new Size(40, 12);
            label1.TabIndex = 5;
            label1.Text = "タイトル";

            titletb.Location = new Point(58, 352);
            titletb.Name = "titletb";
            titletb.Size = new Size(250, 19);
            titletb.TabIndex = 4;
            titletb.TextChanged += textBox1_TextChanged;

            daitb.Location = new Point(58, 382);
            daitb.Name = "daitb";
            daitb.Size = new Size(50, 19);
            daitb.TabIndex = 6;
            daitb.Text = "第";

            watb.Location = new Point(114, 381);
            watb.Name = "watb";
            watb.Size = new Size(50, 19);
            watb.TabIndex = 7;
            watb.Text = "話";

            stud.Location = new Point(170, 382);
            stud.Maximum = new decimal(new int[4] { 9999, 0, 0, 0 });
            stud.Name = "stud";
            stud.Size = new Size(58, 19);
            stud.TabIndex = 8;
            stud.Value = new decimal(new int[4] { 1, 0, 0, 0 });

            symbolcb.AutoSize = true;
            symbolcb.Checked = true;
            symbolcb.CheckState = CheckState.Checked;
            symbolcb.Location = new Point(234, 383);
            symbolcb.Name = "symbolcb";
            symbolcb.Size = new Size(88, 16);
            symbolcb.TabIndex = 9;
            symbolcb.Text = "「」を付加する";
            symbolcb.UseVisualStyleBackColor = true;

            stoucb.AutoSize = true;
            stoucb.Location = new Point(58, 460);
            stoucb.Name = "stoucb";
            stoucb.Size = new Size(188, 16);
            stoucb.TabIndex = 10;
            stoucb.Text = "スペースをアンダーバーに置き換える";
            stoucb.UseVisualStyleBackColor = true;

            subticb.AutoSize = true;
            subticb.Location = new Point(58, 410);
            subticb.Name = "subticb";
            subticb.Size = new Size(130, 16);
            subticb.TabIndex = 13;
            subticb.Text = "サブタイトルを付与する";
            subticb.UseVisualStyleBackColor = true;

            fileselectrbtn.AutoSize = true;
            fileselectrbtn.CausesValidation = false;
            fileselectrbtn.Location = new Point(194, 409);
            fileselectrbtn.Name = "fileselectrbtn";
            fileselectrbtn.Size = new Size(57, 16);
            fileselectrbtn.TabIndex = 22;
            fileselectrbtn.Text = "ファイル";
            fileselectrbtn.UseVisualStyleBackColor = true;

            shoborbtn.AutoSize = true;
            shoborbtn.Checked = true;
            shoborbtn.Location = new Point(257, 408);
            shoborbtn.Name = "shoborbtn";
            shoborbtn.Size = new Size(68, 16);
            shoborbtn.TabIndex = 23;
            shoborbtn.TabStop = true;
            shoborbtn.Text = "しょぼかる";
            shoborbtn.UseVisualStyleBackColor = true;

            label3.AutoSize = true;
            label3.Location = new Point(13, 436);
            label3.Name = "label3";
            label3.Size = new Size(39, 12);
            label3.TabIndex = 21;
            label3.Text = "ファイル";

            subtitb.Location = new Point(58, 432);
            subtitb.Name = "subtitb";
            subtitb.Size = new Size(250, 19);
            subtitb.TabIndex = 12;

            subtibtn.Location = new Point(314, 430);
            subtibtn.Name = "subtibtn";
            subtibtn.Size = new Size(75, 23);
            subtibtn.TabIndex = 11;
            subtibtn.Text = "開く";
            subtibtn.UseVisualStyleBackColor = true;
            subtibtn.Click += subtibtn_Click;

            // ===== RIGHT SIDE: Syobocal search section =====
            keywordLabel.AutoSize = true;
            keywordLabel.Location = new Point(395, 355);
            keywordLabel.Name = "keywordLabel";
            keywordLabel.Size = new Size(41, 12);
            keywordLabel.TabIndex = 29;
            keywordLabel.Text = "検索語";

            keywordtb.Location = new Point(440, 352);
            keywordtb.Name = "keywordtb";
            keywordtb.Size = new Size(120, 19);
            keywordtb.TabIndex = 28;
            keywordtb.KeyDown += keywordtb_KeyDown;

            searchbtn.Location = new Point(566, 350);
            searchbtn.Name = "searchbtn";
            searchbtn.Size = new Size(50, 23);
            searchbtn.TabIndex = 30;
            searchbtn.Text = "検索";
            searchbtn.UseVisualStyleBackColor = true;
            searchbtn.Click += searchbtn_Click;

            shobotitlecb.AutoSize = true;
            shobotitlecb.Location = new Point(622, 354);
            shobotitlecb.Name = "shobotitlecb";
            shobotitlecb.Size = new Size(84, 16);
            shobotitlecb.TabIndex = 26;
            shobotitlecb.Text = "タイトルを取得";
            shobotitlecb.UseVisualStyleBackColor = true;

            autotidcb.AutoSize = true;
            autotidcb.Checked = true;
            autotidcb.Location = new Point(622, 378);
            autotidcb.Name = "autotidcb";
            autotidcb.Size = new Size(96, 16);
            autotidcb.TabIndex = 35;
            autotidcb.Text = "自動TID取得";
            autotidcb.UseVisualStyleBackColor = true;

            searchResultlb.FormattingEnabled = true;
            searchResultlb.ItemHeight = 12;
            searchResultlb.Location = new Point(395, 376);
            searchResultlb.Name = "searchResultlb";
            searchResultlb.Size = new Size(311, 76);
            searchResultlb.TabIndex = 31;
            searchResultlb.SelectedIndexChanged += searchResultlb_SelectedIndexChanged;

            // ===== TID row =====
            label2.AutoSize = true;
            label2.Location = new Point(395, 470);
            label2.Name = "label2";
            label2.Size = new Size(23, 12);
            label2.TabIndex = 19;
            label2.Text = "TID";

            tidtb.Location = new Point(424, 467);
            tidtb.Name = "tidtb";
            tidtb.Size = new Size(90, 19);
            tidtb.TabIndex = 18;

            getbtn.Location = new Point(520, 465);
            getbtn.Name = "getbtn";
            getbtn.Size = new Size(55, 23);
            getbtn.TabIndex = 20;
            getbtn.Text = "取得";
            getbtn.UseVisualStyleBackColor = true;
            getbtn.Click += getbtn_Click_1;

            label4.AutoSize = true;
            label4.Location = new Point(395, 497);
            label4.Name = "label4";
            label4.Size = new Size(101, 12);
            label4.TabIndex = 24;
            label4.Text = "取得サブタイトル数：";

            shobosubkazu.AutoSize = true;
            shobosubkazu.Location = new Point(502, 497);
            shobosubkazu.Name = "shobosubkazu";
            shobosubkazu.Size = new Size(11, 12);
            shobosubkazu.TabIndex = 25;
            shobosubkazu.Text = "-";

            // ===== Menu =====
            menuStrip1.BackColor = SystemColors.Menu;
            menuStrip1.GripStyle = ToolStripGripStyle.Visible;
            menuStrip1.Items.AddRange(new ToolStripItem[2] { ヘルプToolStripMenuItem, ブラウザToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(800, 26);
            menuStrip1.TabIndex = 14;
            menuStrip1.Text = "menuStrip1";

            ヘルプToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[1] { アップデート確認ToolStripMenuItem });
            ヘルプToolStripMenuItem.Name = "ヘルプToolStripMenuItem";
            ヘルプToolStripMenuItem.Size = new Size(56, 22);
            ヘルプToolStripMenuItem.Text = "ヘルプ";

            アップデート確認ToolStripMenuItem.Name = "アップデート確認ToolStripMenuItem";
            アップデート確認ToolStripMenuItem.Size = new Size(172, 22);
            アップデート確認ToolStripMenuItem.Text = "アップデート確認";
            アップデート確認ToolStripMenuItem.Click += アップデート確認ToolStripMenuItem_Click;

            ブラウザToolStripMenuItem.Name = "ブラウザToolStripMenuItem";
            ブラウザToolStripMenuItem.Size = new Size(56, 22);
            ブラウザToolStripMenuItem.Text = "ブラウザ";
            ブラウザToolStripMenuItem.Click += ブラウザToolStripMenuItem_Click;

            Copyright.AutoSize = true;
            Copyright.Font = new Font("MS UI Gothic", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 128);
            Copyright.Location = new Point(633, 20);
            Copyright.Name = "Copyright";
            Copyright.Size = new Size(49, 11);
            Copyright.TabIndex = 15;
            Copyright.Text = "Copyright";

            Verlabel.AutoSize = true;
            Verlabel.Font = new Font("MS UI Gothic", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 128);
            Verlabel.Location = new Point(633, 9);
            Verlabel.Name = "Verlabel";
            Verlabel.Size = new Size(41, 11);
            Verlabel.TabIndex = 16;
            Verlabel.Text = "Version";

            // ===== Browser panel =====
            browserPanel.BorderStyle = BorderStyle.FixedSingle;
            browserPanel.Controls.Add(webBrowser1);
            browserPanel.Controls.Add(browserBackBtn);
            browserPanel.Controls.Add(browserForwardBtn);
            browserPanel.Controls.Add(browserResetBtn);
            browserPanel.Controls.Add(browserTidBtn);
            browserPanel.Location = new Point(13, 520);
            browserPanel.Name = "browserPanel";
            browserPanel.Size = new Size(774, 190);
            browserPanel.TabIndex = 28;
            browserPanel.Visible = false;

            webBrowser1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            webBrowser1.Location = new Point(3, 30);
            webBrowser1.MinimumSize = new Size(20, 20);
            webBrowser1.Name = "webBrowser1";
            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.Size = new Size(766, 152);
            webBrowser1.TabIndex = 0;

            browserBackBtn.Location = new Point(3, 3);
            browserBackBtn.Name = "browserBackBtn";
            browserBackBtn.Size = new Size(52, 23);
            browserBackBtn.TabIndex = 1;
            browserBackBtn.Text = "<戻る";
            browserBackBtn.UseVisualStyleBackColor = true;
            browserBackBtn.Click += browserBackBtn_Click;

            browserForwardBtn.Location = new Point(61, 3);
            browserForwardBtn.Name = "browserForwardBtn";
            browserForwardBtn.Size = new Size(52, 23);
            browserForwardBtn.TabIndex = 2;
            browserForwardBtn.Text = "進む>";
            browserForwardBtn.UseVisualStyleBackColor = true;
            browserForwardBtn.Click += browserForwardBtn_Click;

            browserResetBtn.Location = new Point(119, 3);
            browserResetBtn.Name = "browserResetBtn";
            browserResetBtn.Size = new Size(52, 23);
            browserResetBtn.TabIndex = 3;
            browserResetBtn.Text = "初期化";
            browserResetBtn.UseVisualStyleBackColor = true;
            browserResetBtn.Click += browserResetBtn_Click;

            browserTidBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            browserTidBtn.Location = new Point(694, 3);
            browserTidBtn.Name = "browserTidBtn";
            browserTidBtn.Size = new Size(75, 23);
            browserTidBtn.TabIndex = 4;
            browserTidBtn.Text = "TID取得";
            browserTidBtn.UseVisualStyleBackColor = true;
            browserTidBtn.Click += browserTidBtn_Click;

            // ===== Form =====
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(6f, 12f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 720);
            Controls.Add(searchResultlb);
            Controls.Add(searchbtn);
            Controls.Add(keywordtb);
            Controls.Add(keywordLabel);
            Controls.Add(autotidcb);
            Controls.Add(browserPanel);
            Controls.Add(undobtn);
            Controls.Add(deletebtn);
            Controls.Add(shobotitlecb);
            Controls.Add(shobosubkazu);
            Controls.Add(label4);
            Controls.Add(shoborbtn);
            Controls.Add(fileselectrbtn);
            Controls.Add(label3);
            Controls.Add(getbtn);
            Controls.Add(label2);
            Controls.Add(tidtb);
            Controls.Add(checkbtn);
            Controls.Add(Verlabel);
            Controls.Add(Copyright);
            Controls.Add(subticb);
            Controls.Add(subtitb);
            Controls.Add(subtibtn);
            Controls.Add(stoucb);
            Controls.Add(symbolcb);
            Controls.Add(stud);
            Controls.Add(watb);
            Controls.Add(daitb);
            Controls.Add(label1);
            Controls.Add(titletb);
            Controls.Add(sortkind);
            Controls.Add(clearbtn);
            Controls.Add(renamebtn);
            Controls.Add(ListView1);
            Controls.Add(menuStrip1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            MainMenuStrip = menuStrip1;
            Name = "Form1";
            Text = "ふぁいるりねーまー改";
            Load += Form1_Load;
            DragDrop += Form1_DragDrop;
            DragEnter += Form1_DragEnter;

            ((ISupportInitialize)stud).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            browserPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
