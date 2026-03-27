using DALTWNC_QUIZ.Data;
using DALTWNC_QUIZ.Models;
using DALTWNC_QUIZ.Patterns.Behavioral; 
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DALTWNC_QUIZ.Patterns.Structural
{
    public class QuizSubmissionFacade : IQuizFacade
    {
        private readonly ApplicationDbContext _context;
        private readonly IScoringStrategy _scoringStrategy; 

        public QuizSubmissionFacade(ApplicationDbContext context, IScoringStrategy scoringStrategy)
        {
            _context = context;
            _scoringStrategy = scoringStrategy;
        }

        public async Task<QuizAttempt?> SubmitQuizAsync(int attemptId, Dictionary<string, string> userAnswers, int elapsedSeconds)
        {
            var attempt = await _context.QuizAttempts
                .Include(qa => qa.QuizAttemptQuestions)
                    .ThenInclude(qaq => qaq.Question)
                        .ThenInclude(q => q.Choices)
                .FirstOrDefaultAsync(qa => qa.QuizAttemptID == attemptId && !qa.IsCompleted);

            if (attempt == null) return null;

            foreach (var attemptQuestion in attempt.QuizAttemptQuestions)
            {
                var qIdStr = attemptQuestion.QuestionID.ToString();
                if (userAnswers.TryGetValue(qIdStr, out string choiceIdStr) && int.TryParse(choiceIdStr, out int choiceId))
                {
                    attemptQuestion.SelectedChoiceID = choiceId;
                }
            }

            var scoringResult = _scoringStrategy.Calculate(attempt.QuizAttemptQuestions, attempt.TotalQuestions);

            attempt.CorrectAnswers = scoringResult.CorrectCount;
            attempt.Score = scoringResult.Score;

            attempt.DurationMinute = elapsedSeconds;
            attempt.IsCompleted = true;

            _context.QuizAttempts.Update(attempt);
            await _context.SaveChangesAsync();
            return attempt;
        }
    }
}