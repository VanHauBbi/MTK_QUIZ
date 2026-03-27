using DALTWNC_QUIZ.Data;
using DALTWNC_QUIZ.Models;
using System;
using System.Linq;

namespace DALTWNC_QUIZ.Patterns.TemplateMethod
{
    public class PenaltyQuizProcessor : QuizSubmitProcessor
    {
        public PenaltyQuizProcessor(ApplicationDbContext context) : base(context) { }

        protected override double CalculateScore(QuizAttempt attempt)
        {
            if (attempt.TotalQuestions == 0) return 0;

            double totalScore = 0;
            double pointsPerCorrect = 1.0;
            double penaltyPerWrong = 0.25;

            foreach (var qaq in attempt.QuizAttemptQuestions)
            {
                var correctChoice = qaq.Question.Choices.FirstOrDefault(c => c.IsCorrect);

                if (correctChoice != null && qaq.SelectedChoiceID == correctChoice.ChoiceID)
                {
                    totalScore += pointsPerCorrect;
                }
                else if (qaq.SelectedChoiceID != null)
                {
                    totalScore -= penaltyPerWrong;
                }
            }

            double finalScore = (totalScore / attempt.TotalQuestions) * 10;

            return finalScore < 0 ? 0 : Math.Round(finalScore, 2);
        }
    }
}