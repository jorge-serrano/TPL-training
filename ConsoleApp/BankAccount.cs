using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class BankAccount
    {
        private object _padlock = new object();
        private int _balance;

        public int Balance
        {
            get => _balance;
            private set => _balance = value;
        }

        public void Deposit(int amount)
        {
            Balance+=amount;
        }
        public void Transfer(BankAccount where, int amount)
        {
            where.Balance += amount;
            Balance -= amount;
        }
        public void Withdraw(int amount)
        {
            Balance-=amount;
        }
        public void DepositV1(int amount)
        {

            lock (_padlock)
            {
                // += is really two operations
                // op1 is temp <- get_Balance() + amount
                // op2 is set_Balance(temp)
                // something can happen _between_ op1 and op2

                Balance += amount;
            }
        }

        public void WithdrawV1(int amount)
        {
            lock (_padlock)
            {
                Balance -= amount;
            }
        }
        public void DepositV2(int amount)
        {
            Interlocked.Add(ref _balance, amount);
        }

        public void WithdrawV2(int amount)
        {
            Interlocked.Add(ref _balance, -amount);
        }
    }
}
