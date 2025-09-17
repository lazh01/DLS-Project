using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ArticleService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseController : ControllerBase
{
    private Database database = Database.GetInstance();
    
    [HttpDelete]
    public async Task Delete()
    {
        await database.DeleteDatabase();
    }

    [HttpPost]
    public async Task Post()
    {
        await database.RecreateDatabase();
    }
}