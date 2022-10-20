using Com.Gamegestalt.MintyScript;
using mintyREST.Models;
using System;
using System.IO;

// StreamWriter sw = new StreamWriter(new FileStream("MintyRESTlog.txt", FileMode.Create));
// System.Console.SetOut(sw);
var allow_schubu = "_AllowSchuBu";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allow_schubu,
                      builder =>
                      {
                          builder.SetIsOriginAllowedToAllowWildcardSubdomains()
                          .WithOrigins("https://*.schubu.at", "https://schubu.at").AllowAnyHeader();
                      });
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(allow_schubu);
app.UseAuthorization();

//app.MapControllers();
app.MapGet("/sentence", async () => {
    Sentence testS = new Sentence();
    testS.Text = "[words/article/the:(s)] [s=subject(v)] [v=words/verb:p3] [words/article/the:(o)] [o=object(v):da] [words/article/the:(o2)] [?2:;[adjective(o2)]] [o2=object(v):ac]";

    return Enumerable.Range(1, 5).Select(index => new MintyItem {
            Text = testS.Process(null, null, null),
            Name = "One Sentence",
            IsComplete = true
        }).ToArray();
});

app.MapPost("/sentences", async (ScriptRequest request) => {
    Console.WriteLine("request: "+request.ToString());
    Sentence testS = new Sentence();
    testS.Text = request.Script;

    return Enumerable.Range(1, request.Count).Select(index => new MintyItem {
            Text = MintyUtils.AddDotAndBigStartingLetters(testS.Process(new CharacterWrapper("Lev", GenderType.MALE), new CharacterWrapper("Matl", GenderType.FEMALE), null)),
            Name = "One Sentence",
            IsComplete = true
        }).ToArray();
});

Com.Gamegestalt.MintyScript.Import.ImportFromHTML.ImportAll();

app.Run();

class ScriptRequest {
    public int Count { get; set; }
    public string? Script { get; set; }
    public override string ToString(){
        return "Count: "+Count+"\nScript: "+Script;
    }
}