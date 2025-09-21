using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CommentService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseController : ControllerBase
{
    private Database database = Database.GetInstance();
    
    [HttpDelete]
    public async Task Delete()
    {
        await database.DeleteDatabase();
        Console.WriteLine("Comments table deleted.");
    }

    [HttpPost]
    public async Task Post()
    {
        await database.RecreateDatabase();
        Console.WriteLine("Comments table created.");
    }
}