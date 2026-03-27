using DALTWNC_QUIZ.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DALTWNC_QUIZ.Patterns.Structural
{
    public interface IQuizFacade
    {
        Task<QuizAttempt> SubmitQuizAsync(int attemptId, Dictionary<string, string> userAnswers, int elapsedSeconds);
    }
}
