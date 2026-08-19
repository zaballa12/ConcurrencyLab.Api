using ConcurrencyLab.Api.Data;
using ConcurrencyLab.Api.DTOs;
using ConcurrencyLab.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConcurrencyLab.Api.Controllers;

[ApiController]
[Route("products")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    // returns all products
    public async Task<ActionResult<List<Product>>> GetAll()
    {
        var products = await _context.Products
            .OrderBy(product => product.Id)
            .ToListAsync();

        return Ok(products);
    }

    [HttpDelete]
    // deletes all products
    public async Task<IActionResult> DeleteAll()
    {
        await _context.Products.ExecuteDeleteAsync();

        return NoContent();
    }

    [HttpPost]
    // creates a new product
    public async Task<ActionResult<Product>> Create(CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { error = "name_required" });
        }

        if (request.Stock < 0)
        {
            return BadRequest(new { error = "stock_must_be_zero_or_more" });
        }

        var product = new Product
        {
            Name = request.Name,
            Stock = request.Stock,
            Version = Guid.NewGuid()
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpGet("{id:int}")]
    // returns a product by its ID
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPost("{id:int}/stock")]
    // adds stock to a product
    public async Task<ActionResult<Product>> AddStock(int id, AddStockRequest request)
    {
        if (request.Quantity <= 0)
        {
            return BadRequest(new { error = "quantity_must_be_greater_than_zero" });
        }

        var product = await _context.Products.FindAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        product.Stock += request.Quantity;
        product.Version = Guid.NewGuid();

        await _context.SaveChangesAsync();

        return Ok(product);
    }

    [HttpPost("{id:int}/reserve-naive")]
    // reserves stock for a product without handling concurrency issues
    public async Task<ActionResult<Product>> ReserveStockNaive(int id, ReserveStockRequest request)
    {
        if (request.Quantity <= 0)
        {
            return BadRequest(new { error = "quantity_must_be_greater_than_zero" });
        }

        var product = await _context.Products.FindAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        if (product.Stock < request.Quantity)
        {
            return BadRequest(new { error = "insufficient_stock" });
        }

        product.Stock -= request.Quantity;
        product.Version = Guid.NewGuid();

        // Simulate a delay to demonstrate concurrency issues
        await Task.Delay(3000);
        
        await _context.SaveChangesAsync();

        return Ok(product);
    }

    [HttpPost("{id:int}/reserve")]
    // reserves stock for a product with concurrency handling
    public async Task<ActionResult<Product>> ReserveStock(int id, ReserveStockRequest request)
    {
        if (request.Quantity <= 0)
        {
            return BadRequest(new { error = "quantity_must_be_greater_than_zero" });
        }

        var product = await _context.Products.FindAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        if (product.Stock < request.Quantity)
        {
            return BadRequest(new { error = "insufficient_stock" });
        }

        product.Stock -= request.Quantity;
        product.Version = Guid.NewGuid();

        try
        {
            await Task.Delay(3000);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new
            {
                error = "stock_conflict",
                message = "O estoque foi alterado por outra requisicao."
            });
        }

        return Ok(product);
    }
}
