using DALTWNC_QUIZ.Data;
using DALTWNC_QUIZ.Models;
using System;

namespace DALTWNC_QUIZ.Patterns.TemplateMethod
{
    public class BasicQuizProcessor : QuizSubmitProcessor
    {
        public BasicQuizProcessor(ApplicationDbContext context) : base(context) { }

        protected override double CalculateScore(QuizAttempt attempt)
        {
            if (attempt.TotalQuestions == 0) return 0;

            double score = ((double)(attempt.CorrectAnswers ?? 0) / attempt.TotalQuestions) * 10;
            return Math.Round(score, 2);
        }
    }
}