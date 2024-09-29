using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetCoreMVCApp.Models;
using DotNetCoreMVCApp.Models.Entities;
using DotNetCoreMVCApp.Models.Repository;

namespace DotNetCoreMVCApp.Repository.Implementation
{
    public class UnitOfWork : IDisposable
    {
        private ApplicationDbContext _context;
        private GenericRepository<Customer> _customerRepository;
        private GenericRepository<Supplier> _supplierRepository;
        private GenericRepository<ProjectDatabase> _projectdatabaseRepository;
        private GenericRepository<BankBalance> _bankbalanceRepository;
        private GenericRepository<CashIn> _cashinRepository;
        private GenericRepository<CashOut> _cashoutRepository;
        private GenericRepository<StartingBalance> _startingbalanceRepository;






        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;

        }
        public GenericRepository<Customer> CustomerRepository
        {
            get
            {

                if (_customerRepository == null)
                {
                    _customerRepository = new GenericRepository<Customer>(_context);
                }
                return _customerRepository;
            }
        }
        public GenericRepository<Supplier> SupplierRepository
        {
            get
            {

                if (_supplierRepository == null)
                {
                    _supplierRepository = new GenericRepository<Supplier>(_context);
                }
                return _supplierRepository;
            }
        }
        public GenericRepository<ProjectDatabase> ProjectDatabaseRepository
        {
            get
            {

                if (_projectdatabaseRepository == null)
                {
                    _projectdatabaseRepository = new GenericRepository<ProjectDatabase>(_context);
                }
                return _projectdatabaseRepository;
            }
        }
        public GenericRepository<BankBalance> BankBalanceRepository
        {
            get
            {

                if (_bankbalanceRepository == null)
                {
                    _bankbalanceRepository = new GenericRepository<BankBalance>(_context);
                }
                return _bankbalanceRepository;
            }
        }
        public GenericRepository<StartingBalance> StartingBalanceRepository
        {
            get
            {

                if (_startingbalanceRepository == null)
                {
                    _startingbalanceRepository = new GenericRepository<StartingBalance>(_context);
                }
                return _startingbalanceRepository;
            }
        }





        public GenericRepository<CashIn> CashInRepository
        {
            get
            {

                if (_cashinRepository == null)
                {
                    _cashinRepository = new GenericRepository<CashIn>(_context);
                }
                return _cashinRepository;
            }
        }
        public GenericRepository<CashOut> CashOutRepository
        {
            get
            {

                if (_cashoutRepository == null)
                {
                    _cashoutRepository = new GenericRepository<CashOut>(_context);
                }
                return _cashoutRepository;
            }
        }
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

}
