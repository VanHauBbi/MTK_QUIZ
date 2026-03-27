using DALTWNC_QUIZ.Models;

namespace DALTWNC_QUIZ.Patterns.Observer
{
    public interface IQuizObserver
    {
        void OnQuizSubmitted(QuizAttempt attempt);
    }
}