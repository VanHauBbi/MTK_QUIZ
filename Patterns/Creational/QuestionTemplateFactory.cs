using DALTWNC_QUIZ.Models;
using System.Collections.Generic;
using static DALTWNC_QUIZ.Pages.Admin.QuestionsBank.CreateModel;

namespace DALTWNC_QUIZ.Patterns.Creational
{
    public enum QuestionType
    {
        Standard,
        TrueFalse
    }

    public class QuestionSetup
    {
        public Question Question { get; set; } = new Question();
        public List<ChoiceInputModel> Choices { get; set; } = new List<ChoiceInputModel>();
    }

    public interface IQuestionFactory
    {
        QuestionSetup CreateTemplate();
    }

    public class StandardQuestionFactory : IQuestionFactory
    {
        public QuestionSetup CreateTemplate()
        {
            return new QuestionSetup
            {
                Question = new Question(),
                Choices = new List<ChoiceInputModel> { new(), new(), new(), new() }
            };
        }
    }

    public class TrueFalseQuestionFactory : IQuestionFactory
    {
        public QuestionSetup CreateTemplate()
        {
            return new QuestionSetup
            {
                Question = new Question(),
                Choices = new List<ChoiceInputModel>
                {
                    new ChoiceInputModel { ChoiceText = "Đúng" },
                    new ChoiceInputModel { ChoiceText = "Sai" }
                }
            };
        }
    }

    public static class QuestionCreator
    {
        public static IQuestionFactory GetFactory(QuestionType type)
        {
            if (type == QuestionType.TrueFalse)
                return new TrueFalseQuestionFactory();

            return new StandardQuestionFactory();
        }
    }
}