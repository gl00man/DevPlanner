using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DevPlanner
{
    public partial class ApiWindow : Window
    {
        public ApiWindow()
        {
            InitializeComponent();
        }

        //Buttons

        private void BrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.Filter = "Json files (*.json)|*.json";
            fileDialog.DefaultExt = ".json";
            Nullable<bool> dialogOK = fileDialog.ShowDialog();

            if (dialogOK == true)
            {
                string filePath = "";
                foreach (string x in fileDialog.FileNames)
                {
                    filePath += x;
                }
                pathTxt.Text = filePath;
            }
        }

        private void saveApiBtn_Click(object sender, RoutedEventArgs e)
        {
            NewApi na = new NewApi(sheetNameTxt.Text, pathTxt.Text, sheetIdTxt.Text);
            SaveToFile(na);
            MessageBox.Show("Restart DevPlanner to upload your changes");
            this.Close();
        }

        //saving to txt

        bool SaveToFile(NewApi api)
        {
            using (StreamWriter sw = new StreamWriter("api.txt", true))
            {
                sw.WriteLine("{0}|{1}|{2}", api.name, api.path, api.id);
                sw.Close();
            }
            return true;
        }

        
    }
}
