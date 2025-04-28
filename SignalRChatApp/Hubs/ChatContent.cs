namespace SignalRChatApp.Hubs
{
    public class ChatContent
    {
        public string? Text { get; set; }
        public FileAttachment? Attachment { get; set; }
    }

    public class FileAttachment
    {
        public string? FileName { get; set; }
        public string? Url { get; set; }
    }

}
