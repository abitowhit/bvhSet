using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace bvhSet
{
    public partial class NumCheckFrm : Form
    {
        public NumCheckFrm()
        {
            InitializeComponent();
           richTextBox1.Text = ReadBF();
        }

        private string ReadBF()
        {
            string txt = "";
            if (File.Exists("bvh.txt"))
            {
                string[] bf = File.ReadAllLines("bvh.txt");
                foreach (string line in bf)
                {
                    txt += line.Trim() + "\n";
                }
            }
            return txt;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            rtbReport.Text = "";
            lblMsg.Text = "";
            if (Directory.Exists(tbSearchDir.Text))
            {
                treeView1.Nodes.Add(FileNodes(tbSearchDir.Text));
                treeView1.ExpandAll();
            }
            else
            {
            lblMsg.Text = "Invalid Path";
            }
        }
      public static TreeNode FileNodes(string inFilePath)
    {
        TreeNode root = new TreeNode();
        root.Text = inFilePath;
        if (Directory.Exists(inFilePath))
        {
            foreach (string directory in Directory.EnumerateDirectories(inFilePath))
            {
                root.Nodes.Add(FileNodes(directory));
            }
            foreach (string file in Directory.EnumerateFiles(inFilePath))
            {

                if (file.ToString().ToLower().EndsWith(".bvh"))
                {
                    TreeNode tn = new TreeNode();
                    tn.Text = file.ToString();
                    root.Nodes.Add(tn);
                }
               
            }

        }

        return root;

    }
        private string IsNode(string inFile)
        {
            bool match = false;
            string retVal = "";
            string[] fsplit = inFile.Split('\\');
            string numCheck="";
            if (fsplit.Length > 1 && inFile.ToLower().Contains(".bvh"))
            {
                numCheck = fsplit[fsplit.Length - 1].Replace(".bvh","");

            }
            if (numCheck != "")
            {
            string[] rtSplit = richTextBox1.Text.Split('\n');
            for (int i = 0; i < rtSplit.Length; i++)
            {
                string[] rowSplit = rtSplit[i].Split(' ');
                if (rowSplit.Length > 0)
                {
                    if (rowSplit[0].ToLower().Contains(numCheck))
                    {
                        retVal = rtSplit[i].Replace(" ", "_").Replace(",", "-").Replace("\t", "_").Replace("#","-");
                        match = true;
                    }
                }
            }
                
            }
            else
            {
                retVal += "numcheck not determined";
            }
            return retVal + " ("+ numCheck + "-" + match.ToString() +")";
        }
        private void btnCompare_Click(object sender, EventArgs e)
        {
            string rept = "";
            if (treeView1.Nodes.Count > 0)
            {
                foreach (TreeNode node in treeView1.Nodes)
                {
                    rept = PreviewNode(node,false);
                }
            }
            rtbReport.Text = rept;
        }
        public string PreviewNode(TreeNode inNode, bool commit)
        {
            string rept = "";
            if (inNode.Nodes.Count == 0)
            {
                if (commit)
                {
                    rept += RenameNode(inNode.Text);
                }
                else
                {
                    rept += string.Format("Rename {0} to {1}", inNode.Text, IsNode(inNode.Text)) + "\n";
                }
            }
            else
            {
                rept += "Directory " + inNode.Text + "\n";
                foreach (TreeNode tn in inNode.Nodes)
                {
                    rept += PreviewNode(tn, commit); ;
                }
            }
            return rept;
        }
        private string RenameNode(string inFile)
        {
            string[] invalidFileChars = new string[] { " ", ",", "\t", "\n", "\r", "#", "/", "'", "@", "?", "*", "$", "=", "+", "&", "^", "[", "{", ":", ";", "<", ">","\"" };
            bool match = false;
            string retVal = "";
            string[] fsplit = inFile.Split('\\');
            string numCheck = "";
            string newFilePath="";
            string newFileName = "";
            if (fsplit.Length > 1 && inFile.ToLower().Contains(".bvh"))
            {
                numCheck = fsplit[fsplit.Length - 1].Replace(".bvh", "");
            }
            if (numCheck != "")
            {
                string[] rtSplit = richTextBox1.Text.Split('\n');
                for (int i = 0; i < rtSplit.Length; i++)
                {
                    string[] rowSplit = rtSplit[i].Split(' ');
                    if (rowSplit.Length > 0)
                    {
                        if (rowSplit[0].ToLower().Contains(numCheck))
                        {
                            newFileName = rtSplit[i];
                            foreach (string ifc in invalidFileChars)
                            {
                                newFileName = newFileName.Replace(ifc, "_");
                            }
                            match = true;
                        }
                    }
                }

            }
            else
            {
                retVal += "numcheck not determined";
            }
            if (File.Exists(inFile) && match && numCheck != "" && !inFile.ToLower().Contains(newFileName))
            {
                try
                {
                    newFilePath = inFile.ToLower().Replace(numCheck.ToLower() + ".bvh", newFileName + ".bvh");
                    File.Copy(inFile, newFilePath);
                    File.Delete(inFile);
                    retVal = " Renamed (" + numCheck + "-" + newFileName + match.ToString() + ")";
                }
                catch (Exception ex)
                {
                    if (ex.ToString().ToLower().Contains("invalid char"))
                    {
                        retVal = string.Format("{0} Bad File Char", numCheck);
                    }
                    else
                    {
                        retVal = string.Format("{0} Error:{1}", numCheck, ex.ToString());
                    }
                }
            }
            else
            {
                string err = "";
                if (inFile.ToLower().Contains(newFileName))
                {
                    err = " already renamed.";
                }
                else if (numCheck == "")
                {
                    err = "Missing file info";
                }
                else
                {
                   err = "..Skip..invalid file?";
                }
                retVal = string.Format("{0} {1}", numCheck, err);
            }

            return retVal;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string rept = "";
            foreach (TreeNode tn in treeView1.Nodes)
            {
                rept += PreviewNode(tn, true);// ProcessNode(tn);
            }
            rtbReport.Text = rept;
        }
        public string ProcessNode(TreeNode inNode)
        {
            if (inNode.Nodes.Count == 0)
            {
                return  string.Format("Rename {0} to {1}", inNode.Text, RenameNode(inNode.Text)) + "\n";
            }
            else
            {
                foreach (TreeNode childNode in inNode.Nodes)
                {
                    return ProcessNode(childNode);
                }
            }
            return "";
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
                lblMsg.Text = treeView1.SelectedNode.Text;
            if (treeView1.SelectedNode.Nodes.Count > 0)
            {
            tbSearchDir.Text = treeView1.SelectedNode.Text;
            button2_Click(button2, EventArgs.Empty);
            }
        }
    }//class

}
