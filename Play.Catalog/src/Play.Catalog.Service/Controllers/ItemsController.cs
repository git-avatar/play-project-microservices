using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Entities;
using Play.Common;

namespace Play.Catalog.Service.Controllers;

[ApiController]
[Route("items")]
public class ItemsController : ControllerBase
{
    private readonly IRepository<Item> _itemsRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    
    private static int requestCounter = 0;

    public ItemsController(IRepository<Item> itemsRepository, IPublishEndpoint publishEndpoint)
    {
        _itemsRepository = itemsRepository;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync()
    {
        // The commented code simulates temporary failures!
        
        // requestCounter++;
        // Console.WriteLine($"Request {requestCounter}: Starting...");
        //
        // if (requestCounter <= 2)
        // {
        //     Console.WriteLine($"Request {requestCounter}: Delaying...");
        //     await Task.Delay(TimeSpan.FromSeconds(10));
        // }
        //
        // if (requestCounter <= 4)
        // {
        //     Console.WriteLine($"Request {requestCounter}: (t)ERROR 500...");
        //     return StatusCode(500);
        // }
        var items = (await _itemsRepository.GetAllAsync())
            .Select(x => x.AsDto());
        //Console.WriteLine($"Request {requestCounter}: This is fine!");
        
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
    {
        var item = await _itemsRepository.GetAsync(id);
        if (item == null)
        {
            return NotFound();
        }
        return item.AsDto();
    }

    [HttpPost]
    public async Task<ActionResult<ItemDto>> PostAsync(CreateItemDto dto)
    {
        var item = new Item
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            CreatedDate = DateTime.UtcNow
        };

        await _itemsRepository.CreateAsync(item);

        await _publishEndpoint.Publish(new Contracts.Contracts.CatalogItemCreated(item.Id, item.Name, item.Description));

        return Ok(item);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ItemDto>> PutAsync(Guid id, UpdateItemDto dto)
    {
        var itemToUpdate = await _itemsRepository.GetAsync(id);

        if (itemToUpdate == null)
        {
            return NotFound();
        }

        itemToUpdate.Name = dto.Name;
        itemToUpdate.Description = dto.Description;
        itemToUpdate.Price = dto.Price;

        await _itemsRepository.UpdateAsync(itemToUpdate);
        
        await _publishEndpoint.Publish(new Contracts.Contracts.CatalogItemUpdated(itemToUpdate.Id, itemToUpdate.Name, itemToUpdate.Description));
        
        return NoContent();
    }

    [HttpDelete("id")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var itemToRemove = await _itemsRepository.GetAsync(id);

        if (itemToRemove == null)
        {
            return NotFound();
        }

        await _itemsRepository.RemoveAsync(itemToRemove.Id);
        
        await _publishEndpoint.Publish(new Contracts.Contracts.CatalogItemDeleted(itemToRemove.Id));

        return NoContent();
    }
}