
namespace SwiftDrop.Helpers
{
   public enum AlertMessageType{
        Success,
        Warning,
        Danger,
        Info

    }
public class AlertMessage
    {
       
        public AlertMessageType Type { get; set; }
        public string Text { get; set; }
    }
}


