using DALTWNC_QUIZ.Data;
using DALTWNC_QUIZ.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using DALTWNC_QUIZ.Patterns.Observer;

namespace DALTWNC_QUIZ.Patterns.TemplateMethod
{
    public abstract class QuizSubmitProcessor
    {
        protected readonly ApplicationDbContext _context;

        private readonly List<IQuizObserver> _observers = new List<IQuizObserver>();
        public QuizSubmitProcessor(ApplicationDbContext context)
        {
            _context = context;
        }
        public void Attach(IQuizObserver observer)
        {
            _observers.Add(observer);
        }

        public async Task<QuizAttempt> SubmitQuizAsync(int attemptId)
        {
            var attempt = await _context.QuizAttempts
                .Include(a => a.Quiz)
                .Include(a => a.QuizAttemptQuestions)
                    .ThenInclude(qaq => qaq.Question)
                        .ThenInclude(q => q.Choices)
                .FirstOrDefaultAsync(a => a.QuizAttemptID == attemptId);

            if (attempt == null) throw new Exception("Không tìm thấy bài thi.");

            if (attempt.IsCompleted) throw new Exception("Bài thi đã được hoàn thành.");

            attempt.IsCompleted = true;

            attempt.CorrectAnswers = CountCorrectAnswers(attempt);

            double calculatedScore = CalculateScore(attempt);
            attempt.Score = (decimal?)calculatedScore;

            _context.QuizAttempts.Update(attempt);
            await _context.SaveChangesAsync();

            PostProcessing(attempt);

            return attempt;
        }

        private int CountCorrectAnswers(QuizAttempt attempt)
        {
            return attempt.QuizAttemptQuestions.Count(qaq =>
                qaq.Question.Choices.Any(c => c.IsCorrect && c.ChoiceID == qaq.SelectedChoiceID));
        }

        protected abstract double CalculateScore(QuizAttempt attempt);

        protected virtual void PostProcessing(QuizAttempt attempt)
        {
            foreach (var observer in _observers)
            {
                observer.OnQuizSubmitted(attempt);
            }
        }
    }
}