using System.Windows;
using System.Windows.Documents;

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
                dateTextBlock.Text = newProj.publicationDate;
            }
            else {   }
        }
    }
}
