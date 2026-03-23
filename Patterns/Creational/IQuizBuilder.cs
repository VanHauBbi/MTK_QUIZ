using DALTWNC_QUIZ.Models;

namespace DALTWNC_QUIZ.Patterns.Creational
{
    public interface IQuizBuilder
    {
        IQuizBuilder SetBasicInfo(string title, string? description, string createdBy);
        IQuizBuilder ForSubject(int subjectId);
        IQuizBuilder SetStatus(bool isPublic, bool isDynamic);
        IQuizBuilder AddQuestion(Question question);
        Quiz Build();
        void Reset();
    }
}
