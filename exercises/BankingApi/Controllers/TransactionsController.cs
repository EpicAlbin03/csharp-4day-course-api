using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BankingApi.Data;
using BankingApi.Models;
using BankingApi.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace BankingApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TransactionsController(AppDbContext context)
        {
            _context = context;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated request without a user id claim.");

        private static IQueryable<Transaction> ApplyFilters(IQueryable<Transaction> q, TransactionQuery query)
        {
            if (query.Type.HasValue)
            {
                q = q.Where(t => t.Type == query.Type.Value);
            }

            if (query.MinAmount.HasValue)
            {
                q = q.Where(t => t.Amount >= query.MinAmount.Value);
            }

            if (query.Since.HasValue)
            {
                q = q.Where(t => t.Timestamp >= query.Since.Value);
            }

            return q;
        }

        // GET: api/Transactions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionResponse>>> GetTransactions(
            [FromQuery] TransactionQuery query)
        {
            var userId = GetUserId();
            var q = _context.Transactions.Where(t => t.Account!.OwnerId == userId).AsQueryable();
            q = ApplyFilters(q, query);

            var list = await q.ToListAsync();
            return list.Select(TransactionResponse.FromEntity).ToList();
        }

        // GET: api/Transactions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionResponse>> GetTransaction(int id, [FromQuery] TransactionQuery query)
        {
            var userId = GetUserId();
            var q = _context.Transactions.Where(t => t.Account!.OwnerId == userId).AsQueryable();
            q = ApplyFilters(q, query);

            var transaction = await q.FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                return NotFound();
            }

            return TransactionResponse.FromEntity(transaction);
        }

        // PUT: api/Transactions/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTransaction(int id, Transaction transaction)
        {
            if (id != transaction.Id)
            {
                return BadRequest();
            }

            var userId = GetUserId();
            var existing = await _context.Transactions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id && t.Account!.OwnerId == userId);
            if (existing is null)
            {
                return NotFound();
            }

            _context.Entry(transaction).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TransactionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Transactions
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TransactionResponse>> PostTransaction(Transaction transaction)
        {
            var userId = GetUserId();
            var accountOwnedByUser = await _context.Accounts
                .AnyAsync(a => a.Id == transaction.AccountId && a.OwnerId == userId);
            if (!accountOwnedByUser)
            {
                return Problem(
                    detail: $"AccountId {transaction.AccountId} does not exist or does not belong to you.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTransaction", new { id = transaction.Id },
                TransactionResponse.FromEntity(transaction));
        }

        // DELETE: api/Transactions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var userId = GetUserId();
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == id && t.Account!.OwnerId == userId);
            if (transaction == null)
            {
                return NotFound();
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.Id == id);
        }
    }
}
