namespace DALTWNC_QUIZ.Patterns.State
{
    public interface ITimerState
    {
        string GetColorClass(); 
        string GetMessage();    
        bool ShouldPulse();     
    }

  
    public class RelaxedState : ITimerState
    {
        public string GetColorClass() => "bg-success";
        public string GetMessage() => "Thời gian còn dư dả, cứ bình tĩnh bạn nhé!";
        public bool ShouldPulse() => false;
    }

    
    public class WarningState : ITimerState
    {
        public string GetColorClass() => "bg-warning text-dark";
        public string GetMessage() => "Nhanh tay lên nào, thời gian không còn nhiều!";
        public bool ShouldPulse() => false;
    }

   
    public class UrgentState : ITimerState
    {
        public string GetColorClass() => "bg-danger animate-pulse";
        public string GetMessage() => "KHẨN CẤP! Nộp bài ngay kẻo lỡ!";
        public bool ShouldPulse() => true;
    }
}