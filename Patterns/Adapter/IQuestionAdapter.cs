using DALTWNC_QUIZ.Models;

namespace DALTWNC_QUIZ.Patterns.Adapter
{
    public interface IQuestionAdapter
    {
        Question Convert(ExternalQuestion externalQ);
    }
}