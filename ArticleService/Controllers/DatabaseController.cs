using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ArticleService.database;

namespace ArticleService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly Database _database;

    public DatabaseController(Database database)
    {
        _database = database;
    }

    [HttpDelete]
    public async Task Delete()
    {
        await _database.DeleteDatabase();
    }

    [HttpPost]
    public async Task Post()
    {
        await _database.RecreateDatabase();
    }
}