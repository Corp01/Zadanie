using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using System.Web;
using CsvHelper;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using System.Diagnostics;
using CsvHelper.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;



namespace Zadanie
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            List<Global> ft = new List<Global>();
            ft.Add(new Global() { ID = 0, Site = "https://www.reuters.com/markets/", Name = "www.reuters.com" });
            ft.Add(new Global() { ID = 1, Site = "https://www.investing.com/news/commodities-news", Name = "www.investing.com" });
            comboBox1.DataSource = ft;
            comboBox1.DisplayMember = "Name";
        }

        private void Start_Click(object sender, EventArgs e)
        {
            Global ft1 = comboBox1.SelectedItem as Global;
            string[] Sites = new string[] { "/html/body/div[1]/div/main/div[3]/div/div[1]/div/ul/li[contains(@class, 'static-media-maximizer__card__3Ke0W static-media-maximizer__list__23N8R')]", "/html/body/div[5]/section/div[4]/article[contains(@class, 'js-article-item articleItem')]/div[1]" };
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(ft1.Site);
           string strPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            var titles = new List<Row>();

                var Headername = doc.DocumentNode.SelectNodes(Sites[ft1.ID]);
                foreach (var item in Headername)
                {
                    if (ft1.ID == 0)
                    {
                        string head = HttpUtility.HtmlDecode(item.SelectSingleNode(".//div/div/a").InnerText);
                        var dsc = item.SelectSingleNode(".//div/a");
                        string hrefValue = dsc.GetAttributeValue("href", string.Empty);
                        string link = "https://www.reuters.com" + hrefValue;
                        titles.Add(new Row { Title = head, Link = link });
                    }
                    else
                    {
                        string head = HttpUtility.HtmlDecode(item.SelectSingleNode(".//a").InnerText);
                        var dsc = item.SelectSingleNode(".//a");
                        string hrefValue = dsc.GetAttributeValue("href", string.Empty);
                        string link = ft1.Site + hrefValue;
                        titles.Add(new Row { Title = head, Link = link });
                    }
                }
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                ShouldQuote = args => true
            };
            var location = Path.Combine(strPath, "UNCqa9.csv");
            using (var writer = new StreamWriter(Path.Combine(strPath, "UNCqa9.csv")))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(titles);
            }
            openFileDialog1.FileName = location;
            BinData(location);
            File.Delete(location);

        }
        public void BinData(string filepath)
        {
            DataTable dt = new DataTable();
            string[] lines = File.ReadAllLines(filepath);
            if (lines.Length > 0)
            {
                string firstLine = lines[0];
                string[] headerLabels = firstLine.Split(',');
                foreach(string headerWord in headerLabels)
                {
                    dt.Columns.Add(new DataColumn(headerWord));

                }
                for (int r = 1; r < lines.Length; r++) 
                {
                    string[] dataWords = lines[r].Split('"');
                    DataRow dr = dt.NewRow();
                    int columnIndex = 1;
                    foreach ( string headerWord in headerLabels)
                    {
                        dr[headerWord] = dataWords[columnIndex++];
                        columnIndex++;
                    }
                    dt.Rows.Add(dr);
                }

            }
            if (dt.Rows.Count>0)
            {
                dataGridView1.DataSource = dt;
            }

        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("Press start", "Save problem",
                 MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                var options = new JsonWriterOptions
                {
                    Indented = true
                };
                Global ft1 = comboBox1.SelectedItem as Global;
                string[] Sites = new string[] { "/html/body/div[1]/div/div[2]/div/main/article/div/div[1]/div[2]/div/div[2]/p/text()", "/html/body/div[5]/section/div[contains(@class, 'WYSIWYG articlePage')]/p/text()"};
                HtmlWeb web = new HtmlWeb();
                var rowCount = dataGridView1.Rows.Count;
                string text = "";
                for (int i = 0; i <  rowCount - 1; i++)
                {
                    HtmlDocument doc = web.Load(dataGridView1.Rows[i].Cells[1].Value.ToString());
                    var Headername = doc.DocumentNode.SelectNodes(Sites[ft1.ID]);
                    using (var write = new StreamWriter(Path.Combine(Global.strPath, "Results.js"), true))
                    if (Headername == null)
                    {
                        MessageBox.Show("You've reached the articule limit from this site", "Site limit",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                        i = rowCount-1;
                    }
                    else
                    {
                        foreach (var item in Headername)
                        {
                                text = text + item.InnerText;
                        }
                    }
                    if (Headername != null)
                    {
                        using (var stream = new MemoryStream())
                        {
                            using (var writer = new Utf8JsonWriter(stream, options))
                            {
                                writer.WriteStartObject();
                                writer.WriteString("Title", dataGridView1.Rows[i].Cells[0].Value.ToString());
                                writer.WriteString("Link", dataGridView1.Rows[i].Cells[1].Value.ToString());
                                writer.WriteString("Text", text);
                                writer.WriteEndObject();
                            }
                            string json = Encoding.UTF8.GetString(stream.ToArray());
                            using (var writer = new StreamWriter(Path.Combine(Global.strPath, "Results.js"), true))
                                writer.Write(json);
                        }
                    }
                }
            }
        }
    }
}

