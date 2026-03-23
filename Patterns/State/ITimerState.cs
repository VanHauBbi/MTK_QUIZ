namespace DALTWNC_QUIZ.Patterns.State
{
    public interface ITimerState
    {
        string GetColorClass(); // Trả về class màu của Bootstrap (success, warning, danger)
        string GetMessage();    // Lời nhắn tương ứng
        bool ShouldPulse();     // Có tạo hiệu ứng nhịp tim (nhấp nháy) không?
    }

    // 1. Trạng thái thoải mái (Còn > 50% thời gian)
    public class RelaxedState : ITimerState
    {
        public string GetColorClass() => "bg-success";
        public string GetMessage() => "Thời gian còn dư dả, cứ bình tĩnh bạn nhé!";
        public bool ShouldPulse() => false;
    }

    // 2. Trạng thái cảnh báo (Còn < 30% thời gian)
    public class WarningState : ITimerState
    {
        public string GetColorClass() => "bg-warning text-dark";
        public string GetMessage() => "Nhanh tay lên nào, thời gian không còn nhiều!";
        public bool ShouldPulse() => false;
    }

    // 3. Trạng thái khẩn cấp (Còn < 60 giây)
    public class UrgentState : ITimerState
    {
        public string GetColorClass() => "bg-danger animate-pulse";
        public string GetMessage() => "KHẨN CẤP! Nộp bài ngay kẻo lỡ!";
        public bool ShouldPulse() => true;
    }
}