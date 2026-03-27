using DALTWNC_QUIZ.Models;
using System.Diagnostics;

namespace DALTWNC_QUIZ.Patterns.Observer
{
    public class DashboardObserver : IQuizObserver
    {
        public void OnQuizSubmitted(QuizAttempt attempt)
        {
            Debug.WriteLine($"[DASHBOARD]: Đã cập nhật số liệu mới sau bài thi của Attempt ID {attempt.QuizAttemptID}");
        }
    }
}