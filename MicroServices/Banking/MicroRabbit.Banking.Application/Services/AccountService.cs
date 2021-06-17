using MicroRabbit.Banking.Application.Interfaces;
using MicroRabbit.Banking.Domain.Interfaces;
using MicroRabbit.Banking.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MicroRabbit.Banking.Application.Services
{
    public class AccountService : IAccountService
    {

        private readonly IAccountRepository _accountRepostory;

        public AccountService(IAccountRepository accountRepository)
        {
            this._accountRepostory = accountRepository;
        }

        public async Task<IEnumerable<Account>> GetAccounts()
        {
            return await this._accountRepostory.GetAccounts();
        }
    }
}
