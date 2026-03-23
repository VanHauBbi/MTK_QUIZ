using DALTWNC_QUIZ.Models;

namespace DALTWNC_QUIZ.Patterns.Creational
{
    public class QuestionBuilder
    {
        private Question _question = new();

        public QuestionBuilder() => Reset();

        public void Reset() => _question = new Question { Choices = new List<Choice>() };

        public QuestionBuilder SetContent(string text, int subjectId)
        {
            _question.QuestionText = text;
            _question.SubjectID = subjectId;
            return this;
        }

        public QuestionBuilder AddChoice(string text, bool isCorrect)
        {
            _question.Choices.Add(new Choice { ChoiceText = text, IsCorrect = isCorrect });
            return this;
        }

        public Question Build()
        {
            var result = _question;
            Reset();
            return result;
        }
    }
}
