using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SignalRChatApp.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        public IActionResult Index() => View();
        
        // Renders the interface for sending a message via the user's email.
        public IActionResult Messenger() => View();
        
        // Renders the interface for group management or display.
        public IActionResult Group() => View();
    }
}
