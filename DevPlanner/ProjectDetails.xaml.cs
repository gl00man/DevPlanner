using System;
using System.Collections.Generic;
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
    public partial class ProjectDetails : Window
    {
        public ProjectDetails(NewProject newProj)
        {
            InitializeComponent();

            if(newProj != null)
            { 
                titleTextBlock.Text = newProj.title;
                descrpitonRichTextBox.Document.Blocks.Clear();
                descrpitonRichTextBox.Document.Blocks.Add(new Paragraph(new Run(newProj.description)));
                deadlineTextBlock.Text = newProj.deadline;
            }
            else
            {

            }

            
        }
    }
}
