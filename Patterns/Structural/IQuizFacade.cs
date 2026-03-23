using DALTWNC_QUIZ.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DALTWNC_QUIZ.Patterns.Structural
{
    public interface IQuizFacade
    {
        // Xử lý nộp bài thi và trả về đối tượng kết quả
        Task<QuizAttempt> SubmitQuizAsync(int attemptId, Dictionary<string, string> userAnswers, int elapsedSeconds);
    }
}
