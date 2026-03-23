using DALTWNC_QUIZ.Data;
using DALTWNC_QUIZ.Models;
using System;
using System.Collections.Generic;
using System.Linq;
namespace DALTWNC_QUIZ.Patterns.Decorator.Service
{
    public class BasicSubmissionService : ISubmissionService
    {
        private readonly ApplicationDbContext _context;

        public BasicSubmissionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public QuizAttempt ProcessSubmission(int quizId, int customerId, List<int> selectedChoiceIds)
        {
            var totalQuestions = _context.QuizQuestions.Count(qq => qq.QuizID == quizId);

            var correctAnswersCount = _context.Choices
                .Count(c => selectedChoiceIds.Contains(c.ChoiceID) && c.IsCorrect);

            decimal score = totalQuestions > 0
                ? (decimal)correctAnswersCount / totalQuestions * 10
                : 0;

            return new QuizAttempt
            {
                QuizID = quizId,
                CustomerID = customerId,
                Score = score,
                TotalQuestions = totalQuestions,
                CorrectAnswers = correctAnswersCount,
                StartTime = DateTime.Now,
                IsCompleted = true
            };
        }
    }
}
