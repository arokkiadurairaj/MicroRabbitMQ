using MicroRabbit.Banking.Domain.Interfaces;
using MicroRabbit.Banking.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroRabbit.Banking.Data.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private BankingDBContext _ctx;
        public AccountRepository(BankingDBContext ctx)
        {
            _ctx = ctx;
        }
        public async Task<IEnumerable<Account>> GetAccounts()
        {
            return await this._ctx.Accounts.ToListAsync();
        }
    }
}
