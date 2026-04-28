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
    public class AccountsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AccountsController(AppDbContext context)
        {
            _context = context;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated request without a user id claim.");

        private static HashSet<string> ParseInclude(string? include) =>
            string.IsNullOrWhiteSpace(include)
                ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                : include.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

        private static IQueryable<Account> ApplyFilters(IQueryable<Account> q, AccountQuery query)
        {
            var includes = ParseInclude(query.Include);

            if (includes.Contains("transactions"))
            {
                if (query.Type.HasValue)
                {
                    q = q.Include(a => a.Transactions!.Where(t => t.Type == query.Type.Value));
                }
                else
                {
                    q = q.Include(a => a.Transactions);
                }
            }

            if (includes.Contains("branch"))
            {
                q = q.Include(a => a.Branch);
            }

            return q;
        }

        // GET: api/Accounts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccountResponse>>> GetAccounts([FromQuery] AccountQuery query)
        {
            var userId = GetUserId();
            var q = _context.Accounts.Where(a => a.OwnerId == userId).AsQueryable();
            q = ApplyFilters(q, query);

            var list = await q.ToListAsync();
            return list.Select(AccountResponse.FromEntity).ToList();
        }

        // GET: api/Accounts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AccountResponse>> GetAccount(int id, [FromQuery] AccountQuery query)
        {
            var userId = GetUserId();
            var q = _context.Accounts.Where(a => a.OwnerId == userId).AsQueryable();
            q = ApplyFilters(q, query);

            var account = await q.FirstOrDefaultAsync(a => a.Id == id);

            if (account == null)
            {
                return NotFound();
            }

            return AccountResponse.FromEntity(account);
        }

        // PUT: api/Accounts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAccount(int id, Account account)
        {
            if (id != account.Id)
            {
                return BadRequest();
            }

            var userId = GetUserId();
            var existing = await _context.Accounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id && a.OwnerId == userId);
            if (existing is null)
            {
                return NotFound();
            }

            account.OwnerId = userId;
            account.CreatedAt = existing.CreatedAt;

            _context.Entry(account).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccountExists(id))
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

        // POST: api/Accounts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<AccountResponse>> PostAccount(Account account)
        {
            var userId = GetUserId();
            account.OwnerId = userId;
            account.AccountNumber = $"ACC-{_context.Accounts.Count() + 1000}";

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAccount", new { id = account.Id }, AccountResponse.FromEntity(account));
        }

        // DELETE: api/Accounts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            var userId = GetUserId();
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == id && a.OwnerId == userId);
            if (account == null)
            {
                return NotFound();
            }

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AccountExists(int id)
        {
            return _context.Accounts.Any(e => e.Id == id);
        }

        public record TransferRequest(int TargetAccountId, decimal Amount, string Description);

        [HttpPost("{id}/transfer")]
        public async Task<ActionResult<IEnumerable<TransactionResponse>>> Transfer(int id, TransferRequest request)
        {
            if (request.Amount <= 0)
            {
                return BadRequest("Amount must be positive.");
            }

            var source = await _context.Accounts
                .Include(a => a.Transactions)
                .FirstOrDefaultAsync(a => a.Id == id);
            var target = await _context.Accounts.FindAsync(request.TargetAccountId);

            if (source is null || target is null)
            {
                return NotFound();
            }

            var sourceBalance =
                source.Transactions!.Where(t => t.Type == TransactionType.Credit).Sum(t => t.Amount)
                - source.Transactions!.Where(t => t.Type == TransactionType.Debit).Sum(t => t.Amount);

            if (sourceBalance < request.Amount)
            {
                return Problem(
                    detail: "Insufficient funds.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            var debit = new Transaction
            {
                Type = TransactionType.Debit,
                Amount = request.Amount,
                Description = $"Transfer to {target.AccountNumber}: {request.Description}",
                AccountId = source.Id
            };
            var credit = new Transaction
            {
                Type = TransactionType.Credit,
                Amount = request.Amount,
                Description = $"Transfer from {source.AccountNumber}: {request.Description}",
                AccountId = target.Id
            };

            _context.Transactions.AddRange(debit, credit);
            await _context.SaveChangesAsync();

            return Ok(new[] { TransactionResponse.FromEntity(debit), TransactionResponse.FromEntity(credit) });
        }
    }
}
