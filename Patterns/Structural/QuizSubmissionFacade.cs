using DALTWNC_QUIZ.Data;
using DALTWNC_QUIZ.Models;
using DALTWNC_QUIZ.Patterns.Behavioral; // Thêm namespace của Strategy
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DALTWNC_QUIZ.Patterns.Structural
{
    public class QuizSubmissionFacade : IQuizFacade
    {
        private readonly ApplicationDbContext _context;
        private readonly IScoringStrategy _scoringStrategy; // Khai báo Strategy

        // Tiêm Strategy vào Facade
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

            // 1. Chỉ gán đáp án người dùng chọn vào các câu hỏi
            foreach (var attemptQuestion in attempt.QuizAttemptQuestions)
            {
                var qIdStr = attemptQuestion.QuestionID.ToString();
                if (userAnswers.TryGetValue(qIdStr, out string choiceIdStr) && int.TryParse(choiceIdStr, out int choiceId))
                {
                    attemptQuestion.SelectedChoiceID = choiceId;
                }
            }

            // 2. NHỜ STRATEGY TÍNH ĐIỂM GIÚP (Facade không tự tính nữa)
            var scoringResult = _scoringStrategy.Calculate(attempt.QuizAttemptQuestions, attempt.TotalQuestions);

            // 3. Cập nhật kết quả từ Strategy trả về
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