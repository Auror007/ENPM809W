using ITSupportPortal.Models;

namespace ITSupportPortal.ViewModels
{
    public class OpenCaseViewModel
    {
        public Case openCase { get; set; }
        public IEnumerable<ChatHistory>  chatHistory { get; set; }
    }
}
