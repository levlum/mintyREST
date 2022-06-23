using Microsoft.AspNetCore.Mvc;
using Com.Gamegestalt.MintyScript;
using mintyREST.Models;

namespace mintyREST.Controllers;

[ApiController]
[Route("[controller]")]
public class SentenceController : ControllerBase
{

    private readonly ILogger<SentenceController> _logger;

    public SentenceController(ILogger<SentenceController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetSentence")]
    public IEnumerable<MintyItem> Get()
    {
        MintyScriptTest.Run();
        Sentence testS = new Sentence();
        testS.Text = "Weil [sentence:topic=deep_statements]";

        return Enumerable.Range(1, 5).Select(index => new MintyItem {
                Text = testS.Process(null, null, null, true),
                Name = "One Sentence",
                IsComplete = true
            }).ToArray();
    }
}
