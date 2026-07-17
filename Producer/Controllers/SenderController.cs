using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Producer.services;

namespace Producer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SenderController : ControllerBase
    {
        private readonly IRabbitMQService _rabbitMQService;

        public SenderController(IRabbitMQService rabbitMQService)
        {
            _rabbitMQService = rabbitMQService;
        }
        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] MessageDTO message, [FromQuery] string routingKey)
        {
            await _rabbitMQService.Publish(message,routingKey);
            return Ok();
        }
    }
}
