using DocumentSerials.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace DocumentSerials
{
    public partial class xml_importer : Form
    {
        private List<object> XmlContent { get; set; }
        private ServerDatabase connection { get; set; }
        public xml_importer()
        {
            InitializeComponent();
            XmlContent = new List<object>();
            connection = new ServerDatabase();
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                var fileContent = string.Empty;
                dialog.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
                dialog.FilterIndex = 2;
                dialog.RestoreDirectory = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    xmlFile.Text = dialog.FileName;
                    XmlContent = LoadXml(xmlFile.Text);
                }
            }
        }

        private List<object> LoadXml(string xml_path)
        {
            List<object> ret = new List<object>();
            XElement xml = XElement.Load(xml_path);
            if(xml.Name.LocalName.Equals("books"))
            {
                IEnumerable<XElement> nodes = xml.Descendants().Where(x => x.Attribute("id") != null);
                foreach(var node in nodes)
                {
                    string id = node.Attribute("id").Value;
                    string title = node.Value;
                    ret.Add(new Book(Convert.ToInt32(id), title, ""));
                }
                textBox1.Text = "BOOKS XML LOADED";
            }

            if (xml.Name.LocalName.Equals("countries"))
            {
                IEnumerable<XElement> nodes = xml.Descendants().Where(x => x.Attribute("iso") != null);
                foreach (var node in nodes)
                {
                    string iso = node.Attribute("iso").Value;
                    string code = node.Attribute("code").Value;
                    string name = node.Value;
                    ret.Add(new Country(Convert.ToInt32(iso), code, name));
                }
                textBox1.Text = "COUNTRIES XML LOADED";
            }

            if (xml.Name.LocalName.Equals("durations"))
            {
                IEnumerable<XElement> nodes = xml.Descendants().Where(x => x.Attribute("id") != null);
                foreach (var node in nodes)
                {
                    string id = node.Attribute("id").Value;
                    string description = node.Value;
                    ret.Add(new Duration(Convert.ToInt32(id), description));
                }
                textBox1.Text = "DURATIONS XML LOADED";
            }
            return ret;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(XmlContent.Count > 0)
            {
                Type type = XmlContent[0].GetType();
                string obj = type.Name;
                if(connection.InsertXML(XmlContent, type))
                {
                    MessageBox.Show(obj + " list correctly inserted into DB");
                }
                else
                {
                    MessageBox.Show("Error: The " + obj + " list has already been imported into the DB");
                }
            }
        }
    }
}
