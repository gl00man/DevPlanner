using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Google.Apis.Sheets.v4;
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
        List<NewApi> apiList;

        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static readonly string ApplicationName = "DevPlanner";
        string SpreadsheetId;
        string path;
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
            apiList = new List<NewApi>();
            inProgressProjectsList = new List<string>();

            ReadData();
            privateList.ItemsSource = privProjList;
            checkPrivate(null);

            ReadApiData();
            apisCmb.ItemsSource = apiList;
        }

        //Add new item

        private void acceptBtn_Click(object sender, RoutedEventArgs e)
        {
            bool isActual = false;
            bool isAllData = false;

            isAllData = checkTextboxes(isAllData);

            if (isAllData == true)
            {
                string publicationDate = DateTime.Now.ToString("dd-MM-yyy").ToString(); 

                DateTime? selectedDate = deadlineTxt.SelectedDate;
                string formatted = selectedDate.Value.ToString("dd-MM-yyy", System.Globalization.CultureInfo.InvariantCulture);

                NewProject newProj = new NewProject(titleTxt.Text, formatted, privacyCmb.Text, descriptionTxt.Text, publicationDate);

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
                    SaveToTxt(newProj);

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

            var range = $"!A1:E";
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
                    string desc = newProj.title + "|" + newProj.deadline + "|" + newProj.privacy + "|" + newProj.description + "|" + newProj.publicationDate;
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

        void accessSheets()
        {
            GoogleCredential credential;

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
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
            var range = $"A:E";
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
                        NewProject newProj = new NewProject(row[0].ToString(), dateParsed, row[2].ToString(), row[3].ToString(), row[4].ToString());
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
            var range = $"A:E";
            var valueRange = new ValueRange();

            var objectList = new List<object>() { newProj.title, newProj.deadline, newProj.privacy, newProj.description, newProj.publicationDate };
            valueRange.Values = new List<IList<object>> { objectList };

            var appendRequest = service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendResponse = appendRequest.Execute();
        }

        void ImportListToSheets()
        {
            var range = $"A:E";
            var valueRange = new ValueRange();

            foreach (NewProject proj in pubProjList)
            {
                var objectList = new List<object>() { proj.title, proj.deadline, proj.privacy, proj.description, proj.publicationDate };
                valueRange.Values = new List<IList<object>> { objectList };

                var appendRequest = service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                var appendResponse = appendRequest.Execute();
            }
        }

        //Saving & reading data (txt)

        bool SaveToTxt(NewProject newProj)
        {
            string dateParsed = dateParse(newProj.deadline);
            string desc = newProj.title + "|" + dateParsed + "|" + newProj.privacy + "|" + newProj.description + "|" + newProj.publicationDate;
            using (StreamWriter sw = new StreamWriter(@"assets\data.txt", true))
            {
                sw.WriteLine(desc);
                sw.Close();
            }
            return true;
        }

        internal void ReadData()
        {
            privProjList.Clear();
            using (StreamReader sr = new StreamReader(@"assets\data.txt"))
            {
                string line;

                while((line = sr.ReadLine()) != null)
                {
                    string[] par = line.Split('|');

                    NewProject newProj = new NewProject(par[0], par[1], par[2], par[3], par[4]);
                    if(!privProjList.Contains(newProj))
                    {
                        privProjList.Add(newProj);
                    }
                }
            }
        }

        void ReadApiData()
        {
            using (StreamReader sr = new StreamReader(@"assets\api.txt"))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] par = line.Split('|');

                    NewApi na = new NewApi(par[0], par[1], par[2]);
                    if (!apiList.Contains(na))
                    {
                        apiList.Add(na);
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

        void deadlineMessage()
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
            if (pubMessage.Length > 0)
            {
                message += pubMessage;
                message += ',';
            }
            if (message.Length > 0)
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
            apisCmb.ItemsSource = apiList;
            pubProjList.Clear();
            readSheetData();
            publicList.ItemsSource = null;
            publicList.ItemsSource = pubProjList;
        }

        public void getApi(NewApi api)
        {
            SpreadsheetId = api.id;
            path = api.path;

            inProgressProjectsList.Clear();
            pubProjList.Clear();
            accessSheets();
            readSheetData();
            publicList.ItemsSource = null;
            publicList.ItemsSource = pubProjList;

            inProgressList.ItemsSource = null;
            inProgressList.ItemsSource = inProgressProjectsList;

            deadlineMessage();
        }

        private void addConenctionBtn_Click(object sender, RoutedEventArgs e)
        {
            ApiWindow aw = new ApiWindow();
            aw.Show();
        }

        private void connectBtn_Click(object sender, RoutedEventArgs e)
        {
            getApi(apisCmb.SelectedItem as NewApi);
        }

        //Maximal title and description character check

        private void titleTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            string content = titleTxt.Text;
            int lenght = content.Length;
            if(lenght > 15)
            {
                MessageBox.Show("Title is too long!");
                titleTxt.Text = content.Substring(0, 15);
            }
        }

        private void descriptionTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            string content = descriptionTxt.Text;
            int lenght = descriptionTxt.Text.Length;
            if (lenght > 220)
            {
                MessageBox.Show("Description is too long!");
                descriptionTxt.Text = content.Substring(0, 220);
            }
        }
    }
}
