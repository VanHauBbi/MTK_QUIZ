using DALTWNC_QUIZ.Data;
using DALTWNC_QUIZ.Models;
using System.Collections.Generic;
using System.Diagnostics; // Thêm thư viện này để in ra cửa sổ Output

namespace DALTWNC_QUIZ.Patterns.Decorator.Service
{
    public abstract class SubmissionDecorator : ISubmissionService
    {
        protected readonly ISubmissionService _innerService;

        protected SubmissionDecorator(ISubmissionService innerService)
        {
            _innerService = innerService;
        }

        public virtual QuizAttempt ProcessSubmission(int quizId, int customerId, List<int> selectedChoiceIds)
        {
            return _innerService.ProcessSubmission(quizId, customerId, selectedChoiceIds);
        }
    }

    public class DatabaseSavingDecorator : SubmissionDecorator
    {
        private readonly ApplicationDbContext _context;

        public DatabaseSavingDecorator(ISubmissionService inner, ApplicationDbContext context) : base(inner)
        {
            _context = context;
        }

        public override QuizAttempt ProcessSubmission(int quizId, int customerId, List<int> selectedChoiceIds)
        {
            var attempt = base.ProcessSubmission(quizId, customerId, selectedChoiceIds);

            _context.QuizAttempts.Add(attempt);
            _context.SaveChanges();

            return attempt;
        }
    }

    // THÊM LỚP NÀY VÀO ĐỂ HIỆN THÔNG BÁO ĐIỂM CAO
    public class HighScoreAlertDecorator : SubmissionDecorator
    {
        public HighScoreAlertDecorator(ISubmissionService inner) : base(inner) { }

        public override QuizAttempt ProcessSubmission(int quizId, int customerId, List<int> selectedChoiceIds)
        {
            // Chạy logic của các lớp bên trong (Tính điểm, Lưu DB)
            var attempt = base.ProcessSubmission(quizId, customerId, selectedChoiceIds);

            // Kiểm tra nếu điểm >= 8
            if (attempt.Score.GetValueOrDefault() >= 8.0m)
            {
                // In ra cửa sổ Output (Debug) của Visual Studio
                Debug.WriteLine("****************************************************");
                Debug.WriteLine("=== THÔNG BÁO: CHÚC MỪNG BẠN ĐÃ ĐẠT ĐIỂM GIỎI! ===");
                Debug.WriteLine($"=== SỐ ĐIỂM CỦA BẠN LÀ: {attempt.Score} ===");
                Debug.WriteLine("****************************************************");
            }

            return attempt;
        }
    }
}