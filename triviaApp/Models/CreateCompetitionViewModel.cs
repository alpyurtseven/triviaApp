using System;
namespace triviaApp.Models
{
    public class CreateCompetitionViewModel
    {
        public Competition Competition { get; set; }
        public List<Category> AvailableCategories { get; set; }
        public List<QuestionSelectionViewModel> Questions { get; set; }
        public List<CategorySelectionViewModel> Categories { get; set; }

        public CreateCompetitionViewModel()
        {
            Categories = new List<CategorySelectionViewModel>();
            Questions = new List<QuestionSelectionViewModel>();
        }
    }

    public class CategorySelectionViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool IsSelected { get; set; }
    }

    public class QuestionSelectionViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionContent { get; set; }
        public bool IsSelected { get; set; }
        public int CategoryId { get; set; }  // Associate question with category for filtering
    }
}

