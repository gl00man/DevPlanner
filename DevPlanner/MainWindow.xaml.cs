using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Runtime.ExceptionServices;
using Google.Apis.Sheets.v4;
using System.Net;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4.Data;

namespace DevPlanner
{
    public partial class MainWindow : Window
    {
        string date;
        String[] dateTable;

        List<string> inProgressProjectsList;

        List<NewProject> pubProjList;
        List<NewProject> privProjList;

        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static readonly string ApplicationName = "DevPlanner"; 
        static readonly string SpreadsheetId = ""; //Your google sheet id
        static SheetsService service;

        public MainWindow()
        {
            InitializeComponent();

            date = DateTime.Now.ToString("dd.MM.yyy");
            dateTable = date.Split('.');
            Array.Reverse(dateTable);
            date = "";
            foreach(string x in dateTable)
            {
                date += x;
                date += '-';
            }
            date = date.Remove(date.Length - 1);

            pubProjList = new List<NewProject>();
            privProjList = new List<NewProject>();
            inProgressProjectsList = new List<string>();

            ReadData();
            privateList.ItemsSource = privProjList;

            accessSheets();
            readSheetData();
            publicList.ItemsSource = pubProjList;

        }

        //Add new item

        private void acceptBtn_Click(object sender, RoutedEventArgs e)
        {
            bool isActual = false;
            bool isAllData = false;

            isAllData = checkTextboxes(isAllData);

            if (isAllData == true)
            {
                DateTime? selectedDate = deadlineTxt.SelectedDate;
                string formatted = selectedDate.Value.ToString("dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);

                NewProject newProj = new NewProject(titleTxt.Text, formatted, privacyCmb.Text, descriptionTxt.Text);

                if (privacyCmb.Text == "Public")
                {

                    pubProjList.Add(newProj);
                    publicList.ItemsSource = null;
                    publicList.ItemsSource = pubProjList;
                    saveProjectToSheet(newProj);

                    DateTime actualDate = DateTime.Parse(date);
                    DateTime deadlineDate = DateTime.Parse(deadlineTxt.SelectedDate.ToString());
                    isActual = checkDate(false, actualDate, deadlineDate);

                    if (isActual == true)
                    {
                        inProgressProjectsList.Add(newProj.title);
                    }
                    else { }

                    inProgressList.ItemsSource = null;
                    inProgressList.ItemsSource = inProgressProjectsList;
                }
                else if (privacyCmb.Text == "Private")
                {

                    privProjList.Add(newProj);
                    privateList.ItemsSource = null;
                    privateList.ItemsSource = privProjList;
                    SaveToTxt();

                    DateTime actualDate = DateTime.Parse(date);
                    DateTime deadlineDate = DateTime.Parse(deadlineTxt.SelectedDate.ToString());
                    isActual = checkDate(false, actualDate, deadlineDate);

                    if (isActual == true)
                    {
                        inProgressProjectsList.Add(newProj.title);
                    }
                    else { }

                    inProgressList.ItemsSource = null;
                    inProgressList.ItemsSource = inProgressProjectsList;
                }
            }
            else
            {
                MessageBox.Show("Fill all textboxes!");
            }
        }

        bool checkTextboxes(bool gateway)
        {
            if (titleTxt.Text != "" && descriptionTxt.Text != "" && deadlineTxt.Text != "" && privacyCmb.Text != "")
            {
                gateway = true;
            }
            else
            {
                gateway = false;
            }

            return gateway;
        }


        //Clear textboxes

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            titleTxt.Clear();
            descriptionTxt.Clear();
            deadlineTxt.SelectedDate = null;
            privacyCmb.Text = "";
        }

        //Remove items

        void RemovePublicProject(NewProject selectedProj)
        {
            pubProjList.Remove(selectedProj);
            inProgressProjectsList.Remove(selectedProj.title);

            var range = $"!A1:D";
            var requestBody = new ClearValuesRequest();

            var deleteRequest = service.Spreadsheets.Values.Clear(requestBody, SpreadsheetId, range);
            deleteRequest.Execute();

            ImportListToSheets();
        }

        void RemovePrivateProject(NewProject selectedProj)
        {
            privProjList.Remove(selectedProj);
            inProgressProjectsList.Remove(selectedProj.title);

            using (FileStream fs = new FileStream("data.txt", FileMode.Truncate))
            { }

            using (StreamWriter sw = new StreamWriter("data.txt", true))
            {
                foreach(NewProject newProj in privProjList)
                {
                    string desc = newProj.title + "|" + newProj.deadline + "|" + newProj.privacy + "|" + newProj.description;
                    sw.WriteLine(desc);
                }
            }
        }

        private void delPublicBtn_Click(object sender, RoutedEventArgs e)
        {
            RemovePublicProject(publicList.SelectedItem as NewProject);
            publicList.ItemsSource = null;
            publicList.ItemsSource = pubProjList;

            inProgressList.ItemsSource = null;
            inProgressList.ItemsSource = inProgressProjectsList;
        }

        private void delPrivateBtn_Click(object sender, RoutedEventArgs e)
        {
            RemovePrivateProject(privateList.SelectedItem as NewProject);
            privateList.ItemsSource = null;
            privateList.ItemsSource = privProjList;

            inProgressList.ItemsSource = null;
            inProgressList.ItemsSource = inProgressProjectsList;
        }

        //Saving & reading data (sheets)

        static void accessSheets()
        {
            GoogleCredential credential;

            using (var stream = new FileStream("", FileMode.Open, FileAccess.Read)) //in "" type title of your api key .json file
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }

