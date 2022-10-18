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
        testS.Text = "[words/article/the:(s)] [s=subject(v)] [v=words/verb:p3] [words/article/the:(o)] [o=object(v):da] [words/article/the:(o2)] [?2:;[adjective(o2)]] [o2=object(v):ac]";

        return Enumerable.Range(1, 5).Select(index => new MintyItem {
                Text = testS.Process(null, null, null),
                Name = "One Sentence",
                IsComplete = true
            }).ToArray();
    }
}
