using DALTWNC_QUIZ.Models;
using System.Collections.Generic;

namespace DALTWNC_QUIZ.Patterns.Adapter
{
    public class ExternalQuestionAdapter : IQuestionAdapter
    {
        public Question Convert(ExternalQuestion ext)
        {
            var question = new Question
            {
                QuestionText = ext.Text,
                Choices = new List<Choice>()
            };

            string type = ext.Type?.ToUpper().Trim() ?? "MCQ";

            if (type == "TF")
            {
                bool isTrue = ext.CorrectAnswer?.ToLower() == "true" || ext.CorrectAnswer?.ToUpper() == "T";
                question.Choices.Add(new Choice { ChoiceText = "Đúng", IsCorrect = isTrue });
                question.Choices.Add(new Choice { ChoiceText = "Sai", IsCorrect = !isTrue });
            }
            else
            {
                AddChoice(question, ext.AnswerA, ext.CorrectAnswer == "A");
                AddChoice(question, ext.AnswerB, ext.CorrectAnswer == "B");
                AddChoice(question, ext.AnswerC, ext.CorrectAnswer == "C");
                AddChoice(question, ext.AnswerD, ext.CorrectAnswer == "D");
            }

            return question;
        }

        private void AddChoice(Question q, string text, bool isCorrect)
        {
            if (!string.IsNullOrWhiteSpace(text))
                q.Choices.Add(new Choice { ChoiceText = text, IsCorrect = isCorrect });
        }
    }
}