            service = new SheetsService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        void readSheetData()
        { 
            var range = $"A:D";
            var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);

            try
            {
                var response = request.Execute();
                var values = response.Values;
                if(values != null && values.Count > 0)
                {
                    foreach(var row in values)
                    {
                        string dateParsed = dateParse(row[1].ToString());
                        NewProject newProj = new NewProject(row[0].ToString(), dateParsed, row[2].ToString(), row[3].ToString());
                        if (!pubProjList.Contains(newProj))
                        {
                            pubProjList.Add(newProj);
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("No internet connection :(");
            }
        }

        string dateParse(string date)
        {
            dateTable = date.Split('-');
            Array.Reverse(dateTable);
            date = "";
            foreach(string x in dateTable)
            {
                date += x;
                date += '-';
            }
            date = date.Remove(date.Length - 1);

            return date;
        }

        void saveProjectToSheet(NewProject newProj)
        {
            var range = $"A:D";
            var valueRange = new ValueRange();

            var objectList = new List<object>() { newProj.title, newProj.deadline, newProj.privacy, newProj.description };
            valueRange.Values = new List<IList<object>> { objectList };

            var appendRequest = service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendResponse = appendRequest.Execute();
        }

        void ImportListToSheets()
        {
            var range = $"A:D";
            var valueRange = new ValueRange();

            foreach (NewProject proj in pubProjList)
            {
                var objectList = new List<object>() { proj.title, proj.deadline, proj.privacy, proj.description };
                valueRange.Values = new List<IList<object>> { objectList };

                var appendRequest = service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                var appendResponse = appendRequest.Execute();
            }
        }

        //Saving & reading data (txt)

        bool SaveToTxt()
        {
            string desc = titleTxt.Text + "|" + deadlineTxt.Text + "|" + privacyCmb.Text + "|" + descriptionTxt.Text;
            using (StreamWriter sw = new StreamWriter("data.txt", true))
            {
                sw.WriteLine(desc);
                sw.Close();
            }
            return true;
        }

        internal void ReadData()
        {
            privProjList.Clear();
            using (StreamReader sr = new StreamReader("data.txt"))
            {
                string line;

                while((line = sr.ReadLine()) != null)
                {
                    string[] par = line.Split('|');

                    NewProject newProj = new NewProject(par[0], par[1], par[2], par[3]);
                    if(!privProjList.Contains(newProj))
                    {
                        privProjList.Add(newProj);
                    }
                }
            }
        }

        //Clicking on list for details

        private void privateList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ProjectDetails projDetails = new ProjectDetails(privateList.SelectedItem as NewProject);
            projDetails.Show();
        }

        //Checking deadlines
        bool checkDate(bool isActual, DateTime actualDate, DateTime deadlineDate)
        {

            if (actualDate < deadlineDate)
            {
                isActual = true;
            }
            else if (actualDate == deadlineDate)
            {
;               isActual = true;
            }
            else 
            {
                isActual = false;
            }

            return isActual;
        }

        string checkPublic(string finish)
        {
            finish = "";

            foreach (NewProject newProj in pubProjList)
            {
                bool isActual = false;

                DateTime actualDate = DateTime.Parse(date);
                DateTime deadlineDate = DateTime.Parse(newProj.deadline);

                if (actualDate < deadlineDate)
                {
                    inProgressProjectsList.Add(newProj.title);
                }
                else if (actualDate == deadlineDate)
                {
                    inProgressProjectsList.Add(newProj.title);
                    finish += newProj.title;
                    finish += " ,";
                }
                else { }
            }

            inProgressList.ItemsSource = null;
            inProgressList.ItemsSource = inProgressProjectsList;

            if (finish.Length > 0)
            {
                finish = finish.Remove(finish.Length - 1);
            }
            else { }

            return finish;
        }

        string checkPrivate(string finish)
        {
            finish = "";

            foreach (NewProject newProj in privProjList)
            {
                bool isActual = false;

                DateTime actualDate = DateTime.Parse(date);
                DateTime deadlineDate = DateTime.Parse(newProj.deadline);

                if (actualDate < deadlineDate)
                {
                    inProgressProjectsList.Add(newProj.title);
                }
                else if (actualDate == deadlineDate)
                {
                    inProgressProjectsList.Add(newProj.title);
                    finish += newProj.title;
                    finish += " ,";
                }
                else { }
            }

            inProgressList.ItemsSource = null;
            inProgressList.ItemsSource = inProgressProjectsList;

            if (finish.Length > 0)
            {
                finish = finish.Remove(finish.Length - 1);
            }
            else { }

            return finish;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string message = "";
            string privMessage = "";
            string pubMessage = "";

            pubMessage = checkPublic(pubMessage);
            privMessage = checkPrivate(privMessage);
            if (privMessage.Length > 0)
            {
                message += privMessage;
                message += ',';
            }
            if(pubMessage.Length > 0)
            {
                message += pubMessage;
                message += ',';
            }
            if(message.Length > 0)
            {
                message = message.Remove(message.Length - 1);
                MessageBox.Show("Deadlines Today: " + message);
            }
        }

        private void publicList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ProjectDetails projDetails = new ProjectDetails(publicList.SelectedItem as NewProject);
            projDetails.Show();
        }

        private void refreshPublicBtn_Click(object sender, RoutedEventArgs e)
        {
            pubProjList.Clear();
            readSheetData();
            publicList.ItemsSource = null;
            publicList.ItemsSource = pubProjList;
        }
    }
}
